using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KVSaveSystem
{
    [CreateAssetMenu(fileName = "ArchiveSettingConfig", menuName = "ScriptableObject/KVSaveSystem/ArchiveSettingConfig")]
    public class ArchiveSettingConfigSO : SerializedScriptableObject
    {
        private static ArchiveSettingConfigSO _instance;
        
        public static ArchiveSettingConfigSO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ArchiveSettingConfigSO>("ArchiveSettingConfig");
                    
                    if (_instance == null)
                    {
                        Debug.LogError("ArchiveSettingConfig not found in Resources folder. Please create one or ensure it's in a Resources folder.");
                    }
                }
                return _instance;
            }
        }
        
        [LabelText("默认组别设置")]
        public ArchiveSetting defaultSetting = new ArchiveSetting(ArchiveOperationType.ZeroFormatterFile, false);
        
        [LabelText("特殊组别设置")]
        public Dictionary<string, ArchiveSetting> specialGroupSettings = new Dictionary<string, ArchiveSetting>();
        
        public static IArchiveSetting GetArchiveSetting(string groupName)
        {
            if (Instance)
            {
                if (_instance.specialGroupSettings != null && _instance.specialGroupSettings.ContainsKey(groupName))
                {
                    return _instance.specialGroupSettings[groupName];
                }
                return _instance.defaultSetting;
            }

            return new ArchiveSetting(ArchiveOperationType.ZeroFormatterFile, false);
        }
        
        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }
    }
}

