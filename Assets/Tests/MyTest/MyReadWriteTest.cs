using System.IO;
using System.Text;
using KVSaveSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class MyReadWriteTest : MonoBehaviour
{
    public bool doWrite = true;

    [Button]
    void StartTest()
    {
        var filePath = SaveSystemConst.GetGroupFilePath("MyReadWrite");
        var bytes = Encoding.UTF8.GetBytes("OKOK");

        if (doWrite)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                // 写入文件头标识
                stream.Write(bytes, 0, bytes.Length);
                // 强制刷新到磁盘
                stream.Flush();
            }
        }

        // 检查文件是否存在且大小符合预期
        if (!File.Exists(filePath))
        {
            Debug.LogError("File does not exist.");
            return;
        }

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length < bytes.Length)
        {
            Debug.LogError("File size is smaller than expected.");
            return;
        }

        // 读取文件内容
        using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            byte[] readBytes = new byte[bytes.Length];
            int bytesRead = stream.Read(readBytes, 0, readBytes.Length);

            if (bytesRead < bytes.Length)
            {
                Debug.LogError("Failed to read the expected number of bytes.");
                return;
            }

            var str = Encoding.UTF8.GetString(readBytes);
            Debug.Log($"Read Str: {str}");
        }
    }
}