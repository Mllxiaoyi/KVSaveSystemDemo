using System.Collections.Generic;
//TODO using BaseFramework.Config;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace KVSaveSystem
{
    [CreateAssetMenu(fileName = "SaveArchiveSettingSO", menuName = "ScriptableObject/KVSaveSystem/SaveArchiveSettingSO")]
    public class SaveArchiveSettingSO : SerializedScriptableObject
    {
        private static SaveArchiveSettingSO _instance;
        private static readonly object _instanceLock = new object();
        private const string ASSET_PATH = "Assets/GameResources/Setting/SaveArchiveSetting.asset";
    
        public static SaveArchiveSettingSO Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = LoadOrCreateInstance();
                        }
                    }
                }
                return _instance;
            }
        }
        
        private static SaveArchiveSettingSO LoadOrCreateInstance()
        {
#if UNITY_EDITOR
            var instance = AssetDatabase.LoadMainAssetAtPath(ASSET_PATH) as SaveArchiveSettingSO;
            if (instance == null)
            {
                instance = CreateInstance<SaveArchiveSettingSO>();
                AssetDatabase.CreateAsset(instance, ASSET_PATH);
                AssetDatabase.SaveAssets();
            }
            return instance;
#else
            var instance = InGameResourceFactory.Instance.LoadAsset<SaveArchiveSettingSO>(ASSET_PATH);
            if (instance == null)
            {
                BaseFramework.Log.Error("错误，找不到ArchiveSettingConfigSO！");
                instance = CreateInstance<SaveArchiveSettingSO>();
            }
            return instance;
#endif
        }

        
        [LabelText("是否开启加密")]
        public bool enableEncrypt = true;
        
        [LabelText("默认组别设置")]
        public ArchiveSetting defaultSetting = new ArchiveSetting(ArchiveOperationType.Nino);
        
        [LabelText("特殊组别设置")]
        public Dictionary<string, ArchiveSetting> specialGroupSettings = new Dictionary<string, ArchiveSetting>();
        
        public static IArchiveSetting GetArchiveSetting(string groupName, bool onlySpecial = false)
        {
            if (Instance)
            {
                if (Instance.specialGroupSettings.TryGetValue(groupName, out var setting))
                {
                    return setting;
                }
                return onlySpecial ? null : Instance.defaultSetting;
            }

            return onlySpecial ? null : new ArchiveSetting(ArchiveOperationType.Nino);
        }
        
        
        [LabelText("开启双写（设置值时同样设置 PlayerPrefs 的值")]
        public bool isOpenDoubleWrite;

        public void Init()
        {
            
        }
    }
}

