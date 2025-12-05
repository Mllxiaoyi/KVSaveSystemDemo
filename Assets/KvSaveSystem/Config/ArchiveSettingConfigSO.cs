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
                // TODO 接入项目需更改
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
        
        
        
        [LabelText("默认组别设置")]
        public ArchiveSetting defaultSetting = new ArchiveSetting(ArchiveOperationType.Nino);
        
        [LabelText("特殊组别设置")]
        public Dictionary<string, ArchiveSetting> specialGroupSettings = new Dictionary<string, ArchiveSetting>();
        
        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }
    }
}

