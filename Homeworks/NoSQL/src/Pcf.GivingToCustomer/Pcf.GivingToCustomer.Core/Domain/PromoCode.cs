using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Pcf.GivingToCustomer.Core.Domain
{
    public class PromoCode : BaseEntity
    {
        public string Code { get; set; }

        public string ServiceInfo { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public Guid PartnerId { get; set; }

        public Guid PreferenceId { get; set; }

        [BsonIgnore]
        public virtual Preference Preference { get; set; }

        [BsonIgnore]
        public virtual ICollection<PromoCodeCustomer> Customers { get; set; }

        public List<Guid> CustomerIds { get; set; } = new List<Guid>();
    }
}