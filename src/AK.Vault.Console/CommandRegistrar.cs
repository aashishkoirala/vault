using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace AK.Vault.Console
{
    public static class CommandRegistrar
    {
        public static IServiceCollection AddVaultConsoleCommands(this IServiceCollection services)
        {
            var assembly = typeof(ICommand).Assembly;
            var commandTypes = assembly.GetTypes().Where(x => x != typeof(CommandBase) && x.GetInterfaces().Any(y => y == typeof(ICommand))).ToArray();
            foreach (var type in commandTypes) services = services.AddTransient(type);
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