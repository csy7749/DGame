using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace GameLogic
{
    public enum ClientSaveDataStorageMode
    {
        PlayerPrefs,
        JsonFile,
    }

    public abstract class BaseClientSaveData
    {
        private const string JSON_FILE_DIRECTORY = "ClientSaveData";

        private string m_saveKey;
        private ClientSaveDataStorageMode m_storageMode;

        /// <summary>
        /// 初始化保存数据
        /// </summary>
        /// <param name="saveKey">保存数据的键名</param>
        /// <param name="storageMode">保存数据的存储方式</param>
        public void Init(string saveKey, ClientSaveDataStorageMode storageMode)
        {
            m_saveKey = saveKey;
            m_storageMode = storageMode;
            Load();
        }

        /// <summary>
        /// 加载数据
        /// <remarks>子类可重写实现解密</remarks>
        /// </summary>
        protected virtual void Load()
        {
            string jsonStr = ReadJsonFromStorage();

            if (!string.IsNullOrEmpty(jsonStr))
            {
                JsonConvert.PopulateObject(jsonStr, this);
            }
        }

        /// <summary>
        /// 保存数据到本地存储
        /// <remarks>子类可重写实现加密</remarks>
        /// </summary>
        public virtual void Save()
            => WriteJsonToStorage(JsonConvert.SerializeObject(this, Formatting.None));

        protected static T Get<T>() where T : BaseClientSaveData, new()
            => ClientSaveDataMgr.Instance.GetSaveData<T>();

        /// <summary>
        /// 从当前存储后端读取JSON字符串
        /// </summary>
        protected string ReadJsonFromStorage()
        {
            switch (m_storageMode)
            {
                case ClientSaveDataStorageMode.JsonFile:
                    string filePath = GetJsonFilePath();
                    return File.Exists(filePath) ? File.ReadAllText(filePath, Encoding.UTF8) : string.Empty;
                case ClientSaveDataStorageMode.PlayerPrefs:
                default:
                    return DGame.Utility.PlayerPrefsUtil.GetString(m_saveKey);
            }
        }

        /// <summary>
        /// 将JSON字符串写入当前存储后端
        /// </summary>
        protected void WriteJsonToStorage(string jsonStr)
        {
            switch (m_storageMode)
            {
                case ClientSaveDataStorageMode.JsonFile:
                    string filePath = GetJsonFilePath();
                    string directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(filePath, jsonStr, Encoding.UTF8);
                    break;
                case ClientSaveDataStorageMode.PlayerPrefs:
                default:
                    DGame.Utility.PlayerPrefsUtil.SetString(m_saveKey, jsonStr);
                    break;
            }
        }

        /// <summary>
        /// 获取当前存档对应的JSON文件路径
        /// </summary>
        protected string GetJsonFilePath()
            => Path.Combine(Application.persistentDataPath, JSON_FILE_DIRECTORY, $"{GetSafeFileName(m_saveKey)}.json");
        

        private static string GetSafeFileName(string fileName)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }

            return fileName;
        }
    }
}