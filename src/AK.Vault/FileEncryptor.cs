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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace AK.Vault
{
    /// <summary>
    /// Encrypts and decrypts sets of files.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class FileEncryptor
    {
        private readonly SymmetricEncryptor _symmetricEncryptor;
        private readonly VaultOptions _vaultOptions;
        private readonly FileNameManager _fileNameManager;
        private readonly SymmetricEncryptionParameters _parameters;
        private readonly BlockingCollection<Message> _messages = new BlockingCollection<Message>();
        private readonly string _vaultName;

        internal FileEncryptor(SymmetricEncryptor symmetricEncryptor,
            EncryptionKeyGenerator encryptionKeyGenerator,
            VaultOptions vaultOptions,
            FileNameManager fileNameManager,
            EncryptionKeyInput encryptionKeyInput,
            string vaultName)
        {
            var key = encryptionKeyGenerator.Generate(encryptionKeyInput);

            _parameters = new SymmetricEncryptionParameters
            {
                Algorithm = AlgorithmType.Aes,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = key
            };

            _symmetricEncryptor = symmetricEncryptor;
            _vaultOptions = vaultOptions;
            _fileNameManager = fileNameManager;
            _vaultName = vaultName;
        }

        /// <summary>
        /// Action to take with an update message when one is sent
        /// (for use primarily to report progress).
        /// </summary>
        public Action<Message> UpdateMessageAction { get; set; }

        /// <summary>
        /// Encrypts the given set of files.
        /// </summary>
        /// <param name="filePatterns">
        /// List of file patterns. 
        /// A file pattern can be a single file, a folder, or a folder path with a wildcard.</param>
        /// <returns>
        /// List of FileEncryptionResult instances representing the encryption result for each
        /// file in the set.
        /// </returns>
        public IEnumerable<FileEncryptionResult> Encrypt(IEnumerable<string> filePatterns)
        {
            var results = filePatterns
                .SelectMany(GetEncryptionResultListForEncryption)
                .ToArray();

            ExecuteBatch(results, EncryptFile);

            return results;
        }

        /// <summary>
        /// Decrypts the given set of files.
        /// </summary>
        /// <param name="filePatterns">
        /// List of file patterns. For decryption, the file pattern is assumed
        /// relative to the default encrypted file location.
        /// </param>
        /// <returns>
        /// List of FileEncryptionResult instances representing the decryption result for each
        /// file in the set.
        /// </returns>
        public IEnumerable<FileEncryptionResult> Decrypt(IEnumerable<string> filePatterns)
        {
            var results = filePatterns
                .SelectMany(GetEncryptionResultListForDecryption)
                .ToArray();

            ExecuteBatch(results, DecryptFile);

            return results;
        }

        private static IEnumerable<FileEncryptionResult> GetEncryptionResultListForEncryption(string filePattern)
        {
            if (Directory.Exists(filePattern))
            {
                return Directory
                    .GetFiles(filePattern, "*.*", SearchOption.AllDirectories)
                    .Select(x => new FileEncryptionResult { UnencryptedFilePath = x })
                    .ToArray();
            }

            if (File.Exists(filePattern)) return new[] { new FileEncryptionResult { UnencryptedFilePath = filePattern } };

            var pattern = Path.GetFileName(filePattern);
            var directory = Path.GetDirectoryName(filePattern);

            if (pattern == null || directory == null)
                throw new Exception($"Unexpected error - cannot get file or directory name from file pattern {filePattern}.");

            return Directory
                .GetFiles(directory, pattern, SearchOption.AllDirectories)
                .Select(x => new FileEncryptionResult { UnencryptedFilePath = x })
                .ToArray();
        }

        private IEnumerable<FileEncryptionResult> GetEncryptionResultListForDecryption(string filePattern)
        {
            var encryptedFileLocation = _vaultOptions.Vaults
                .Single(x => x.Name == _vaultName).EncryptedFileLocation;

            return Directory
                .GetFiles(encryptedFileLocation, filePattern, SearchOption.TopDirectoryOnly)
                .Select(x => new FileEncryptionResult { EncryptedFilePath = x })
                .ToArray();
        }

        private void ExecuteBatch(IEnumerable<FileEncryptionResult> results, Action<FileEncryptionResult> action)
        {
            // ReSharper disable AccessToDisposedClosure

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                using (var task = Task.Factory.StartNew(
                    () => ProcessMessageUpdate(cancellationTokenSource), cancellationTokenSource.Token))
                {
                    Parallel.ForEach(results, action);
                    cancellationTokenSource.Cancel();

                    Info();

                    // ReSharper disable once MethodSupportsCancellation
                    //
                    task.Wait();
                }
            }

            // ReSharper restore AccessToDisposedClosure            
        }

        private void ProcessMessageUpdate(CancellationTokenSource cancellationTokenSource)
        {
            while (true)
            {
                if (cancellationTokenSource.IsCancellationRequested) break;

                var message = _messages.Take();
                if (message == Message.Empty) continue;

                UpdateMessageAction?.Invoke(message);
            }
        }

        private void EncryptFile(FileEncryptionResult encryptionResult)
        {
            if (encryptionResult.IsDone) return;
            try
            {
                var encryptedFileLocation = _vaultOptions.Vaults
                    .Single(x => x.Name == _vaultName).EncryptedFileLocation;

                encryptionResult.EncryptedFilePath = Path.Combine(encryptedFileLocation,
                    _fileNameManager.GenerateNameForEncryptedFile(encryptionResult.UnencryptedFilePath));

                if (File.Exists(encryptionResult.EncryptedFilePath)) File.Delete(encryptionResult.EncryptedFilePath);

                Info($"[{encryptionResult.UnencryptedFilePath}]: Encrypting...");
                using (FileStream
                    inFile = File.OpenRead(encryptionResult.UnencryptedFilePath),
                    outFile = File.OpenWrite(encryptionResult.EncryptedFilePath))
                {
                    _fileNameManager.WriteOriginalFileNameToStream(encryptionResult.UnencryptedFilePath, outFile);
                    _symmetricEncryptor.Encrypt(_parameters, inFile, outFile);
                }

                encryptionResult.IsDone = true;
                Info($"[{encryptionResult.UnencryptedFilePath}]: Encrypted.");
            }
            catch (Exception ex)
            {
                encryptionResult.Exception = ex;
                Error($"[{encryptionResult.UnencryptedFilePath}]: Could not encrypt.");
            }
        }

        private void DecryptFile(FileEncryptionResult encryptionResult)
        {
            try
            {
                var sourceFileName = encryptionResult.EncryptedFilePath;

                Info($"[{sourceFileName}]: Deciphering name...");
                using (var inFile = File.OpenRead(sourceFileName))
                {
                    encryptionResult.UnencryptedFilePath = _fileNameManager.ReadOriginalFileNameFromStream(inFile);
                    Info($"[{encryptionResult.UnencryptedFilePath}]: Decrypting...");

                    var targetDirectory = Path.GetDirectoryName(encryptionResult.UnencryptedFilePath);
                    if (targetDirectory == null)
                    {
                        throw new Exception("Unexpected error - cannot get directory name from " +
                            $"file path {encryptionResult.UnencryptedFilePath}.");
                    }

                    if (Path.IsPathRooted(targetDirectory))
                    {
                        targetDirectory = targetDirectory.Substring(
                            targetDirectory.IndexOf(Path.DirectorySeparatorChar) + 1);
                    }

                    var decryptedFileLocation = _vaultOptions.Vaults
                        .Single(x => x.Name == _vaultName).DecryptedFileLocation;

                    targetDirectory = Path.Combine(decryptedFileLocation, targetDirectory);
                    if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);

                    encryptionResult.UnencryptedFilePath = Path.Combine(
                        targetDirectory, Path.GetFileName(encryptionResult.UnencryptedFilePath));

                    using (var outFile = File.OpenWrite(encryptionResult.UnencryptedFilePath))
                        _symmetricEncryptor.Decrypt(_parameters, inFile, outFile);
                }

                encryptionResult.IsDone = true;
                Info($"[{encryptionResult.UnencryptedFilePath}]: Decrypted.");
            }
            catch (Exception ex)
            {
                encryptionResult.Exception = ex;
                Error($"[{encryptionResult.EncryptedFilePath}]: Could not decrypt.");
            }
        }

        private void Info(string message = null) => _messages.Add(message == null ? Message.Empty : new Message(message));

        private void Error(string message) => _messages.Add(message == null ? Message.Empty : new Message(message, true));
    }
}