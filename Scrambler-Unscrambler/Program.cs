/*
 * WRITTEN BY: Millen Boekel
 * USING: Snake case naming convention
 * PURPOSE: To encrypt, decrypt and fracture a file for transit or general security
 * DATE FINISHED: 11/10/2019
 * 
 * Copyright (c) 2019 Millen Boekel
 * Released under the MIT licence: http://opensource.org/licenses/mit-license
 */

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Security.Cryptography;

namespace Scrambler_Unscrambler
{
    class Program
    {
        /*
         * To read input as strings
         * 
         * @field   prompt     Message to display to the user
         */
        static string string_read(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        /*
         * To read input as integers
         * 
         * @field   prompt     Message to display to the user
         */
        static int integer_read(string prompt)
        {
            string input;
            int result;
            bool cont = false;
            do // Keeps in a loop until a valid integer is input
            {
                Console.Write(prompt);
                input = Console.ReadLine();
                if (int.TryParse(input, out result) == true)
                {
                    cont = true;
                }
                else
                {
                    Console.WriteLine("Is not a number, try again."); // Letting user know their input is not a number
                }
            }
            while (cont == false);
            result = Convert.ToInt32(input);
            return result;
        }

        /*
         * Reading all bytes and converting to base64
         * 
         * @field   directory     directory of the file
         */
        static string go_to_base64(string directory)
        {
            byte[] result = File.ReadAllBytes(directory);
            return Convert.ToBase64String(result);
        }

        /*
         * Displays all the options for the user to select
         */
        static void option_menu() // 
        {
            Console.WriteLine("==== Encrypt/Decrypt Menu ====");
            Console.WriteLine("1. Encrypt");
            Console.WriteLine("2. Decrypt");
            Console.WriteLine("3. Exit");
        }

        /*
         * Where the encryption process begins
         * 
         * @field   location     directory of the file
         */
        static string encrypting(string location)
        {
            string key = string_read("Enter encryption key: ");
            int levels;
            do // making sure the levels of encryption are within the bounds, The encrypted file size gets exponentially bigger the more it is encrypted
            {
                levels = integer_read("Enter levels of encryption (1-3): ");
            }
            while (levels <= 0 && levels > 3);
            string encrypted = go_to_base64(location); // Getting base64 text
            for (int i = 0; i < levels; i++) // Encrypting the requested amount of times with the key
            {
                encrypted = Encryption.Encrypt(encrypted, key);
            }
            return encrypted;
        }

        /*
         * Cuts up the file into multiple files
         * 
         * @field   encrypted     the encrypted text
         * @field   location      directory of the file
         */
        static void chop_shop(string encrypted, string location)
        {
            string file_name = Path.GetFileNameWithoutExtension(location); // Getting the name of the file
            string path = Path.GetDirectoryName(location) + "\\"; // Getting the locations it's in
            int option;
            do // If the input is not a number nor between 1 and 10 it just loops again
            {
                option = integer_read("Number of files to split into? (1-10): ");
            }
            while (option <= 0 && option > 10);
            int char_per_file = encrypted.Length / option; // Getting how many characters per file is needed
            int char_tail = encrypted.Length % option; // Getting the remainder left
            string[] char_arr = new string[option]; // Each string in the array represents one file
            Directory.CreateDirectory(path + file_name); // Create a new temporary directory
            for (int i = 0; i < char_arr.Length; i++) // Looping through and getting text from the arrays
            {
                char_arr[i] = encrypted.Substring((i * char_per_file), char_per_file); // Getting the part of the file
                if (i == char_arr.Length - 1) // If it is the last file add the char_tail
                {
                    char_arr[i] += encrypted.Substring((encrypted.Length) - char_tail, char_tail);
                }
                File.WriteAllText(path + file_name + "\\" + file_name + Convert.ToString(i) + ".txt", char_arr[i]);
            }
            ZipFile.CreateFromDirectory(path + file_name,path + file_name + ".zip"); // Create the zip file
            Directory.Delete(path + file_name, true); // Deleting temp directory
        }

        /*
         * Undo the chopping of the file
         * 
         * @field   location      directory of the file
         */
        static string unchop_shop(string location)
        {
            ZipFile.ExtractToDirectory(location, location.Split('.')[0]); // Extract contents
            string[] files = Directory.GetFiles(location.Split('.')[0]); // save the files names
            string whole_file = "";
            for (int i = 0; i < files.Length; i++) // Loop through all the files
            {
                whole_file += File.ReadAllText(files[i]);
            }
            return whole_file;
        }

        /*
         * Decrypting the file
         * 
         * @field   text        the encrypted text
         * @field   location    the location of the file
         * @field   encryption  how many times the file has been encrypted
         * @field   key         encryption key
         * @field   extension   the extension of the file (eg .exe, .png... ect)
         */
        static void decrypting(string text, string location, int encryption, string key, string extension)
        {
            string buffer = text;
            string path = Path.GetDirectoryName(location) + "\\";
            for (int i = 0; i < encryption; i++) // decrypting however many times
            {
                buffer = Encryption.Decrypt(buffer, key);
            }
            byte[] decrypted = Convert.FromBase64String(buffer); // From base64 to bytes
            File.WriteAllBytes(path + Path.GetFileNameWithoutExtension(location) + extension, decrypted); // creating the file
            Directory.Delete(location.Split('.')[0], true); // cleaning up
            File.Delete(location); // cleaning up
        }

        static void Main(string[] args) // The main loop
        {
            while(true)
            {
                option_menu(); // Displays menu
                string option = string_read("Choose: ");
                switch (option)
                {
                    case "1": // Encrypt
                        string path = string_read("Enter path of file: ");
                        chop_shop(encrypting(path), path);
                        break;
                    case "2": // Decrypt
                        string location = string_read("zip file location: ");
                        int encryption = integer_read("Enter levels of encryption (1-3): ");
                        string key = string_read("Enter key: ");
                        string extension = string_read("File extension (eg .exe): ");
                        if (File.Exists(location))
                        {
                            decrypting(unchop_shop(location), location, encryption, key, extension);
                        }
                        else
                        {
                            Console.WriteLine("File doesn't exist");
                        }
                        break;
                    case "3": // Exit
                        Environment.Exit(0);
                        break;
                    default: // No mans land
                        Console.WriteLine("Not an option");
                        break;
                }
                Console.Clear();
            }
        }
    }
    public class Encryption // Place where the magic happens
    {
        // Taken from:
        // https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp

        public static string Encrypt(string clearText, string EncryptionKey)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText, string EncryptionKey)
        {
                cipherText = cipherText.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText;
        }
    }
}
