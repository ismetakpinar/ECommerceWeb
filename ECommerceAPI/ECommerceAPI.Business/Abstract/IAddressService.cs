using ECommerceAPI.Dto.Dtos.AddressDtos;
using ECommerceAPI.Entity.Entities;

namespace ECommerceAPI.Business.Abstract;

public interface IAddressService
{
    Task<List<Address>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Address> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        CreateAddressDto dto,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        UpdateAddressDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}