using PromoCodeFactory.WebHost.Models.Customers;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models.Employees;

namespace PromoCodeFactory.WebHost.Mapping;

public static class CustomerMapper
{
    public static CustomerResponse ToCustomerResponse(Customer customer, IEnumerable<PromoCode> promoCodes)
    {
        return new CustomerResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Preferences.Select(PreferencesMapper.ToPreferenceShortResponse).ToList(),
            promoCodes.Select(pc => PromoCodesMapper.ToCustomerPromoCodeResponse(pc, customer)).ToList());
    }

    public static CustomerShortResponse ToCustomerShortResponce(Customer customer)
    {
        return new CustomerShortResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Preferences.Select(PreferencesMapper.ToPreferenceShortResponse).ToList());
    }

    public static Customer ToCustomer(CustomerCreateRequest request, IEnumerable<Preference> preferences)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Preferences = preferences.ToList()
        };
    }
}
