using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.DataAccess.Data;
using System;
using System.Collections.Generic;

namespace Pcf.GivingToCustomer.IntegrationTests.Data
{
    public class MongoDbTestInitializer : IDbInitializer
    {
        private readonly MongoDbContext _dbContext;

        public MongoDbTestInitializer(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void InitializeDb()
        {
            CleanDb();

            CreateIndexes();

            SeedPreferences();
            SeedCustomers();
        }

        public void CleanDb()
        {
            var database = _dbContext.Customers.Database;
            var collections = database.ListCollectionNames().ToList();

            if (collections.Contains("Customers"))
                database.DropCollection("Customers");

            if (collections.Contains("Preferences"))
                database.DropCollection("Preferences");

            if (collections.Contains("PromoCodes"))
                database.DropCollection("PromoCodes");
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
            var preferences = TestDataFactory.Preferences;
            if (preferences.Count > 0)
                _dbContext.Preferences.InsertMany(preferences);
        }

        private void SeedCustomers()
        {
            var customers = TestDataFactory.Customers;
            foreach (var customer in customers)
            {
                _dbContext.Customers.InsertOne(customer);
            }
        }
    }
}