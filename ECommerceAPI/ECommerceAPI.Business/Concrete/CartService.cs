using ECommerceAPI.Business.Abstract;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos.CartDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Enums;
using ECommerceAPI.Entity.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Business.Concrete;

public class CartService : ICartService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerService _customerService;

    public CartService(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ICustomerService customerService)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _customerService = customerService;
    }

    #region Helper Methods
    private async Task<Address> GetAddressOrThrowAsync(
    int addressId,
    int customerId,
    CancellationToken cancellationToken)
    {
        var address = await _unitOfWork
            .Repository<Address>()
            .GetAsync(
                x => x.AddressId == addressId &&
                     x.CustomerId == customerId,
                cancellationToken);

        if (address is null)
        {
            throw new NotFoundException(
                "Adres bulunamadı veya bu kullanıcıya ait değil.");
        }

        return address;
    }
    private async Task<CartItem> GetCartItemOrThrowAsync(
    int cartId,
    int productId,
    CancellationToken cancellationToken)
    {
        var cartItem = await _unitOfWork
            .Repository<CartItem>()
            .GetAsync(
                x => x.CartId == cartId &&
                     x.ProductId == productId,
                cancellationToken);

        if (cartItem is null)
        {
            throw new NotFoundException(
                "Ürün sepette bulunamadı.");
        }

        return cartItem;
    }

    private async Task<Customer> GetCurrentCustomerAsync(
        CancellationToken cancellationToken)
    {
        var appUserId = _currentUserService.GetAppUserId();

        return await _customerService.GetByAppUserIdAsync(
            appUserId,
            cancellationToken);
    }

    private async Task<Cart?> GetCartAsync(
        int customerId,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork
            .Repository<Cart>()
            .GetAsync(
                x => x.CustomerId == customerId,
                cancellationToken);
    }

    private async Task<Product> GetProductOrThrowAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Product>();

        var product = await repository.GetAsync(
            x => x.ProductId == productId,
            cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(
                "Ürün bulunamadı.");
        }

        return product;
    }

    #endregion

    public async Task AddToCartAsync(
        AddToCartDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Quantity <= 0)
        {
            throw new BadRequestException(
                "Ürün miktarı 0'dan büyük olmalıdır.");
        }

        var cartRepository = _unitOfWork.Repository<Cart>();
        var cartItemRepository = _unitOfWork.Repository<CartItem>();

        var customer =
            await GetCurrentCustomerAsync(cancellationToken);

        var product =
            await GetProductOrThrowAsync(
                dto.ProductId,
                cancellationToken);

        if (dto.Quantity > product.Stock)
        {
            throw new BadRequestException(
                "İstenen ürün miktarı stok miktarını aşamaz.");
        }

        var cart = await GetCartAsync(
            customer.CustomerId,
            cancellationToken);

        if (cart is null)
        {
            cart = new Cart
            {
                CustomerId = customer.CustomerId
            };

            var newCartItem = new CartItem
            {
                Cart = cart,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };

            await cartRepository.AddAsync(
                cart,
                cancellationToken);

            await cartItemRepository.AddAsync(
                newCartItem,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return;
        }

        var cartItem = await cartItemRepository.GetAsync(
            x =>
                x.CartId == cart.CartId &&
                x.ProductId == dto.ProductId,
            cancellationToken);

        if (cartItem is not null)
        {
            var newQuantity =
                cartItem.Quantity + dto.Quantity;

            if (newQuantity > product.Stock)
            {
                throw new BadRequestException(
                    "Sepetteki toplam ürün miktarı stok miktarını aşamaz.");
            }

            cartItem.Quantity = newQuantity;
        }
        else
        {
            var newCartItem = new CartItem
            {
                CartId = cart.CartId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };

            await cartItemRepository.AddAsync(
                newCartItem,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
    public async Task CheckoutAsync(
    CheckoutDto dto,
    CancellationToken cancellationToken = default)
    {
        var orderRepository = _unitOfWork.Repository<Order>();
        var orderDetailRepository = _unitOfWork.Repository<OrderDetail>();
        var productRepository = _unitOfWork.Repository<Product>();
        var cartItemRepository = _unitOfWork.Repository<CartItem>();
        var cartRepository = _unitOfWork.Repository<Cart>();

        var customer =
            await GetCurrentCustomerAsync(cancellationToken);

        var address =
            await GetAddressOrThrowAsync(
                dto.AddressId,
                customer.CustomerId,
                cancellationToken);

        await _unitOfWork.BeginTransactionAsync(
            cancellationToken);

        try
        {
            var specification =
                new CartWithItemsAndProductsSpecification(
                    customer.CustomerId);

            var cart = await cartRepository.GetAsync(
                specification,
                cancellationToken);

            if (cart is null || !cart.CartItems.Any())
            {
                throw new BadRequestException(
                    "Sepet boş.");
            }

            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.Quantity > cartItem.Product.Stock)
                {
                    throw new BadRequestException(
                        $"{cartItem.Product.Name} ürünü için yeterli stok bulunmamaktadır.");
                }
            }

            var totalAmount = cart.CartItems.Sum(
                x => x.Product.Price * x.Quantity);

            var order = new Order
            {
                CustomerId = customer.CustomerId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,

                ShippingAddressTitle = address.Title,
                ShippingCity = address.City,
                ShippingDistrict = address.District,
                ShippingFullAddress = address.FullAddress
            };

            await orderRepository.AddAsync(
                order,
                cancellationToken);
            foreach (var cartItem in cart.CartItems)
            {
                var orderDetail = new OrderDetail
                {
                    Order = order,
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.Name,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.Price
                };

                await orderDetailRepository.AddAsync(
                    orderDetail,
                    cancellationToken);

                cartItem.Product.Stock -= cartItem.Quantity;

                productRepository.Update(
                    cartItem.Product);

                cartItemRepository.SoftDelete(
                    cartItem);
            }

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
                "Stok bilgisi başka bir işlem tarafından değiştirildi. Lütfen sepetinizi yenileyip tekrar deneyin.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);

            throw;
        }
    }
    public async Task<CartDto> GetMyCartAsync(
    CancellationToken cancellationToken = default)
    {
        var cartRepository = _unitOfWork.Repository<Cart>();

        var customer =
            await GetCurrentCustomerAsync(cancellationToken);

        var specification =
            new CartWithItemsAndProductsSpecification(
                customer.CustomerId);

        var cart = await cartRepository.GetAsync(
            specification,
            cancellationToken);

        if (cart is null)
        {
            return new CartDto
            {
                Items = new List<CartItemDto>(),
                TotalItemCount = 0,
                TotalAmount = 0
            };
        }

        var items = cart.CartItems
            .Select(x => new CartItemDto
            {
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                UnitPrice = x.Product.Price,
                Quantity = x.Quantity,
                LineTotal = x.Product.Price * x.Quantity
            })
            .ToList();

        return new CartDto
        {
            CartId = cart.CartId,
            Items = items,
            TotalItemCount = items.Sum(x => x.Quantity),
            TotalAmount = items.Sum(x => x.LineTotal)
        };
    }
    public async Task RemoveCartItemAsync(
    int productId,
    CancellationToken cancellationToken = default)
    {
        var cartItemRepository =
            _unitOfWork.Repository<CartItem>();

        var customer =
            await GetCurrentCustomerAsync(cancellationToken);

        var cart = await GetCartAsync(
            customer.CustomerId,
            cancellationToken);

        if (cart is null)
        {
            throw new NotFoundException(
                "Sepet bulunamadı.");
        }

        var cartItem = await GetCartItemOrThrowAsync(
            cart.CartId,
            productId,
            cancellationToken);

        cartItemRepository.SoftDelete(cartItem);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
    public async Task UpdateCartItemAsync(
    UpdateCartItemDto dto,
    CancellationToken cancellationToken = default)
    {

        var cartItemRepository =
            _unitOfWork.Repository<CartItem>();

        var customer =
            await GetCurrentCustomerAsync(cancellationToken);

        if (dto.Quantity < 0)
        {
            throw new BadRequestException(
                "Ürün miktarı negatif olamaz.");
        }

        var cart = await GetCartAsync(
            customer.CustomerId,
            cancellationToken);

        if (cart is null)
        {
            throw new NotFoundException(
                "Sepet bulunamadı.");
        }

        var cartItem = await GetCartItemOrThrowAsync(
            cart.CartId,
            dto.ProductId,
            cancellationToken);

        if (dto.Quantity == 0)
        {
            cartItemRepository.SoftDelete(cartItem);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return;
        }

        var product = await GetProductOrThrowAsync(
            dto.ProductId,
            cancellationToken);

        if (dto.Quantity > product.Stock)
        {
            throw new BadRequestException(
                "İstenen ürün miktarı stok miktarını aşamaz.");
        }

        cartItem.Quantity = dto.Quantity;

        var result = await _unitOfWork.SaveChangesAsync(
            cancellationToken);


    }

}
