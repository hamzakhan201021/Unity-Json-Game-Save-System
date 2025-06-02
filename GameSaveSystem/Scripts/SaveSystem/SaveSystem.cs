using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HKGameSave
{
    public static class SaveSystem
    {

        // Events...
        public static UnityEvent OnPrepareForSave = new UnityEvent();
        public static UnityEvent OnHandleLoad = new UnityEvent();

        // Max Slots.
        private const int _maxSlots = 4;

        /// <summary>
        /// Save data holder.
        /// </summary>
        public static SaveData SaveDataHolder;

        // -- Path Helpers --

        private static string GetSaveFilePath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, $"slot{slot}.save");
        }

        private static string[] GetSavedSlots()
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            return Directory.GetFiles(Application.persistentDataPath, "slot*.save");
        }

        private static int ExtractSlotNumber(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (name.StartsWith("slot") && int.TryParse(name.Substring(4), out int num))
                return num;
            return -1;
        }

        // -- Save --

        /// <summary>
        /// Use to save asyncrounously
        /// </summary>
        /// <returns></returns>
        public static async Task SaveAsync() => await Save(true);

        /// <summary>
        /// Use to save async without having to await
        /// </summary>
        public static async void SaveAsyncNoAwait() => await Save(true);

        /// <summary>
        /// Use to save instantly
        /// </summary>
        public static void SaveInstant() => Save(false).GetAwaiter().GetResult();

        private static async Task Save(bool useAsync)
        {
            OnPreSave();

            string tempPath = Path.Combine(Application.persistentDataPath, "temp.save");

            if (useAsync)
            {
                await File.WriteAllTextAsync(tempPath, GetEncryptedText());
            }
            else
            {
                File.WriteAllText(tempPath, GetEncryptedText());
            }

            var existingSlots = GetSavedSlots()
                .Select(path => new FileInfo(path))
                .ToList();

            if (existingSlots.Count < _maxSlots)
            {
                // Find next available slot number
                int nextSlot = Enumerable.Range(1, _maxSlots)
                    .First(i => !existingSlots.Any(f => Path.GetFileName(f.FullName) == $"slot{i}.save"));

                string newPath = GetSaveFilePath(nextSlot);
                File.Move(tempPath, newPath);
            }
            else
            {
                // Overwrite the oldest slot
                var oldestFile = existingSlots.OrderBy(f => f.LastWriteTimeUtc).First();
                string overwritePath = oldestFile.FullName;

                File.Delete(overwritePath);
                File.Move(tempPath, overwritePath);
            }
        }

        // -- Load --

        /// <summary>
        /// Use to load asyncrounously
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> LoadAsync(int slot) => await Load(slot, true);

        /// <summary>
        /// Use to load async without having to await
        /// </summary>
        public static async void LoadAsyncNoAwait(int slot) => await Load(slot, true);

        /// <summary>
        /// Use to load instantly
        /// </summary>
        public static bool LoadInstant(int slot) => Load(slot, false).GetAwaiter().GetResult();

        private static async Task<bool> Load(int slot, bool useAsync)
        {
            string path = GetSaveFilePath(slot);

            if (!File.Exists(path))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Slot {slot} doesn't exist.");
#endif
                return false;
            }

            string encrypted;

            if (useAsync)
            {
                encrypted = await File.ReadAllTextAsync(path);
            }
            else
            {
                encrypted = File.ReadAllText(path);
            }

            var decrypted = EncryptionUtils.DecryptString(encrypted);

            if (decrypted.success)
            {
                SaveDataHolder = JsonUtility.FromJson<SaveData>(decrypted.plainText);
            }
            else
            {
                return false;
            }

            OnAfterLoad();

            return true;
        }

        // -- Events --

        private static void OnPreSave()
        {
            OnPrepareForSave.Invoke();
        }

        private static void OnAfterLoad()
        {
            OnHandleLoad.Invoke();
        }

        // -- Helpers --

        /// <summary>
        /// Get Encrypted Data
        /// </summary>
        /// <returns></returns>
        private static string GetEncryptedText()
        {
            string json = JsonUtility.ToJson(SaveDataHolder, true);
            return EncryptionUtils.EncryptString(json);
        }

        /// <summary>
        /// Get Latest Edited Slot Number.
        /// </summary>
        /// <returns></returns>
        public static int GetLatestSlotNumber()
        {
            var existingSlots = GetSavedSlots();
            if (existingSlots.Length == 0) return -1;

            var latest = existingSlots
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .First();

            return ExtractSlotNumber(latest);
        }

        /// <summary>
        /// Use to get all slot files, orders by latest edited.
        /// </summary>
        /// <returns></returns>
        public static (int slot, DateTime? time)[] GetAllSlotInfo()
        {
            return Enumerable.Range(1, _maxSlots)
                .Select(i =>
                {
                    string path = GetSaveFilePath(i);
                    return File.Exists(path)
                        ? (slot: i, time: (DateTime?)File.GetLastWriteTimeUtc(path))
                        : (slot: i, time: null);
                })
                .Where(entry => entry.time != null)
                .OrderByDescending(entry => entry.time)
                .ToArray();
        }

        /// <summary>
        /// Delete Slot via slot number if exists.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static bool DeleteSlot(int slot)
        {
            string path = GetSaveFilePath(slot);

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                Debug.LogError($"Failed to delete slot {slot}: {ex.Message}");
#endif
                return false;
            }
        }

        /// <summary>
        /// Use this to delete all slots saved...
        /// </summary>
        public static void DeleteAllSlots()
        {
            var slotInfos = GetAllSlotInfo();

            foreach (var slotInfo in slotInfos)
            {
                DeleteSlot(slotInfo.slot);
            }
        }
    }

}