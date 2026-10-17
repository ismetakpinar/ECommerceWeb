using AutoMapper;
using ECommerceAPI.Business.Abstract;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos.AddressDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Exceptions;

namespace ECommerceAPI.Business.Concrete;

public class AddressService : IAddressService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICustomerService _customerService;

    public AddressService(
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

    private static void Normalize(CreateAddressDto dto)
    {
        dto.Title = dto.Title.Trim();
        dto.City = dto.City.Trim();
        dto.District = dto.District.Trim();
        dto.FullAddress = dto.FullAddress.Trim();
    }

    private static void Normalize(UpdateAddressDto dto)
    {
        dto.Title = dto.Title.Trim();
        dto.City = dto.City.Trim();
        dto.District = dto.District.Trim();
        dto.FullAddress = dto.FullAddress.Trim();
    }

    private async Task<Customer> GetCurrentCustomerAsync(
        CancellationToken cancellationToken)
    {
        var appUserId = _currentUserService.GetAppUserId();

        return await _customerService.GetByAppUserIdAsync(
            appUserId,
            cancellationToken);
    }

    private async Task<Address> GetAddressOrThrowAsync(
        int addressId,
        int customerId,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Address>();

        var address = await repository.GetAsync(
            x =>
                x.AddressId == addressId &&
                x.CustomerId == customerId,
            cancellationToken);

        if (address is null)
        {
            throw new NotFoundException("Adres bulunamadı.");
        }

        return address;
    }

    private async Task<bool> AddressExistsAsync(
        int customerId,
        string title,
        string city,
        string district,
        string fullAddress,
        int? excludeAddressId,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Address>();

        return await repository.AnyAsync(
            x =>
                x.CustomerId == customerId &&
                (!excludeAddressId.HasValue ||
                 x.AddressId != excludeAddressId.Value) &&
                x.Title.ToLower() == title.ToLower() &&
                x.City.ToLower() == city.ToLower() &&
                x.District.ToLower() == district.ToLower() &&
                x.FullAddress.ToLower() == fullAddress.ToLower(),
            cancellationToken);
    }

    public async Task CreateAsync(
        CreateAddressDto dto,
        CancellationToken cancellationToken = default)
    {
        Normalize(dto);

        var repository = _unitOfWork.Repository<Address>();

        var customer = await GetCurrentCustomerAsync(cancellationToken);

        var addressExists = await AddressExistsAsync(
            customer.CustomerId,
            dto.Title,
            dto.City,
            dto.District,
            dto.FullAddress,
            null,
            cancellationToken);

        if (addressExists)
        {
            throw new BadRequestException("Bu adres zaten kayıtlı.");
        }

        var address = _mapper.Map<Address>(dto);

        address.CustomerId = customer.CustomerId;

        await repository.AddAsync(address, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Address>();

        var customer = await GetCurrentCustomerAsync(cancellationToken);

        var address = await GetAddressOrThrowAsync(
            id,
            customer.CustomerId,
            cancellationToken);

        repository.SoftDelete(address);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Address>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Address>();

        var customer = await GetCurrentCustomerAsync(cancellationToken);

        var specification =
            new AddressListSpecification(customer.CustomerId);

        return await repository.GetAllAsync(
            specification,
            cancellationToken);
    }

    public async Task<Address> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetCurrentCustomerAsync(cancellationToken);

        return await GetAddressOrThrowAsync(
            id,
            customer.CustomerId,
            cancellationToken);
    }

    public async Task UpdateAsync(
        UpdateAddressDto dto,
        CancellationToken cancellationToken = default)
    {
        Normalize(dto);

        var repository = _unitOfWork.Repository<Address>();

        var customer = await GetCurrentCustomerAsync(cancellationToken);

        var address = await GetAddressOrThrowAsync(
            dto.AddressId,
            customer.CustomerId,
            cancellationToken);

        var addressExists = await AddressExistsAsync(
            customer.CustomerId,
            dto.Title,
            dto.City,
            dto.District,
            dto.FullAddress,
            dto.AddressId,
            cancellationToken);

        if (addressExists)
        {
            throw new BadRequestException("Bu adres zaten kayıtlı.");
        }

        _mapper.Map(dto, address);

        repository.Update(address);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}