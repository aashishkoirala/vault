/*******************************************************************************************************************************
 * AK.Vault.Console.Screen
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
using Con = System.Console;

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// Convenience wrapper around System.Console.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal static class Screen
    {
        public static void Clear()
        {
            Con.Clear();
        }

        public static void Print()
        {
            Con.WriteLine();
        }

        public static void Print(string format, params object[] args)
        {
            Print(Colors.Normal, format, args);
        }

        public static void Print(ConsoleColor foreColor, string format, params object[] args)
        {
            var existingForeColor = Con.ForegroundColor;
            try
            {
                Con.ForegroundColor = foreColor;
                Con.WriteLine(format, args);
            }
            finally
            {
                Con.ForegroundColor = existingForeColor;
            }
        }

        public static void Print(ConsoleColor foreColor, ConsoleColor backColor, string format, params object[] args)
        {
            var existingForeColor = Con.ForegroundColor;
            var existingBackColor = Con.BackgroundColor;
            try
            {
                Con.ForegroundColor = foreColor;
                Con.BackgroundColor = backColor;
                Con.WriteLine(format, args);
            }
            finally
            {
                Con.ForegroundColor = existingForeColor;
                Con.BackgroundColor = existingBackColor;
            }
        }

        public static ConsoleKeyInfo ReadKey()
        {
            return Con.ReadKey(true);
        }

        public static class Colors
        {
            public const ConsoleColor Heading = ConsoleColor.Cyan;
            public const ConsoleColor Normal = ConsoleColor.Gray;
            public const ConsoleColor Error = ConsoleColor.Red;

            public static ConsoleColor CurrentForeground => Con.ForegroundColor;

            public static ConsoleColor CurrentBackground => Con.BackgroundColor;
        }
    }
}