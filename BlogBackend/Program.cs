using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlogBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;
                    if (!env.IsDevelopment())
                    {
                        //�H�U�o�ӥu��bAzure Service App �̭�����A�bIIS�|�X��
                        var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
                        config.AddAzureKeyVault(
                        keyVaultEndpoint,
                        new DefaultAzureCredential());
                        //
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
