using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AK.Vault.Console
{
    internal class Program : IHostedService
    {
        private readonly CommandParser commandParser;
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Entry point method.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>0 if success, 1 if not.</returns>
        public static async Task<int> Main(string[] args)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var commandParser = new CommandParser(cancellationTokenSource);

                await new HostBuilder()
                    .ConfigureAppConfiguration(c => c.AddCommandLine(args).AddJsonFile("appsettings.json"))
                    .ConfigureServices((c, s) => s
                        .Configure<ConsoleLifetimeOptions>(o => o.SuppressStatusMessages = true)
                        .Configure<VaultOptions>(o => c.Configuration.Bind(o))
                        .AddSingleton(commandParser)
                        .AddVaultServices()
                        .AddVaultConsoleCommands()
                        .AddHostedService<Program>())
                    .RunConsoleAsync(cancellationTokenSource.Token);

                return commandParser.ReturnCode;
            }
        }

        public Program(IConfiguration configuration, IServiceProvider serviceProvider, CommandParser commandParser)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            this.commandParser = commandParser;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            this.commandParser.ParseAndExecute(this.configuration, this.serviceProvider);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}