namespace GameLogic
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class ClientSaveDataAttribute : System.Attribute
    {
        /// <summary>
        /// 保存数据的键名
        /// </summary>
        public string SaveKey { get; private set; }

        /// <summary>
        /// 是否按角色ID区分存储
        /// </summary>
        public bool PerRoleID { get; private set; }

        /// <summary>
        /// 存储方式
        /// </summary>
        public ClientSaveDataStorageMode StorageMode { get; private set; }

        /// <summary>
        /// 构造客户端保存数据特性
        /// </summary>
        /// <param name="saveKey">保存数据的键名</param>
        /// <param name="perRoleID">是否按角色ID区分存储，默认为false</param>
        /// <param name="storageMode">存储方式，默认使用PlayerPrefs</param>
        public ClientSaveDataAttribute(string saveKey, bool perRoleID = false,
            ClientSaveDataStorageMode storageMode = ClientSaveDataStorageMode.PlayerPrefs)
        {
            SaveKey = saveKey;
            PerRoleID = perRoleID;
            StorageMode = storageMode;
        }

        /// <summary>
        /// 构造客户端保存数据特性
        /// </summary>
        /// <param name="saveKey">保存数据的键名</param>
        /// <param name="storageMode">存储方式</param>
        public ClientSaveDataAttribute(string saveKey, ClientSaveDataStorageMode storageMode)
            : this(saveKey, false, storageMode)
        {
        }
    }
}