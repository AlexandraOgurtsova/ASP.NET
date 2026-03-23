using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models.Customers;
using PromoCodeFactory.WebHost.Models.PromoCodes;

namespace PromoCodeFactory.WebHost.Mapping;

public static class PromoCodesMapper
{
    public static PromoCodeShortResponse ToPromoCodeShortResponse(PromoCode promoCode)
    {
        return new PromoCodeShortResponse(
            promoCode.Id,
            promoCode.Code,
            promoCode.ServiceInfo,
            promoCode.PartnerName,
            promoCode.BeginDate,
            promoCode.EndDate,
            promoCode.PartnerManager.Id,
            promoCode.Preference.Id);
    }

    public static CustomerPromoCodeResponse ToCustomerPromoCodeResponse(PromoCode promoCode, Customer customer)
    {
        var customerPromoCode = customer.CustomerPromoCodes?
            .FirstOrDefault(cp => cp.PromoCodeId == promoCode.Id);

        return new CustomerPromoCodeResponse(
            promoCode.Id,
            promoCode.Code,
            promoCode.ServiceInfo,
            promoCode.PartnerName,
            promoCode.BeginDate,
            promoCode.EndDate,
            promoCode.PartnerManager.Id,
            promoCode.Preference.Id,
            customerPromoCode?.CreatedAt ?? DateTimeOffset.UtcNow,
            customerPromoCode?.AppliedAt
        );
    }

    public static PromoCode ToPromoCode(PromoCodeCreateRequest request, Employee partnerManager, Preference preference)
    {
        return new PromoCode
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            ServiceInfo = request.ServiceInfo,
            BeginDate = request.BeginDate,
            EndDate = request.EndDate,
            PartnerName = request.PartnerName,
            PartnerManager = partnerManager,
            Preference = preference
        };
    }
}
