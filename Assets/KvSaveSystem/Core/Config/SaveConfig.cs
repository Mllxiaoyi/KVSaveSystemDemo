using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KVSaveSystem;
using UnityEngine;


[CreateAssetMenu(fileName = "SaveConfig", menuName = "ScriptableObject/SaveConfig", order = 1)]
public class SaveConfig : ScriptableObject
{
    private static SaveConfig _instance;
    public static SaveConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<SaveConfig>("SaveConfig");
                if (_instance == null)
                {
                    Debug.LogError("SaveConfig instance not found! Please ensure it exists in Resources folder.");
                    _instance = ScriptableObject.CreateInstance<SaveConfig>();
                }
            }

            return _instance;
        }
    }
    
    public const string SAVE_PATH_ROOT = "Save";
    
    public const string SAVE_FILE_EXTENSION = ".sav";

    public static string PublicArchiveDirectoryPath;

    private static string _userKey = "AllUser";

    public static string UserKey
    {
        get => _userKey;
        set => _userKey = value;
    }

    public static string UserArchiveDirectoryPath;


    private void OnEnable()
    {
        PublicArchiveDirectoryPath = Path.Combine(Application.persistentDataPath, SAVE_PATH_ROOT).Replace('\\', '/');
        UserArchiveDirectoryPath = Path.Combine(Application.persistentDataPath, UserKey).Replace('\\', '/');
    }

    /// <summary>
    /// 获取组别保存路径
    /// </summary>
    /// <param name="groupName">组别名称</param>
    /// <param name="archiveSetting">组别配置</param>
    /// <returns>组别保存路径</returns>
    public static string GetGroupFilePath(string groupName, IArchiveSetting archiveSetting = null)
    {
        string groupFileName = $"{groupName}{SAVE_FILE_EXTENSION}";

        if (archiveSetting == null)
            archiveSetting = ArchiveSettingConfigSO.GetArchiveSetting(groupName);

        string groupDirectoryPath = "";
        if (archiveSetting == null || archiveSetting.IsUserArchive == false)
            groupDirectoryPath = PublicArchiveDirectoryPath;
        else
            groupDirectoryPath = UserArchiveDirectoryPath;

        if (!Directory.Exists(groupDirectoryPath))
            Directory.CreateDirectory(groupDirectoryPath);

        return Path.Combine(groupDirectoryPath, groupFileName).Replace('\\', '/');
    }
}