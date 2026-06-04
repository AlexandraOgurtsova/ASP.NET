using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.DataAccess;

namespace Pcf.GivingToCustomer.DataAccess.Data
{
    public class MongoDbInitializer : IDbInitializer
    {
        private readonly MongoDbContext _dbContext;

        public MongoDbInitializer(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void InitializeDb()
        {
            var database = _dbContext.Customers.Database;

            var collections = database.ListCollectionNames().ToList();

            if (collections.Contains("Customers"))
                database.DropCollection("Customers");

            if (collections.Contains("Preferences"))
                database.DropCollection("Preferences");

            if (collections.Contains("PromoCodes"))
                database.DropCollection("PromoCodes");

            CreateIndexes();

            SeedPreferences();
            SeedCustomers();
        }

        private void CreateIndexes()
        {
            var emailIndex = Builders<Customer>.IndexKeys.Ascending(x => x.Email);
            _dbContext.Customers.Indexes.CreateOne(new CreateIndexModel<Customer>(emailIndex,
                new CreateIndexOptions { Unique = true, Name = "IX_Email_Unique" }));

            var codeIndex = Builders<PromoCode>.IndexKeys.Ascending(x => x.Code);
            _dbContext.PromoCodes.Indexes.CreateOne(new CreateIndexModel<PromoCode>(codeIndex,
                new CreateIndexOptions { Unique = true, Name = "IX_Code_Unique" }));
        }

        private void SeedPreferences()
        {
            var preferences = FakeDataFactory.Preferences;
            _dbContext.Preferences.InsertMany(preferences);
        }

        private void SeedCustomers()
        {
            var customers = FakeDataFactory.Customers;
            _dbContext.Customers.InsertMany(customers);
        }
    }
}