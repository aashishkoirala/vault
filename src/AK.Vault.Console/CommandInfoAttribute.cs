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

using System;

namespace AK.Vault.Console
{
    /// <summary>
    /// MEF metadata attribute that implements ICommandInfo and should be applied
    /// to ICommand instances.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class CommandInfoAttribute : Attribute
    {
        public string CommandName { get; set; }
        public bool RequiresEncryptionKeyInput { get; set; }
    }
}
