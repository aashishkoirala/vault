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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace AK.Vault.Console
{
    internal static class Program
    {
        /// <summary>
        /// Entry point method.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>0 if success, 1 if not.</returns>
        public static async Task<int> Main(string[] args)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var applicationState = new ApplicationState(cancellationTokenSource);

                await new HostBuilder()
                    .ConfigureAppConfiguration(c => c.AddCommandLine(args).AddJsonFile("appsettings.json"))
                    .ConfigureServices((c, s) => s
                        .Configure<ConsoleLifetimeOptions>(o => o.SuppressStatusMessages = true)
                        .Configure<VaultOptions>(o => c.Configuration.Bind(o))
                        .AddVaultServices()
                        .AddVaultConsoleServices(applicationState))
                    .RunConsoleAsync(cancellationTokenSource.Token);

                return applicationState.ReturnCode;
            }
        }
    }
}