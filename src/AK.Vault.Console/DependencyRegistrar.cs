/*******************************************************************************************************************************
 * Copyright © 2014-2019 Aashish Koirala <https://www.aashishkoirala.com>
 * 
 * This file is part of VAULT.
 *  
 * VAULT is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * VAULT is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with VAULT.  If not, see <https://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace AK.Vault.Console
{
    internal static class DependencyRegistrar
    {
        public static IServiceCollection AddVaultConsoleServices(
            this IServiceCollection services, ApplicationState applicationState) => services
                .AddSingleton<VaultSelector>()
                .AddSingleton<CommandExecutor>()
                .AddSingleton<EncryptionKeyEvaluator>()
                .AddSingleton(applicationState)
                .AddCommands();

        private static IServiceCollection AddCommands(this IServiceCollection services)
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