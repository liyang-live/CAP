﻿using System;
using DotNetCore.CAP.Processor;
using DotNetCore.CAP.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace DotNetCore.CAP
{
    public class SqlServerCapOptionsExtension : ICapOptionsExtension
    {
        private readonly Action<SqlServerOptions> _configure;

        public SqlServerCapOptionsExtension(Action<SqlServerOptions> configure)
        {
            _configure = configure;
        }

        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IStorage, SqlServerStorage>();
            services.AddScoped<IStorageConnection, SqlServerStorageConnection>();
            services.AddScoped<ICapPublisher, CapPublisher>();
            services.AddTransient<IAdditionalProcessor, DefaultAdditionalProcessor>();

            var sqlServerOptions = new SqlServerOptions();
            _configure(sqlServerOptions);

            var provider = TempBuildService(services);
            var dbContextObj = provider.GetService(sqlServerOptions.DbContextType);
            if (dbContextObj != null)
            {
                var dbContext = (DbContext)dbContextObj;
                sqlServerOptions.ConnectionString = dbContext.Database.GetDbConnection().ConnectionString;
            }
            services.Configure(_configure);
            services.AddSingleton(sqlServerOptions);
        }

        private IServiceProvider TempBuildService(IServiceCollection services)
        {
            return services.BuildServiceProvider();
        }
    }
}