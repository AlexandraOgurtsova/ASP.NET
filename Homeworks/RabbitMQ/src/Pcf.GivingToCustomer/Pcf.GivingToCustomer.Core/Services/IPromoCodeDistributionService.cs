using System;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.Core.Services
{
    public interface IPromoCodeDistributionService
    {
        Task DistributePromoCodeToCustomersAsync(
            Guid promoCodeId,
            string promoCode,
            string serviceInfo,
            Guid preferenceId,
            Guid partnerId,
            DateTime beginDate,
            DateTime endDate);
    }
}