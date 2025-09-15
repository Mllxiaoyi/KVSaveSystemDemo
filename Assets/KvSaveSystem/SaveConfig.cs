using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    private static string _userKey = "AllUser";

    public static string UserKey
    {
        get => _userKey;
        set => _userKey = value;
    }
    

    private static string _userArchiveDirectoryPath;

    public static string UserArchiveDirectoryPath
    {
        get
        {
            if (string.IsNullOrEmpty(_userArchiveDirectoryPath))
            {
                _userArchiveDirectoryPath = Path.Combine(Application.persistentDataPath, UserKey);
            }

            return _userArchiveDirectoryPath;
        }
    }

    private const string SAVE_FILE_EXTENSION = ".sav";

    /// <summary>
    /// 获取组别保存路径
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public static string GetGroupFilePath(string groupName) =>
        Path.Combine(UserArchiveDirectoryPath, $"{groupName}{SAVE_FILE_EXTENSION}").Replace('\\', '/');
}