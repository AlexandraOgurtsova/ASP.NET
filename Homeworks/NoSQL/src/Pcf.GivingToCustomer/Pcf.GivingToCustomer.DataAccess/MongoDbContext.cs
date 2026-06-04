using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Configuration;
using Pcf.GivingToCustomer.Core.Domain;

namespace Pcf.GivingToCustomer.DataAccess
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("Customers");
        public IMongoCollection<Preference> Preferences => _database.GetCollection<Preference>("Preferences");
        public IMongoCollection<PromoCode> PromoCodes => _database.GetCollection<PromoCode>("PromoCodes");
    }
}