using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Save
{
    public sealed class SaveService : MonoBehaviour
    {
        private const string SaveKey = "TalismanBag.V02.CoreLoop.SaveData";

        [SerializeField] private bool loadOnAwake = true;
        [SerializeField] private bool resetOnEditorPlay = true;

        public SaveData Current { get; private set; }

        public static SaveService Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

#if UNITY_EDITOR
            if (resetOnEditorPlay)
            {
                ResetSave();
            }
#endif

            if (loadOnAwake)
            {
                Load();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static SaveService GetOrCreate()
        {
            if (Instance != null)
            {
                return Instance;
            }

            SaveService existing = FindObjectOfType<SaveService>(true);
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }

            GameObject serviceObject = new("CoreLoopSaveService_Runtime");
            SaveService service = serviceObject.AddComponent<SaveService>();
            Instance = service;
            return service;
        }

        public SaveData EnsureLoaded()
        {
            if (Current == null)
            {
                Load();
            }

            Current.Normalize();
            return Current;
        }

        public void Load()
        {
            // TODO: Add saveVersion migration and a formal save-file path when persistence moves beyond PlayerPrefs.
            if (!HasSave())
            {
                Current = CreateDefaultSaveData();
                return;
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                Current = CreateDefaultSaveData();
                return;
            }

            try
            {
                Current = JsonUtility.FromJson<SaveData>(json) ?? CreateDefaultSaveData();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to load CoreLoop save data. A default save will be used. {exception.Message}");
                Current = CreateDefaultSaveData();
            }

            Current.Normalize();
        }

        public void Save()
        {
            EnsureLoaded();
            string json = JsonUtility.ToJson(Current);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public void ResetSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            Current = CreateDefaultSaveData();
            PlayerPrefs.Save();
        }

        public bool HasSave()
        {
            return PlayerPrefs.HasKey(SaveKey);
        }

        private static SaveData CreateDefaultSaveData()
        {
            SaveData saveData = new();
            saveData.Normalize();
            return saveData;
        }
    }
}
