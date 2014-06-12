/*******************************************************************************************************************************
 * AK.Vault.Console.CommandParser
 * Copyright © 2014 Aashish Koirala <http://aashishkoirala.github.io>
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
 * along with VAULT.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// Entry point; also parses command line arguments to create the correct ICommand instance
    /// and execute it.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal static class CommandParser
    {
        /// <summary>
        /// Entry point method.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>0 if success, 1 if not.</returns>
        public static int Main(string[] args)
        {
            try
            {
                Screen.Print(Screen.Colors.Heading, "VAULT by Aashish Koirala (c) 2014");
                Screen.Print();
                return Parse(args).Execute() ? 0 : 1;
            }
            catch (Exception ex)
            {
                Screen.Print(Screen.Colors.Error, "Unexpected error:{0}{1}{0}", Environment.NewLine, ex);
                return 1;
            }
        }

        private static ICommand Parse(string[] args)
        {
            ICommand command;
            return !TryParse(args, out command) ? new DoNothingCommand() : command;
        }

        private static bool TryParse(string[] args, out ICommand command)
        {
            command = null;
            try
            {
                if (args.Length == 0) args = new[] {"launch"};

                var commandName = args[0];
                var assembly = Assembly.GetExecutingAssembly();
                using (var assemblyCatalog = new AssemblyCatalog(assembly))
                using (var container = new CompositionContainer(assemblyCatalog))
                {
                    var export = container
                        .GetExports<ICommand, ICommandInfo>()
                        .SingleOrDefault(x => x.Metadata.CommandName == commandName);

                    if (export == null)
                    {
                        Screen.Print(Screen.Colors.Error, "Invalid command: {0}", commandName);
                        PrintUsage();
                        return false;
                    }

                    command = export.Value;
                    var assignParametersResult = command.AssignParameters(args.Skip(1).ToArray());
                    if (!assignParametersResult)
                    {
                        Screen.Print(Screen.Colors.Error, "Invalid parameters for {0}.", commandName);
                        PrintUsage();
                        return false;
                    }

                    if (export.Metadata.RequiresEncryptionKeyInput)
                    {
                        bool cancelled;
                        var encryptionKeyInput = EncryptionKeyInputPrompter.Prompt(out cancelled);
                        if (cancelled) return false;
                        command.AssignEncryptionKeyInput(encryptionKeyInput);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Screen.Print(Screen.Colors.Error, "Error parsing command: {0}", ex.Message);
                PrintUsage();
                return false;
            }
        }

        private static void PrintUsage()
        {
            Screen.Print();
            Screen.Print(Screen.Colors.Heading, "Usage:");
            Screen.Print("\tvault [launch]");
            Screen.Print("\t\tLaunches the interactive application.");
            Screen.Print();
            Screen.Print("\tvault encrypt filepattern1 [filepattern2] [filepattern3] ...");
            Screen.Print("\t\tEncrypts the given files.");
            Screen.Print();
            Screen.Print("\tvault encrypt filepattern1 [filepattern2] [filepattern3] ...");
            Screen.Print("\t\tDecrypts the given files.");
            Screen.Print();
            Screen.Print("\tvault list");
            Screen.Print("\t\tLists the content of the vault.");
            Screen.Print();
            Screen.Print("\tvault find filename");
            Screen.Print("\t\tChecks to see if the file is in the vault and gives you the encrypted filename if it does.");
            Screen.Print();
        }

        private class DoNothingCommand : ICommand
        {
            public void AssignEncryptionKeyInput(EncryptionKeyInput encryptionKeyInput) {}

            public bool AssignParameters(string[] args)
            {
                return false;
            }

            public bool Execute()
            {
                return false;
            }
        }
    }
}