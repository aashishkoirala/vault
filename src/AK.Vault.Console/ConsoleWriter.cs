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
using SysConsole = System.Console;

namespace AK.Vault.Console
{
    /// <summary>
    /// Convenience wrapper around System.Console.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class ConsoleWriter
    {
        private const ConsoleColor HeadingForeground = ConsoleColor.Cyan;
        private const ConsoleColor NormalForeground = ConsoleColor.Gray;
        private const ConsoleColor ErrorForeground = ConsoleColor.Red;
        private const ConsoleColor HighlightForeground = ConsoleColor.Black;
        private const ConsoleColor HighlightBackground = ConsoleColor.Gray;
        private const ConsoleColor NormalBackground = ConsoleColor.Black;

        public bool IsRedirected => SysConsole.IsOutputRedirected || SysConsole.IsInputRedirected || SysConsole.IsErrorRedirected;

        public void Clear()
        {
            if (!SysConsole.IsOutputRedirected) SysConsole.Clear();
        }

        public void Blank() => SysConsole.WriteLine();
        public void Info(string message, bool noLine = false) => Print(NormalForeground, message, noLine);
        public void Error(string message) => Print(ErrorForeground, message);
        public void Heading(string message) => Print(HeadingForeground, message);
        public void Highlight(string message) => Print(HighlightForeground, HighlightBackground, message);
        public ConsoleKeyInfo ReadKey() => SysConsole.ReadKey(true);

        private static void Print(ConsoleColor foreColor, string message, bool noLine = false) => 
            Print(foreColor, NormalBackground, message, noLine);

        private static void Print(ConsoleColor foreColor, ConsoleColor backColor, 
            string message, bool noLine = false)
        {
            var existingForeColor = SysConsole.ForegroundColor;
            var existingBackColor = SysConsole.BackgroundColor;
            try
            {
                SysConsole.ForegroundColor = foreColor;
                SysConsole.BackgroundColor = backColor;
                if (noLine) SysConsole.Write(message);
                else SysConsole.WriteLine(message);
            }
            finally
            {
                SysConsole.ForegroundColor = existingForeColor;
                SysConsole.BackgroundColor = existingBackColor;
            }
        }
    }
}