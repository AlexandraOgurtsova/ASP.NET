using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;

namespace Pcf.GivingToCustomer.Core.Services
{
    public class PromoCodeDistributionService : IPromoCodeDistributionService
    {
        private readonly IRepository<PromoCode> _promoCodesRepository;
        private readonly IRepository<Preference> _preferencesRepository;
        private readonly IRepository<Customer> _customersRepository;

        public PromoCodeDistributionService(
            IRepository<PromoCode> promoCodesRepository,
            IRepository<Preference> preferencesRepository,
            IRepository<Customer> customersRepository)
        {
            _promoCodesRepository = promoCodesRepository;
            _preferencesRepository = preferencesRepository;
            _customersRepository = customersRepository;
        }

        public async Task DistributePromoCodeToCustomersAsync(
            Guid promoCodeId,
            string promoCode,
            string serviceInfo,
            Guid preferenceId,
            Guid partnerId,
            DateTime beginDate,
            DateTime endDate)
        {
            var preference = await _preferencesRepository.GetFirstWhere(p => p.Id == preferenceId);

            if (preference == null)
                throw new ArgumentException("Предпочтение не найдено", nameof(preferenceId));

            var customers = await _customersRepository
                .GetWhere(d => d.Preferences.Any(x => x.PreferenceId == preference.Id));

            var newPromoCode = new PromoCode
            {
                Id = promoCodeId,
                Code = promoCode,
                ServiceInfo = serviceInfo,
                BeginDate = beginDate,
                EndDate = endDate,
                PartnerId = partnerId,
                Preference = preference,
                PreferenceId = preferenceId,
                Customers = customers.Select(customer => new PromoCodeCustomer
                {
                    CustomerId = customer.Id,
                    Customer = customer,
                    PromoCodeId = promoCodeId
                }).ToList()
            };

            await _promoCodesRepository.AddAsync(newPromoCode);
        }
    }
}