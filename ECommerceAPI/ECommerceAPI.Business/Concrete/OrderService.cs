using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos.OrderDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Enums;
using ECommerceAPI.Entity.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Business.Concrete;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICustomerService _customerService;

    public OrderService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ICustomerService customerService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _customerService = customerService;
    }

    private async Task<Customer> GetCurrentCustomerAsync(
        CancellationToken cancellationToken)
    {
        var appUserId = _currentUserService.GetAppUserId();

        return await _customerService.GetByAppUserIdAsync(
            appUserId,
            cancellationToken);
    }

    private async Task<Order> GetOrderOrThrowAsync(
        int orderId,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Order>();

        var order = await repository.GetByIdAsync(
            orderId,
            cancellationToken);

        if (order is null)
            throw new NotFoundException("Sipariş bulunamadı.");

        return order;
    }

    public async Task<List<Order>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var specification = new OrderListSpecification();

        var repository = _unitOfWork.Repository<Order>();

        return await repository.GetAllAsync(
            specification,
            cancellationToken);
    }

    public async Task<Order> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var specification =
            new OrderByIdWithCustomerSpecification(id);

        var repository = _unitOfWork.Repository<Order>();

        var order = await repository.GetAsync(
            specification,
            cancellationToken);

        if (order is null)
            throw new NotFoundException("Sipariş bulunamadı.");

        return order;
    }

    public async Task<OrderDetailDto> GetMyOrderDetailAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var customer =
            await GetCurrentCustomerAsync(cancellationToken);

        var specification =
            new MyOrderDetailSpecification(
                orderId,
                customer.CustomerId);

        var repository = _unitOfWork.Repository<Order>();

        var order = await repository.GetAsync(
            specification,
            cancellationToken);

        if (order is null)
            throw new NotFoundException("Sipariş bulunamadı.");

        var items = order.OrderDetails
            .Select(x => new OrderItemDto
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                UnitPrice = x.UnitPrice,
                Quantity = x.Quantity,
                LineTotal = x.UnitPrice * x.Quantity
            })
            .ToList();

        return new OrderDetailDto
        {
            OrderId = order.OrderId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            CustomerId = customer.CustomerId,
            CustomerName =
                $"{customer.FirstName} {customer.LastName}",
            Items = items,
            TotalAmount = items.Sum(x => x.LineTotal),
            ShippingAddressTitle = order.ShippingAddressTitle,
            ShippingCity = order.ShippingCity,
            ShippingDistrict = order.ShippingDistrict,
            ShippingFullAddress = order.ShippingFullAddress,
        };
    }

    public async Task<List<OrderListDto>> GetMyOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        var customer =
            await GetCurrentCustomerAsync(cancellationToken);

        var specification =
            new GetMyOrdersSpecification(
                customer.CustomerId);

        var repository = _unitOfWork.Repository<Order>();

        var orders = await repository.GetAllAsync(
            specification,
            cancellationToken);

        return orders
            .Select(x => new OrderListDto
            {
                OrderId = x.OrderId,
                OrderDate = x.OrderDate,
                Status = x.Status,
                CustomerName =
            $"{customer.FirstName} {customer.LastName}",

                TotalAmount = x.TotalAmount
            })
                .ToList();
    }

    public async Task UpdateAsync(
        UpdateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Order>();

        var order = await GetOrderOrThrowAsync(
            dto.OrderId,
            cancellationToken);

        if (!IsValidStatusTransition(
            order.Status,
            dto.Status))
        {
            throw new BadRequestException(
                $"Sipariş durumu {order.Status} durumundan {dto.Status} durumuna geçirilemez.");
        }

        order.Status = dto.Status;

        repository.Update(order);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool IsValidStatusTransition(
        OrderStatus currentStatus,
        OrderStatus newStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending =>
                newStatus == OrderStatus.Preparing,

            OrderStatus.Preparing =>
                newStatus == OrderStatus.Shipped,

            OrderStatus.Shipped =>
                newStatus == OrderStatus.Delivered,

            OrderStatus.Delivered => false,

            OrderStatus.Cancelled => false,

            _ => false
        };
    }

    public async Task CancelOrderAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var specification =
            new OrderWithDetailsAndProductsSpecification(
                orderId);

        var orderRepository =
            _unitOfWork.Repository<Order>();

        var productRepository =
            _unitOfWork.Repository<Product>();

        var order = await orderRepository.GetAsync(
            specification,
            cancellationToken);

        if (order is null)
            throw new NotFoundException("Sipariş bulunamadı.");

        if (order.Status != OrderStatus.Pending &&
            order.Status != OrderStatus.Preparing)
        {
            throw new BadRequestException(
                $"{order.Status} durumundaki sipariş iptal edilemez.");
        }

        await _unitOfWork.BeginTransactionAsync(
            cancellationToken);

        try
        {
            foreach (var detail in order.OrderDetails)
            {
                detail.Product.Stock += detail.Quantity;
                productRepository.Update(detail.Product);
            }

            order.Status = OrderStatus.Cancelled;

            orderRepository.Update(order);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            await _unitOfWork.CommitTransactionAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);

            throw new BadRequestException(
                "Stok bilgisi başka bir işlem tarafından değiştirildi. Sipariş iptal işlemini tekrar deneyin.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);
            throw;
        }
    }
}