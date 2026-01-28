using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace KVSaveSystem
{
    [CreateAssetMenu(fileName = "SaveArchiveSettingSO", menuName = "ScriptableObject/KVSaveSystem/SaveArchiveSettingSO")]
    public class SaveArchiveSettingSO : SerializedScriptableObject, ISaveArchiveSettingProvider
    {
        [LabelText("是否开启加密")]
        [SerializeField]
        private bool enableEncrypt = true;

        /// <summary>
        /// 实现 ISaveArchiveSettingProvider 接口
        /// </summary>
        public bool EnableEncrypt => enableEncrypt;
        
        [LabelText("默认组别设置")]
        public ArchiveSetting defaultSetting = new ArchiveSetting(ArchiveOperationType.Nino);
        
        [LabelText("特殊组别设置")]
        public Dictionary<string, ArchiveSetting> specialGroupSettings = new Dictionary<string, ArchiveSetting>();
        
        public IArchiveSetting GetArchiveSetting(string groupName, bool onlySpecial = false)
        {
            if (specialGroupSettings.TryGetValue(groupName, out var setting))
            {
                return setting;
            }
            return onlySpecial ? null : defaultSetting;
        }
        
        
        [LabelText("开启双写（设置值时同样设置 PlayerPrefs 的值")]
        [SerializeField]
        private bool isOpenDoubleWrite;

        /// <summary>
        /// 实现 ISaveArchiveSettingProvider 接口
        /// </summary>
        public bool IsOpenDoubleWrite => isOpenDoubleWrite;

        public void Init()
        {
            
        }
    }
}

