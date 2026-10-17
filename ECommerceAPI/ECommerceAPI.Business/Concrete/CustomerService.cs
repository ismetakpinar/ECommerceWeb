using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Specifications.CustomerSpecifications;
using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos.CustomerDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Exceptions;

namespace ECommerceAPI.Business.Concrete;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CustomerService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    private static void NormalizeUpdateDto(UpdateCustomerDto dto)
    {
        dto.FirstName = dto.FirstName.Trim();
        dto.LastName = dto.LastName.Trim();
        dto.Phone = dto.Phone.Trim();
    }

    private async Task<Customer> GetCurrentCustomerOrThrowAsync(
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Customer>();

        var appUserId = _currentUserService.GetAppUserId();

        var customer = await repository.GetAsync(
            x => x.AppUserId == appUserId,
            cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(
                "Kullanıcıya ait müşteri kaydı bulunamadı.");
        }

        return customer;
    }

    public async Task<List<Customer>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Customer>();

        return await repository.GetAllAsync(cancellationToken);
    }

    public async Task<Customer> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var specification =
            new CustomerByIdWithDetailsSpecification(id);

        var repository = _unitOfWork.Repository<Customer>();

        var customer = await repository.GetAsync(
            specification,
            cancellationToken);

        if (customer is null)
            throw new NotFoundException("Müşteri bulunamadı.");

        return customer;
    }

    public async Task<Customer> GetByAppUserIdAsync(
        int appUserId,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Customer>();

        var customer = await repository.GetAsync(
            x => x.AppUserId == appUserId,
            cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(
                "Kullanıcıya ait müşteri kaydı bulunamadı.");
        }

        return customer;
    }

    public async Task<Customer> GetMeAsync(
        CancellationToken cancellationToken = default)
    {
        var appUserId = _currentUserService.GetAppUserId();

        var specification =
            new CustomerWithDetailsSpecification(appUserId);

        var repository = _unitOfWork.Repository<Customer>();

        var customer = await repository.GetAsync(
            specification,
            cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(
                "Kullanıcıya ait müşteri kaydı bulunamadı.");
        }

        return customer;
    }

    public async Task UpdateAsync(
        UpdateCustomerDto dto,
        CancellationToken cancellationToken = default)
    {
        NormalizeUpdateDto(dto);

        var repository = _unitOfWork.Repository<Customer>();

        var customer =
            await GetCurrentCustomerOrThrowAsync(cancellationToken);

        _mapper.Map(dto, customer);

        repository.Update(customer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var customerRepository =
            _unitOfWork.Repository<Customer>();

        var orderRepository =
            _unitOfWork.Repository<Order>();

        var customer = await customerRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (customer is null)
            throw new NotFoundException("Müşteri bulunamadı.");

        var hasOrders = await orderRepository.AnyAsync(
            x => x.CustomerId == id,
            cancellationToken);

        if (hasOrders)
        {
            throw new BadRequestException(
                "Siparişi bulunan müşteri silinemez.");
        }

        customerRepository.SoftDelete(customer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}