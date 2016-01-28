## Vault
##### By [Aashish Koirala](http://aashishkoirala.github.io)

Vault is a command-line tool that lets you encrypt and decrypt your sensitive files. I built it for my use as I wanted to put my files up in the cloud but did not trust any cloud provider with my data in the open. With this, I feel confident putting all my stuff up on Dropbox (yes, the one that was recently hacked).

##### How It Works
The encryption is done using the following parameters:

- **Algorithm**: Advanced Encryption Standard (AES)
- **Key Size**: 256 bits
- **Block Cipher Mode**: Cipher Block Chaining (CBC)
- **Initialization Vector Size:** 128 bits
- **Padding**: PKCS7

The encryption process for each file works as follows:

- The full file path is lowercased and the name's 256-bit SHA1 hash is calculated. The hex string version of this with a ".vault" extension becomes the name of the encrypted file.
- A cryptographically random sequence of non-zero bytes is generated to create the initialization vector.
- The full path of the file is BASE64 encoded.
- The following are then written to the output file in sequence:
	- The length of the BASE64 encoded filename.
	- The BASE64 encoded filename.
	- The initialization vector.
	- The encrypted data.

The decryption process, of course, works in reverse.

I did not want the added overhead of maintaining and protecting an encryption key - so I decided to make it a function of some other pieces of information that only I would be able to come up with. Not at all acceptable for an enterprise solution, but I think it works for a personal encryptor and it works for me. During encryption/decryption, it asks me for:

- My full name
- My date of birth
- My social security number
- A secret passphrase

It then combines these values and calculates the 256-bit SHA1 hash of the result - which becomes the encryption key.

##### Configuration
You can configure up to 36 vaults. A good use case for multiple vaults is if you are syncing each vault to a different cloud storage provider. You configure vaults in *vault.exe.config*:

	<ak.vault>
		<vaultConfiguration>
		  <vaults>
		    <vault name="vault_name" encryptedFileLocation="encrypted_file_location" decryptedFileLocation="decrypted_file_location" />
		    <vault name="vault_name" encryptedFileLocation="encrypted_file_location" decryptedFileLocation="decrypted_file_location" />
		    <vault name="vault_name" encryptedFileLocation="encrypted_file_location" decryptedFileLocation="decrypted_file_location" />
		    <vault name="vault_name" encryptedFileLocation="encrypted_file_location" decryptedFileLocation="decrypted_file_location" />
			...
		  </vaults>
		</vaultConfiguration>
	</ak.vault>

Where:

*vault\_name:* A unique name to identify the vault. This must be unique for each vault, and the application will ask you to pick a vault before you do anything.

*encrypted\_file\_location:* The folder where you want to keep all your encrypted files for this vault. This needs to be one folder where everything goes, and can be sync'ed with your favorite cloud storage.

*decrypted\_file\_location:* The folder that is used as a temporary workspace to place decrypted file and folder structure in as you're working with a set of files. You could use a different one per vault or the same one for all vaults.

##### Usage

**vault** [**launch**]

Launches the interactive console application. Once you have some files in your encrypted location, the interactive application lets you browse through the folder tree and decrypt and launch selected files as needed, making consuming encrypted files a tad easier.

**vault list**

Displays a tree-type list of all files that are in the encrypted file location, by folder hierarchy.

**vault report**

Displays a tabular report of all files that are in the encrypted file location, with the original and encrypted names.

**vault find** *original\_file\_full\_path*

Checks to see if the file has been encrypted and is in the encrypted file location. If so, gives you the name of the encrypted file.

*Example:*

	vault find C:\Aashish\Text\TechRef\Domain_Driven_Design.pdf
 
     
**vault encrypt** *filepattern1* [*filepattern2*] [*filepattern3*] `...`

Encrypts the files and puts the encrypted files in the encrypted file location. A file pattern can be the full path for a file, the full path for a folder, or a folder path with a wild card representing multiple files. You can specify multiple file patterns.

*Example:*

	vault encrypt C:\Aashish\Text\TechRef\*.pdf "C:\Aashish\Data\Secret Stuff\*.xlsx"

**vault decrypt** *filepattern1* [*filepattern2*] [*filepattern3*] `...`

Decrypts the given files and puts them in the configured decrypted file location. The file patterns in this case need to be relative to the configured encrypted file location.

*Example:*

	vault decrypt 2342389234ABEF02340ABC203482300DE234023EF02340ABC20348F0234348F0.vault

**vault fd** *original\_file1\_full\_path* [*original\_file2\_full\_path*] `...`

Checks to see if the file has been encrypted and is in the encrypted file location. If so, decrypts the file.

*Example:*

	vault fd C:\Aashish\Text\TechRef\Domain_Driven_Design.pdf
