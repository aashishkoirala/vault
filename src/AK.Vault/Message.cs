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

namespace AK.Vault
{
    /// <summary>
    /// Represents an update/progress message sent by IFileEncryptor.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class Message
    {
        public Message(string description, bool isError = false)
        {
            Description = description;
            IsError = isError;
        }

        private Message() {}

        /// <summary>
        /// Represents an empty message to do nothing with.
        /// </summary>
        public static Message Empty { get; } = new EmptyMessage();

        /// <summary>
        /// Message description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Whether the message is an error message.
        /// </summary>
        public bool IsError { get; private set; }

        private class EmptyMessage : Message {}
    }
}
