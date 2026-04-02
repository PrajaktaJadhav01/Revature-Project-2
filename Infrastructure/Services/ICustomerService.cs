using CustomerManagementSystem.Domain.Entities;

namespace CustomerManagementSystem.Infrastructure.Services;

public interface ICustomerService
{
    Task<Customer> CreateCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Customer?> UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<bool> DeleteCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetCustomersAsync(int pageNumber, int pageSize, int? assignedSalesRepId, CancellationToken cancellationToken = default);
    Task<Customer?> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Contact> AddContactAsync(Contact contact, CancellationToken cancellationToken = default);
    Task<Contact?> UpdateContactAsync(Contact contact, CancellationToken cancellationToken = default);
    Task<bool> DeleteContactAsync(int contactId, CancellationToken cancellationToken = default);

    Task<Address> AddAddressAsync(Address address, CancellationToken cancellationToken = default);
    Task<Address?> UpdateAddressAsync(Address address, CancellationToken cancellationToken = default);
    Task<bool> DeleteAddressAsync(int addressId, CancellationToken cancellationToken = default);

    Task<Interaction> AddInteractionAsync(Interaction interaction, CancellationToken cancellationToken = default);
    Task<Interaction?> UpdateInteractionAsync(Interaction interaction, CancellationToken cancellationToken = default);
    Task<bool> DeleteInteractionAsync(int interactionId, CancellationToken cancellationToken = default);

    Task<bool> ChangeClassificationAsync(int customerId, CustomerClassification classification, CancellationToken cancellationToken = default);

    Task<bool> HasActiveInvoicesAsync(int customerId, CancellationToken cancellationToken = default);

    Task<bool> IsDuplicateCustomerAsync(string email, string customerName, int? excludeCustomerId = null, CancellationToken cancellationToken = default);

    Task<int?> GetAssignedSalesRepIdAsync(int customerId, CancellationToken cancellationToken = default);
}

