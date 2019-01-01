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

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "list" command; generates a display tree of the vault.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [CommandInfo(CommandName = "list", RequiresEncryptionKeyInput = false)]
    internal class ListCommand : CommandBase
    {
        protected override bool PromptBeforeStart => false;

        private readonly ListGenerator _listGenerator;

        public ListCommand(ListGenerator listGenerator, 
            FileEncryptorFactory fileEncryptorFactory,
            ConsoleWriter console, ILogger<ListCommand> logger) : 
            base(fileEncryptorFactory, console, logger, true) => _listGenerator = listGenerator;

        protected override async Task<bool> ExecuteCommand(ICollection<Exception> exceptions)
        {
            PrintListItem(await _listGenerator.Generate(VaultName));
            return true;
        }

        private void PrintListItem(FolderEntry entry, string indent = null)
        {
            indent = indent ?? string.Empty;
            _console.Info($"{indent} [{entry.Name}]");
            indent += ">";

            foreach (var file in entry.Files) PrintListItem(file, indent);
            foreach (var folder in entry.Folders) PrintListItem(folder, indent);
        }

        private void PrintListItem(FileEntry entry, string indent)
        {
            indent = indent ?? string.Empty;
            _console.Info($"{indent} {entry.Name}");
        }
    }
}