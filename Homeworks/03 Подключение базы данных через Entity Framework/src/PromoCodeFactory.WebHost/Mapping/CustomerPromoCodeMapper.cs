using PromoCodeFactory.WebHost.Models.Customers;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models.Employees;

namespace PromoCodeFactory.WebHost.Mapping;

public static class CustomerPromoCodeMapper
{
    public static CustomerPromoCode ToCustomerPromoCode(Guid customerId, Guid promoCodeId)
    {
        return new CustomerPromoCode
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PromoCodeId = promoCodeId,
            CreatedAt = DateTimeOffset.UtcNow,
            AppliedAt = null
        };
    }
}
