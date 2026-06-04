using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Pcf.GivingToCustomer.Core.Domain
{
    public class Customer : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; }

        [BsonIgnore]
        public virtual ICollection<CustomerPreference> Preferences { get; set; }

        [BsonIgnore]
        public virtual ICollection<PromoCodeCustomer> PromoCodes { get; set; }

        public List<Guid> PreferenceIds { get; set; } = new List<Guid>();

        public List<Guid> PromoCodeIds { get; set; } = new List<Guid>();
    }
}