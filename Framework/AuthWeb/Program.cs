﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (String.IsNullOrWhiteSpace(envName))
                throw new ArgumentNullException("EnvironmentName not found in ASPNETCORE_ENVIRONMENT");

            var appconfig = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{envName}.json", optional: true)
                .AddJsonFile($"configuration.json", optional: true)
                .Build();
            var host = new WebHostBuilder()
                .UseEnvironment(envName)
                .UseConfiguration(appconfig)
                .ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseUrls(appconfig.GetValue<string>("WebHostBuilder:UseUrls"))
                //.UseIISIntegration()
                .UseStartup<Startup>();
            host.Build().Run();
            //CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
