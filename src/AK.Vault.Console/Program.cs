using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Reflection;
using AK.Vault.Configuration;

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
                        .Configure<VaultConfiguration>(o => c.Configuration.Bind(o))
                        .AddSingleton(commandParser)
                        .AddSingleton<IEncryptionKeyGenerator, EncryptionKeyGenerator>()
                        .AddSingleton<IFileEncryptorFactory, FileEncryptorFactory>()
                        .AddSingleton<IFileNameManager, FileNameManager>()
                        .AddSingleton<IListGenerator, ListGenerator>()
                        .AddSingleton<ISymmetricEncryptor, SymmetricEncryptor>()
                        .AddCommands()
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

    public static class Thingy
    {
        public static IServiceCollection AddCommands(this IServiceCollection services)
        {
            var assembly = typeof(ICommand).Assembly;
            var commandTypes = assembly.GetTypes().Where(x => x != typeof(CommandBase) && x.GetInterfaces().Any(y => y == typeof(ICommand))).ToArray();
            foreach (var type in commandTypes) services = services.AddSingleton(type);
            var commandNameTypeHash = commandTypes.ToDictionary(x => x.GetCustomAttribute<CommandInfoAttribute>().CommandName);
            services.AddTransient<Func<string, ICommand>>(sp => n =>
            {
                var type = commandNameTypeHash.TryGetValue(n, out var t) ? t : null;
                return (ICommand)(type == null ? null : sp.GetService(type));
            });
            return services;
        }
    }
}