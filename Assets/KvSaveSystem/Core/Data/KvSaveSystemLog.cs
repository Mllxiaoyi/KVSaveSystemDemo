using UnityEngine;

namespace KVSaveSystem
{
    public class SaveSystemLog
    {
        public static void Info(string str)
        {
            Debug.Log(str);
        }

        public static void Error(string err)
        {
            Debug.LogError(err);
        }
    }
}

