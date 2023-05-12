using System;
using System.Globalization;
using UnityEngine;

namespace Sabresaurus.PlayerPrefsUtilities
{
    public static class PlayerPrefsUtility
    {
        // Each encrypted player pref key is given a prefix (this helps the Editor to identify them)
        public const string KEY_PREFIX = "ENC-";

        // Each encrypted value is prefixed with a type identifier (because encrypted values are stored as strings)
        public const string VALUE_FLOAT_PREFIX = "0";
        public const string VALUE_INT_PREFIX = "1";
        public const string VALUE_STRING_PREFIX = "2";
        public const string VALUE_BOOL_PREFIX = "3";

        /// <summary>
        /// Determines if the specified player pref key refers to an encrypted record
        /// </summary>
        public static bool IsEncryptedKey(string key)
        {
            // Encrypted keys use a special prefix
            if (key.StartsWith(KEY_PREFIX))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Decrypts the specified key
        /// </summary>
        public static string DecryptKey(string encryptedKey)
        {
            if (encryptedKey.StartsWith(KEY_PREFIX))
            {
                // Remove the key prefix from the encrypted key
                string strippedKey = encryptedKey.Substring(KEY_PREFIX.Length);
                // Return the decrypted key
                return SimpleEncryption.DecryptString(strippedKey);
            }
            else
            {
                throw new InvalidOperationException("Could not decrypt item, no match found in known encrypted key prefixes");
            }
        }

        /// <summary>
        /// Encrypted version of PlayerPrefs.SetFloat(), stored key and value is encrypted in player prefs
        /// </summary>
        public static void SetEncryptedFloat(string key, float value)
        {
            string encryptedKey = SimpleEncryption.EncryptString(key);
            string encryptedValue = SimpleEncryption.EncryptFloat(value);

            // Store the encrypted key and value (with relevant identifying prefixes) in PlayerPrefs
            PlayerPrefs.SetString(KEY_PREFIX + encryptedKey, VALUE_FLOAT_PREFIX + encryptedValue);
        }

        /// <summary>
        /// Encrypted version of PlayerPrefs.SetInt(), stored key and value is encrypted in player prefs
        /// </summary>
        public static void SetEncryptedInt(string key, int value)
        {
            string encryptedKey = SimpleEncryption.EncryptString(key);
            string encryptedValue = SimpleEncryption.EncryptInt(value);

            // Store the encrypted key and value (with relevant identifying prefixes) in PlayerPrefs
            PlayerPrefs.SetString(KEY_PREFIX + encryptedKey, VALUE_INT_PREFIX + encryptedValue);
        }

        /// <summary>
        /// Encrypted version of PlayerPrefs.SetString(), stored key and value is encrypted in player prefs
        /// </summary>
        public static void SetEncryptedString(string key, string value)
        {
            string encryptedKey = SimpleEncryption.EncryptString(key);
            string encryptedValue = SimpleEncryption.EncryptString(value);

            // Store the encrypted key and value (with relevant identifying prefixes) in PlayerPrefs
            PlayerPrefs.SetString(KEY_PREFIX + encryptedKey, VALUE_STRING_PREFIX + encryptedValue);
        }

        /// <summary>
        /// Encrypted version of EditorPrefs.SetBool(), stored key and value is encrypted in player prefs
        /// </summary>
        public static void SetEncryptedBool(string key, bool value)
        {
            string encryptedKey = SimpleEncryption.EncryptString(key);
            string encryptedValue = SimpleEncryption.EncryptBool(value);

            // Store the encrypted key and value (with relevant identifying prefixes) in PlayerPrefs
            PlayerPrefs.SetString(KEY_PREFIX + encryptedKey, VALUE_BOOL_PREFIX + encryptedValue);
        }

        /// <summary>
        /// Helper method that can handle any of the encrypted player pref types, returning a float, int or string based
        /// on what type of value has been stored.
        /// </summary>
        public static object GetEncryptedValue(string encryptedKey, string encryptedValue)
        {
            // See what type identifier the encrypted value starts
            if (encryptedValue.StartsWith(VALUE_FLOAT_PREFIX))
            {
                // It's a float, so decrypt it as a float and return the value
                return GetEncryptedFloat(SimpleEncryption.DecryptString(encryptedKey.Substring(KEY_PREFIX.Length)));
            }
            else if (encryptedValue.StartsWith(VALUE_INT_PREFIX))
            {
                // It's an int, so decrypt it as an int and return the value
                return GetEncryptedInt(SimpleEncryption.DecryptString(encryptedKey.Substring(KEY_PREFIX.Length)));
            }
            else if (encryptedValue.StartsWith(VALUE_STRING_PREFIX))
            {
                // It's a string, so decrypt it as a string and return the value
                return GetEncryptedString(SimpleEncryption.DecryptString(encryptedKey.Substring(KEY_PREFIX.Length)));
            }
            else if (encryptedValue.StartsWith(VALUE_BOOL_PREFIX))
            {
                // It's a string, so decrypt it as a string and return the value
                return GetEncryptedBool(SimpleEncryption.DecryptString(encryptedKey.Substring(KEY_PREFIX.Length)));
            }
            else
            {
                throw new InvalidOperationException("Could not decrypt item, no match found in known encrypted key prefixes");
            }
        }

        /// <summary>
        /// Encrypted version of PlayerPrefs.GetFloat(), an unencrypted key is passed and the value is returned decrypted
        /// </summary>
        public static float GetEncryptedFloat(string key, float defaultValue = 0.0f)
        {
            // Encrypt and prefix the key so we can look it up from player prefs
            string encryptedKey = KEY_PREFIX + SimpleEncryption.EncryptString(key);

            // Look up the encrypted value
            string fetchedString = PlayerPrefs.GetString(encryptedKey);

            if (!string.IsNullOrEmpty(fetchedString))
            {
                // Strip out the type identifier character
                fetchedString = fetchedString.Remove(0, 1);

                // Decrypt and return the float value
                return SimpleEncryption.DecryptFloat(fetchedString);
            }
            else
            {
                // No existing player pref value, so return defaultValue instead
                return defaultValue;
            }
        }

        /// <summary>
        /// Encrypted version of PlayerPrefs.GetInt(), an unencrypted key is passed and the value is returned decrypted
        /// </summary>
        public static int GetEncryptedInt(string key, int defaultValue = 0)
        {
            // Encrypt and prefix the key so we can look it up from player prefs
            string encryptedKey = KEY_PREFIX + SimpleEncryption.EncryptString(key);

            // Look up the encrypted value
            string fetchedString = PlayerPrefs.GetString(encryptedKey);

            if (!string.IsNullOrEmpty(fetchedString))
            {
                // Strip out the type identifier character
                fetchedString = fetchedString.Remove(0, 1);

                // Decrypt and return the int value
                return SimpleEncryption.DecryptInt(fetchedString);
            }
            else
            {
                // No existing player pref value, so return defaultValue instead
                return defaultValue;
            }
        }

        /// <summary>
        /// Encrypted version of PlayerPrefs.GetString(), an unencrypted key is passed and the value is returned decrypted
        /// </summary>
        public static string GetEncryptedString(string key, string defaultValue = "")
        {
            // Encrypt and prefix the key so we can look it up from player prefs
            string encryptedKey = KEY_PREFIX + SimpleEncryption.EncryptString(key);

            // Look up the encrypted value
            string fetchedString = PlayerPrefs.GetString(encryptedKey);

            if (!string.IsNullOrEmpty(fetchedString))
            {
                // Strip out the type identifier character
                fetchedString = fetchedString.Remove(0, 1);

                // Decrypt and return the string value
                return SimpleEncryption.DecryptString(fetchedString);
            }
            else
            {
                // No existing player pref value, so return defaultValue instead
                return defaultValue;
            }
        }

        /// <summary>
        /// Encrypted version of EditorPrefs.GetBool(), an unencrypted key is passed and the value is returned decrypted
        /// </summary>
        public static bool GetEncryptedBool(string key, bool defaultValue = false)
        {
            // Encrypt and prefix the key so we can look it up from player prefs
            string encryptedKey = KEY_PREFIX + SimpleEncryption.EncryptString(key);

            // Look up the encrypted value
            string fetchedString = PlayerPrefs.GetString(encryptedKey);

            if (!string.IsNullOrEmpty(fetchedString))
            {
                // Strip out the type identifier character
                fetchedString = fetchedString.Remove(0, 1);

                // Decrypt and return the int value
                return SimpleEncryption.DecryptBool(fetchedString);
            }
            else
            {
                // No existing player pref value, so return defaultValue instead
                return defaultValue;
            }
        }

        /// <summary>
        /// Helper method to store a bool in PlayerPrefs (stored as an int)
        /// </summary>
        public static void SetBool(string key, bool value)
        {
            // Store the bool as an int (1 for true, 0 for false)
            if (value)
            {
                PlayerPrefs.SetInt(key, 1);
            }
            else
            {
                PlayerPrefs.SetInt(key, 0);
            }
        }

        /// <summary>
        /// Helper method to retrieve a bool from PlayerPrefs (stored as an int)
        /// </summary>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            // Use HasKey to check if the bool has been stored (as int defaults to 0 which is ambiguous with a stored False)
            if (PlayerPrefs.HasKey(key))
            {
                int value = PlayerPrefs.GetInt(key);

                // As in C, assume zero is false and any non-zero value (including its intended 1) is true
                if (value != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // No existing player pref value, so return defaultValue instead
                return defaultValue;
            }
        }

        /// <summary>
        /// Helper method to store an enum value in PlayerPrefs (stored using the string name of the enum)
        /// </summary>
        public static void SetEnum(string key, Enum value)
        {
            // Convert the enum value to its string name (as opposed to integer index) and store it in a string PlayerPref
            PlayerPrefs.SetString(key, value.ToString());
        }

        /// <summary>
        /// Generic helper method to retrieve an enum value from PlayerPrefs and parse it from its stored string into the
        /// specified generic type. This method should generally be preferred over the non-generic equivalent
        /// </summary>
        public static T GetEnum<T>(string key, T defaultValue = default(T)) where T : struct
        {
            // Fetch the string value from PlayerPrefs
            string stringValue = PlayerPrefs.GetString(key);

            if (!string.IsNullOrEmpty(stringValue))
            {
                // Existing value, so parse it using the supplied generic type and cast before returning it
                return (T)Enum.Parse(typeof(T), stringValue);
            }
            else
            {
                // No player pref for this, just return default. If no default is supplied this will be the enum's default
                return defaultValue;
            }
        }

        /// <summary>
        /// Non-generic helper method to retrieve an enum value from PlayerPrefs (stored as a string). Default value must be
        /// passed, passing null will mean you need to do a null check where you call this method. Generally try to use the
        /// generic version of this method instead: GetEnum<T>
        /// </summary>
        public static object GetEnum(string key, Type enumType, object defaultValue)
        {
            // Fetch the string value from PlayerPrefs
            string value = PlayerPrefs.GetString(key);

            if (!string.IsNullOrEmpty(value))
            {
                // Existing value, parse it using the supplied type, then return the result as an object
                return Enum.Parse(enumType, value);
            }
            else
            {
                // No player pref for this key, so just return supplied default. It's required to supply a default value,
                // you can just pass null, but you would then need to do a null check where you call non-generic GetEnum().
                // Consider using GetEnum<T>() which doesn't require a default to be passed (supplying default(T) instead)
                return defaultValue;
            }
        }

        /// <summary>
        /// Helper method to store a DateTime (complete with its timezone) in PlayerPrefs as a string
        /// </summary>
        public static void SetDateTime(string key, DateTime value)
        {
            // Convert to an ISO 8601 compliant string ("o"), so that it's fully qualified, then store in PlayerPrefs
            PlayerPrefs.SetString(key, value.ToString("o", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Helper method to retrieve a DateTime from PlayerPrefs (stored as a string) and return a DateTime complete with
        /// timezone (works with UTC and local DateTimes)
        /// </summary>
        public static DateTime GetDateTime(string key, DateTime defaultValue = new DateTime())
        {
            // Fetch the string value from PlayerPrefs
            string stringValue = PlayerPrefs.GetString(key);

            if (!string.IsNullOrEmpty(stringValue))
            {
                // Make sure to parse it using Roundtrip Kind otherwise a local time would come out as UTC
                return DateTime.Parse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }
            else
            {
                // No existing player pref value, so return defaultValue instead
                return defaultValue;
            }
        }

        /// <summary>
        /// Helper method to store a TimeSpan in PlayerPrefs as a string
        /// </summary>
        public static void SetTimeSpan(string key, TimeSpan value)
        {
            // Use the TimeSpan's ToString() method to encode it as a string which is then stored in PlayerPrefs
            PlayerPrefs.SetString(key, value.ToString());
        }

        /// <summary>
        /// Helper method to retrieve a TimeSpan from PlayerPrefs (stored as a string)
        /// </summary>
        public static TimeSpan GetTimeSpan(string key, TimeSpan defaultValue = new TimeSpan())
        {
            // Fetch the string value from PlayerPrefs
            string stringValue = PlayerPrefs.GetString(key);

            if (!string.IsNullOrEmpty(stringValue))
            {
                // Parse the string and return the TimeSpan
                return TimeSpan.Parse(stringValue);
            }
            else
            {
                // No existing player pref value, so return defaultValue instead
                return defaultValue;
            }
        }
    }
}
