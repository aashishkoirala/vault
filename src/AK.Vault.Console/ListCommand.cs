﻿/*******************************************************************************************************************************
 * AK.Vault.Console.ListCommand
 * Copyright © 2014-2016 Aashish Koirala <http://aashishkoirala.github.io>
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
using System.Collections.Generic;

#endregion

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

        private readonly IListGenerator listGenerator;

        public ListCommand(IListGenerator listGenerator, IFileEncryptorFactory fileEncryptorFactory) :
            base(fileEncryptorFactory)
        {
            this.listGenerator = listGenerator;
        }

        public override bool ProcessParameters() => true;

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            PrintListItem(this.listGenerator.Generate(this.VaultName));
            return true;
        }

        private static void PrintListItem(FolderEntry entry, string indent = null)
        {
            indent = indent ?? string.Empty;
            Screen.Print("{0} [{1}]", indent, entry.Name);
            indent += ">";

            foreach (var file in entry.Files) PrintListItem(file, indent);
            foreach (var folder in entry.Folders) PrintListItem(folder, indent);
        }

        private static void PrintListItem(FileEntry entry, string indent)
        {
            indent = indent ?? string.Empty;
            Screen.Print("{0} {1}", indent, entry.Name);
        }
    }
}