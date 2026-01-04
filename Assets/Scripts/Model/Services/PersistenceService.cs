using System.IO;
using SkiGame.Model.Data;
using UnityEngine;

namespace SkiGame.Model.Services
{
    public sealed class PersistenceService
    {
        private const string SAVE_FILE_NAME = "64_dev_map_save.json";

        private PersistenceService() { }

        public static bool SaveExists()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            return File.Exists(path);
        }

        public static void SaveMap(MapSaveData data)
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(path, json);
            Debug.Log($"Map saved to: {path}.");
        }

        public static MapSaveData LoadMap()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            if (!File.Exists(path))
            {
                Debug.LogWarning("No save file found.");
                return null;
            }

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<MapSaveData>(json);
        }
    }
}
