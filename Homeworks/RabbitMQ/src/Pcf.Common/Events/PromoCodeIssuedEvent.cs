using System;

namespace Pcf.Common.Events
{
    public class PromoCodeIssuedEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        public Guid PartnerId { get; set; }

        public Guid? PartnerManagerId { get; set; }

        public Guid PromoCodeId { get; set; }

        public string PromoCode { get; set; }

        public string ServiceInfo { get; set; }

        public Guid PreferenceId { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}