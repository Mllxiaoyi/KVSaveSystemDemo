using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KVSaveSystem;
using UnityEngine;


public class KvSaveSystemConst
{
    public const string SAVE_PATH_ROOT = "Save";
    
    public const string SAVE_FILE_EXTENSION = ".sav";
    
    private static string PersistentDataPath = Application.persistentDataPath;

    private static string _publicArchiveDirectoryPath;

    /// <summary>
    /// 公共用户存档位置（注意首次调用必须是在 Unity 主线程里）
    /// </summary>
    public static string PublicArchiveDirectoryPath
    {
        get
        {
            if (_publicArchiveDirectoryPath == null)
                _publicArchiveDirectoryPath = Path.Combine(PersistentDataPath, SAVE_PATH_ROOT).Replace('\\', '/');
            return _publicArchiveDirectoryPath;
        }
    }

    private static string _userKey = "AllUser";

    public static string UserKey
    {
        get => _userKey;
        set
        {
            _userKey = value;
            _userArchiveDirectoryPath = Path.Combine(PersistentDataPath, SAVE_PATH_ROOT, UserKey).Replace('\\', '/');
        }
    }

    private static string _userArchiveDirectoryPath;

    /// <summary>
    /// 用户账号存档位置
    /// </summary>
    public static string UserArchiveDirectoryPath => _userArchiveDirectoryPath;
    

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
            archiveSetting = SaveArchiveSettingSO.GetArchiveSetting(groupName);

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