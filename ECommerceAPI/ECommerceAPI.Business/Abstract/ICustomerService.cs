using ECommerceAPI.Dto.Dtos.CustomerDtos;

namespace ECommerceAPI.Business.Abstract;

public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Customer> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Customer> GetMeAsync(
        CancellationToken cancellationToken = default);

    Task<Customer> GetByAppUserIdAsync(
        int appUserId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        UpdateCustomerDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}