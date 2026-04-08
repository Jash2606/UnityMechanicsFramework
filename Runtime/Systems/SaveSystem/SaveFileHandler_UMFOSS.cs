using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Static utility class for reading and writing save files to disk.
    /// Handles JSON serialization, optional XOR encryption, and slot management.
    /// All files are stored under Application.persistentDataPath/Saves/.
    /// This class has no scene dependencies — it only deals with files.
    /// </summary>
    public static class SaveFileHandler_UMFOSS
    {
        private const string SAVE_FOLDER = "Saves";
        private const string SAVE_EXTENSION = ".sav";

        /// <summary>
        /// The directory where all save files are stored.
        /// </summary>
        public static string SaveDirectory
        {
            get { return Path.Combine(Application.persistentDataPath, SAVE_FOLDER); }
        }

        /// <summary>
        /// Gets the full file path for a given slot name.
        /// </summary>
        /// <param name="slotName">The save slot name (e.g., "Slot1").</param>
        /// <returns>Full path to the save file.</returns>
        public static string GetFilePath(string slotName)
        {
            return Path.Combine(SaveDirectory, slotName + SAVE_EXTENSION);
        }

        /// <summary>
        /// Saves a SaveData object to disk as JSON.
        /// Optionally encrypts the file contents with XOR encryption.
        /// </summary>
        /// <param name="data">The SaveData to serialize and write.</param>
        /// <param name="slotName">The slot name to save to.</param>
        /// <param name="encrypt">Whether to XOR-encrypt the output.</param>
        /// <param name="encryptionKey">The XOR encryption key (required if encrypt is true).</param>
        /// <returns>True if save succeeded, false otherwise.</returns>
        public static bool Save(SaveData_UMFOSS data, string slotName, bool encrypt = false, string encryptionKey = "")
        {
            if (data == null)
            {
                Debug.LogError("[SaveFileHandler] Cannot save null data.");
                return false;
            }

            if (string.IsNullOrEmpty(slotName))
            {
                Debug.LogError("[SaveFileHandler] Slot name cannot be null or empty.");
                return false;
            }

            try
            {
                // Ensure directory exists
                if (!Directory.Exists(SaveDirectory))
                {
                    Directory.CreateDirectory(SaveDirectory);
                    Debug.Log($"[SaveFileHandler] Created save directory: {SaveDirectory}");
                }

                // Manually trigger dictionary packing before serialization (JsonUtility requires this for non-MonoBehaviours)
                if (data.savedObjects != null)
                {
                    data.savedObjects.OnBeforeSerialize();
                }

                // Serialize to JSON
                string json = JsonUtility.ToJson(data, true);

                // Optional encryption
                if (encrypt && !string.IsNullOrEmpty(encryptionKey))
                {
                    json = XOREncryptDecrypt(json, encryptionKey);
                }

                // Write to disk
                string filePath = GetFilePath(slotName);
                File.WriteAllText(filePath, json, Encoding.UTF8);

                Debug.Log($"[SaveFileHandler] Save successful: {filePath} ({new FileInfo(filePath).Length} bytes)");
                return true;
            }
            catch (IOException ex)
            {
                Debug.LogError($"[SaveFileHandler] IO error while saving to slot '{slotName}': {ex.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogError($"[SaveFileHandler] Permission denied while saving to slot '{slotName}': {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveFileHandler] Unexpected error while saving to slot '{slotName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads a SaveData object from disk.
        /// Returns null if the file doesn't exist, is corrupt, or cannot be read.
        /// </summary>
        /// <param name="slotName">The slot name to load from.</param>
        /// <param name="encrypt">Whether the file is XOR-encrypted.</param>
        /// <param name="encryptionKey">The XOR encryption key (required if encrypt is true).</param>
        /// <returns>The deserialized SaveData, or null on failure.</returns>
        public static SaveData_UMFOSS Load(string slotName, bool encrypt = false, string encryptionKey = "")
        {
            if (string.IsNullOrEmpty(slotName))
            {
                Debug.LogError("[SaveFileHandler] Slot name cannot be null or empty.");
                return null;
            }

            string filePath = GetFilePath(slotName);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SaveFileHandler] Save file not found: {filePath}");
                return null;
            }

            try
            {
                // Read file
                string json = File.ReadAllText(filePath, Encoding.UTF8);

                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError($"[SaveFileHandler] Save file is empty: {filePath}");
                    return null;
                }

                // Optional decryption
                if (encrypt && !string.IsNullOrEmpty(encryptionKey))
                {
                    json = XOREncryptDecrypt(json, encryptionKey);
                }

                // Deserialize
                SaveData_UMFOSS data = JsonUtility.FromJson<SaveData_UMFOSS>(json);

                if (data == null)
                {
                    Debug.LogError($"[SaveFileHandler] Failed to deserialize save file: {filePath}");
                    return null;
                }

                // Manually trigger dictionary unpacking after deserialization
                if (data.savedObjects != null)
                {
                    data.savedObjects.OnAfterDeserialize();
                }

                Debug.Log($"[SaveFileHandler] Load successful: {filePath} (version {data.saveVersion})");
                return data;
            }
            catch (IOException ex)
            {
                Debug.LogError($"[SaveFileHandler] IO error while loading slot '{slotName}': {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveFileHandler] Failed to parse save file '{slotName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Loads only the metadata of a save file (version, timestamp, scene)
        /// without deserializing the gameplay data. Fast enough for save slot UI.
        /// </summary>
        /// <param name="slotName">The slot name to read metadata from.</param>
        /// <param name="encrypt">Whether the file is encrypted.</param>
        /// <param name="encryptionKey">The encryption key.</param>
        /// <returns>A SaveData with metadata populated but savedObjects may be empty, or null on failure.</returns>
        public static SaveData_UMFOSS LoadMetadata(string slotName, bool encrypt = false, string encryptionKey = "")
        {
            // For simplicity and safety, we load the full file but the caller
            // should only access the metadata fields. The savedObjects dictionary
            // is populated but not processed — this is still fast for UI display.
            return Load(slotName, encrypt, encryptionKey);
        }

        /// <summary>
        /// Deletes a save file from disk.
        /// </summary>
        /// <param name="slotName">The slot name to delete.</param>
        /// <returns>True if deleted successfully or file didn't exist.</returns>
        public static bool Delete(string slotName)
        {
            if (string.IsNullOrEmpty(slotName))
            {
                Debug.LogError("[SaveFileHandler] Slot name cannot be null or empty.");
                return false;
            }

            string filePath = GetFilePath(slotName);

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[SaveFileHandler] Deleted save file: {filePath}");
                }
                else
                {
                    Debug.Log($"[SaveFileHandler] No save file to delete: {filePath}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveFileHandler] Failed to delete slot '{slotName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks whether a save file exists for the given slot.
        /// </summary>
        public static bool SaveExists(string slotName)
        {
            if (string.IsNullOrEmpty(slotName)) return false;
            return File.Exists(GetFilePath(slotName));
        }

        /// <summary>
        /// Returns a list of all save slot names that have files on disk.
        /// </summary>
        public static List<string> GetAllSaveSlots()
        {
            List<string> slots = new List<string>();

            if (!Directory.Exists(SaveDirectory))
            {
                return slots;
            }

            try
            {
                string[] files = Directory.GetFiles(SaveDirectory, "*" + SAVE_EXTENSION);
                foreach (string file in files)
                {
                    string slotName = Path.GetFileNameWithoutExtension(file);
                    slots.Add(slotName);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveFileHandler] Error listing save slots: {ex.Message}");
            }

            return slots;
        }

        /// <summary>
        /// Simple XOR encryption/decryption.
        /// Not cryptographically strong — intended as anti-tamper for casual games.
        /// The same method is used for both encryption and decryption (XOR is symmetric).
        /// </summary>
        /// <param name="input">The string to encrypt or decrypt.</param>
        /// <param name="key">The encryption key.</param>
        /// <returns>The XOR-processed string.</returns>
        private static string XOREncryptDecrypt(string input, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("[SaveFileHandler] Encryption key is empty. Returning input unchanged.");
                return input;
            }

            StringBuilder sb = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                char c = (char)(input[i] ^ key[i % key.Length]);
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
