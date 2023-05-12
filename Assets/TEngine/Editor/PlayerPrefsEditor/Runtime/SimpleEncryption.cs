using System;
using System.Security.Cryptography;
using System.Text;

namespace Sabresaurus.PlayerPrefsUtilities
{
    public static class SimpleEncryption
    {
        // IMPORTANT: Make sure to change this key for each project you use this encryption in to help secure your
        // encrypted values. This key must be exactly 32 characters long (256 bit).
        private static string key = ":{j%6j?E:t#}G10mM%9hp5S=%}2,Y26C";

        // Cache the encryption provider
        private static RijndaelManaged provider = null;

        private static void SetupProvider()
        {
            // Create a new encryption provider
            provider = new RijndaelManaged();

            // Get the bytes from the supplied string key and use it as the provider's key
            provider.Key = Encoding.ASCII.GetBytes(key);

            // Ensure that the same data is always encrypted the same way when used with the same key
            provider.Mode = CipherMode.ECB;
        }

        /// <summary>
        /// Encrypts the specified string using the key stored in SimpleEncryption and returns the encrypted result
        /// </summary>
        public static string EncryptString(string sourceString)
        {
            if (provider == null)
            {
                // Encryption provider hasn't been set up yet, so set it up
                SetupProvider();
            }

            // Create an encryptor to encrypt the bytes
            ICryptoTransform encryptor = provider.CreateEncryptor();

            // Convert the source string into bytes to be encrypted
            byte[] sourceBytes = Encoding.UTF8.GetBytes(sourceString);

            // Encrypt the bytes using the encryptor we just created
            byte[] outputBytes = encryptor.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length);

            // Convert the encrypted bytes into a Base 64 string, so we can safely represent them as a string and return
            // that string
            return Convert.ToBase64String(outputBytes);
        }

        /// <summary>
        /// Decrypts the specified string from its specified encrypted value into the returned decrypted value using the
        /// key stored in SimpleEncryption
        /// </summary>
        public static string DecryptString(string sourceString)
        {
            if (provider == null)
            {
                // Encryption provider hasn't been set up yet, so set it up
                SetupProvider();
            }

            // Create a decryptor to decrypt the encrypted bytes
            ICryptoTransform decryptor = provider.CreateDecryptor();

            // Convert the base 64 string representing the encrypted bytes back into an array of encrypted bytes
            byte[] sourceBytes = Convert.FromBase64String(sourceString);

            // Use the decryptor we just created to decrypt those bytes
            byte[] outputBytes = decryptor.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length);

            // Turn the decrypted bytes back into the decrypted string and return it
            return Encoding.UTF8.GetString(outputBytes);
        }

        /// <summary>
        /// Encrypts the specified float value and returns an encrypted string
        /// </summary>
        public static string EncryptFloat(float value)
        {
            // Convert the float into its 4 bytes
            byte[] bytes = BitConverter.GetBytes(value);

            // Represent those bytes as a base 64 string
            string base64 = Convert.ToBase64String(bytes);

            // Return the encrypted version of that base 64 string
            return SimpleEncryption.EncryptString(base64);
        }

        /// <summary>
        /// Encrypts the specified int value and returns an encrypted string
        /// </summary>
        public static string EncryptInt(int value)
        {
            // Convert the int value into its 4 bytes
            byte[] bytes = BitConverter.GetBytes(value);

            // Represent those bytes as a base 64 string
            string base64 = Convert.ToBase64String(bytes);

            // Return the encrypted version of that base 64 string
            return SimpleEncryption.EncryptString(base64);
        }

        /// Encrypts the specified bool value and returns an encrypted string
        /// </summary>
        public static string EncryptBool(bool value)
        {
            // Convert the bool value into its 4 bytes
            byte[] bytes = BitConverter.GetBytes(value);

            // Represent those bytes as a base 64 string
            string base64 = Convert.ToBase64String(bytes);

            // Return the encrypted version of that base 64 string
            return SimpleEncryption.EncryptString(base64);
        }

        /// <summary>
        /// Decrypts the encrypted string representing a float into the decrypted float
        /// </summary>
        public static float DecryptFloat(string sourceString)
        {
            // Decrypt the encrypted string
            string decryptedString = SimpleEncryption.DecryptString(sourceString);

            // Convert the decrypted Base 64 representation back into bytes
            byte[] bytes = Convert.FromBase64String(decryptedString);

            // Turn the bytes back into a float and return it
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Decrypts the encrypted string representing an int into the decrypted int
        /// </summary>
        public static int DecryptInt(string sourceString)
        {
            // Decrypt the encrypted string
            string decryptedString = SimpleEncryption.DecryptString(sourceString);

            // Convert the decrypted Base 64 representation back into bytes
            byte[] bytes = Convert.FromBase64String(decryptedString);

            // Turn the bytes back into a int and return it
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Decrypts the encrypted string representing a bool into the decrypted bool
        /// </summary>
        public static bool DecryptBool(string sourceString)
        {
            // Decrypt the encrypted string
            string decryptedString = SimpleEncryption.DecryptString(sourceString);

            // Convert the decrypted Base 64 representation back into bytes
            byte[] bytes = Convert.FromBase64String(decryptedString);

            // Turn the bytes back into a bool and return it
            return BitConverter.ToBoolean(bytes, 0);
        }
    }
}
