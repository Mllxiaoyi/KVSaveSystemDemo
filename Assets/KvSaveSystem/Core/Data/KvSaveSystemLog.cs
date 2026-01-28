using UnityEngine;

namespace KVSaveSystem
{
    public class SaveSystemLog
    {
        /// <summary>
        /// Info 级别日志
        /// </summary>
        /// <param name="str"></param>
        public static void Info(string str)
        {
            Debug.Log(str);
        }

        /// <summary>
        /// error 级别日志
        /// </summary>
        /// <param name="err"></param>
        public static void Error(string err)
        {
            Debug.LogError(err);
        }

        /// <summary>
        /// 性能日志
        /// </summary>
        /// <param name="str"></param>
        public static void Performance(string str)
        {
            Debug.Log(str);
        }
    }
}

