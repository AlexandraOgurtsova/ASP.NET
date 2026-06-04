using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Configuration;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.IntegrationTests.Data;
using System;
using System.IO;

namespace Pcf.GivingToCustomer.IntegrationTests
{
    public class MongoDatabaseFixture : IDisposable
    {
        private readonly MongoDbTestInitializer _mongoDbTestInitializer;
        private readonly MongoDbSettings _settings;

        public MongoDatabaseFixture()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .Build();

            _settings = new MongoDbSettings();
            configuration.GetSection("MongoDbSettings").Bind(_settings);

            var testDatabaseName = $"{_settings.DatabaseName}";
            _settings.DatabaseName = testDatabaseName;

            DbContext = new MongoDbContext(_settings);

            _mongoDbTestInitializer = new MongoDbTestInitializer(DbContext);
            _mongoDbTestInitializer.InitializeDb();
        }

        public void Dispose()
        {
            _mongoDbTestInitializer?.CleanDb();
        }

        public MongoDbContext DbContext { get; private set; }
    }
}