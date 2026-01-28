using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nino.Core;

namespace KVSaveSystem
{
    public class FileArchiveOperation
    {
        // 文件头标识
        public static readonly byte[] FileHeader = Encoding.UTF8.GetBytes("#FileArchive#");

        // 文件版本标识
        private static readonly byte[] VersionMarker = { 0x01 }; // 版本1

        // 文件尾标识
        public static readonly byte[] FileFooter = Encoding.UTF8.GetBytes("#EndOfGroup#");

        // 文件级锁，防止并发访问同一文件
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new();

        // 全局请求计数器，用于生成唯一的请求ID
        private static long _globalRequestId;

        private static readonly byte[] XorKey = Encoding.UTF8.GetBytes("pandada");

        public static byte[] GetBytes(KvSaveDataGroup group)
        {
            var dic = group.DataDic;
            // 1. 序列化
            byte[] serializedData = NinoSerializer.Serialize(dic);
            // 2. 加密
            if (SaveArchiveSettingProvider.Current != null && SaveArchiveSettingProvider.Current.EnableEncrypt) 
                serializedData = XorEncryptionAlgorithm.Encrypt(serializedData, XorKey);

            return serializedData;
        }
        
        public static IDictionary<string, ISaveDataObj> ParseBytes(byte[] data)
        {
            byte[] decryptedData = data;
            // 1. 解密
            if (SaveArchiveSettingProvider.Current != null && SaveArchiveSettingProvider.Current.EnableEncrypt)
                decryptedData = XorEncryptionAlgorithm.Decrypt(data, XorKey);
            // 2. 反序列化
            IDictionary<string, ISaveDataObj> dic = NinoDeserializer.Deserialize<ConcurrentDictionary<string, ISaveDataObj>>(decryptedData);
            
            return dic;
        }

        public static void SaveToDisk(KvSaveDataGroup dataGroup)
        {
            var filePath = SaveSystemConst.GetGroupFilePath(dataGroup.GroupName);
            var tmpFilePath = filePath + ".bak";

            if (File.Exists(tmpFilePath))
                File.Delete(tmpFilePath);

            byte[] serializedData = GetBytes(dataGroup);

            using (Stream stream = new FileStream(tmpFilePath, FileMode.Create, FileAccess.Write))
            {
                // 写入文件头标识
                stream.Write(FileHeader, 0, FileHeader.Length);
                // 写入版本标记
                stream.Write(VersionMarker, 0, VersionMarker.Length);
                // 写入数据长度（4字节）
                byte[] lengthBytes = BitConverter.GetBytes(serializedData.Length);
                stream.Write(lengthBytes, 0, lengthBytes.Length);
                // 写入序列化数据（大数据块）
                stream.Write(serializedData, 0, serializedData.Length);
                // 写入文件尾标识
                stream.Write(FileFooter, 0, FileFooter.Length);
                // 强制刷新到磁盘
                stream.Flush();
            }

            // 原子化
            AtomicFileReplace(tmpFilePath, filePath);
        }

        /// <summary>
        /// 异步保存数据快照到磁盘，支持并发和取消 Token
        /// </summary>
        /// <param name="dataGroup"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="TimeoutException"></exception>
        public static async Task SaveToDiskAsync(KvSaveDataGroup dataGroup, CancellationToken cancellationToken)
        {
            var groupName = dataGroup.GroupName;
            var filePath = dataGroup.FilePath;
            var requestId = Interlocked.Increment(ref _globalRequestId);
            var tmpFilePath = $"{filePath}.tmp_{requestId}"; // 使用唯一请求ID作为临时文件名避免冲突

            if (File.Exists(tmpFilePath))
                File.Delete(tmpFilePath);

            // 序列化 + 加密
            cancellationToken.ThrowIfCancellationRequested();
            byte[] serializedData = GetBytes(dataGroup);
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // 获取文件级锁（每个文件一个锁）
                var fileLock = _fileLocks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));

                // 写入临时文件
                await using (var stream = new FileStream(tmpFilePath, FileMode.Create, FileAccess.Write))
                {
                    // 写入文件头标识
                    await stream.WriteAsync(FileHeader, 0, FileHeader.Length, cancellationToken);
                    // 写入版本标记
                    await stream.WriteAsync(VersionMarker, 0, VersionMarker.Length, cancellationToken);
                    // 写入数据长度（4字节）
                    byte[] lengthBytes = BitConverter.GetBytes(serializedData.Length);
                    await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length, cancellationToken);
                    // 写入序列化数据（大数据块）
                    await stream.WriteAsync(serializedData, 0, serializedData.Length, cancellationToken);
                    // 写入文件尾标识
                    await stream.WriteAsync(FileFooter, 0, FileFooter.Length, cancellationToken);
                    // 强制刷新到磁盘
                    await stream.FlushAsync(cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();

                // 使用文件锁保护原子替换操作
                if (await fileLock.WaitAsync(1000, cancellationToken)) // 1秒超时
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        AtomicFileReplace(tmpFilePath, filePath);
                    }
                    finally
                    {
                        fileLock.Release();
                    }
                }
                else
                {
                    throw new TimeoutException($"Failed to acquire file lock for {groupName} within 1 second");
                }
            }
            finally
            {
                // 清理临时文件（如果替换失败）
                if (File.Exists(tmpFilePath))
                    File.Delete(tmpFilePath);
            }
        }

        /// <summary>
        /// 原子文件替换方法，优先使用 File.Replace，降级使用 File.Move
        /// </summary>
        /// <param name="sourceFile">源文件路径</param>
        /// <param name="targetFile">目标文件路径</param>
        private static void AtomicFileReplace(string sourceFile, string targetFile)
        {
            try
            {
                if (File.Exists(targetFile))
                    File.Replace(sourceFile, targetFile, null);
                else
                    File.Move(sourceFile, targetFile);
            }
            catch (PlatformNotSupportedException)
            {
                SaveSystemLog.Info($"File.Replace not supported, falling back to File.Move: {Path.GetFileName(targetFile)}");
                FallbackFileReplace(sourceFile, targetFile);
            }
            catch (UnauthorizedAccessException)
            {
                SaveSystemLog.Info($"File.Replace access denied, falling back to File.Move: {Path.GetFileName(targetFile)}");
                FallbackFileReplace(sourceFile, targetFile);
            }
        }

        /// <summary>
        /// 降级的文件替换方案
        /// </summary>
        private static void FallbackFileReplace(string sourceFile, string targetFile)
        {
            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }

            File.Move(sourceFile, targetFile);
        }

        /// <summary>
        /// 从磁盘加载数据到数据组
        /// </summary>
        /// <param name="dataGroup">要加载数据的目标数据组</param>
        /// <param name="filePath">文件路径</param>
        public static void LoadFromDisk(KvSaveDataGroup dataGroup, string filePath)
        {
            if (!File.Exists(filePath))
                return;

            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // 读取文件头
                byte[] headerBuffer = new byte[FileHeader.Length];
                int headerBytesRead = stream.Read(headerBuffer, 0, FileHeader.Length);

                // 读取版本
                byte[] versionBuffer = new byte[VersionMarker.Length];
                int versionBytesRead = stream.Read(versionBuffer, 0, VersionMarker.Length);
                if (versionBytesRead != VersionMarker.Length || !ArrayEquals(versionBuffer, VersionMarker))
                {
                    throw new InvalidDataException($"不支持的文件版本，组名：{dataGroup.GroupName}");
                }

                // 读取数据长度
                byte[] lengthBuffer = new byte[4];
                int lengthBytesRead = stream.Read(lengthBuffer, 0, 4);
                if (lengthBytesRead != 4)
                {
                    throw new InvalidDataException($"无法读取数据长度，组名：{dataGroup.GroupName}");
                }

                int dataLength = BitConverter.ToInt32(lengthBuffer, 0);
                // 数据长度合理性检查
                if (dataLength < 0 || dataLength > 100 * Math.Pow(2, 20)) // 100MB 限制
                {
                    throw new InvalidDataException($"数据长度异常：{dataLength} bytes，组名：{dataGroup.GroupName}");
                }

                // 读取序列化数据
                byte[] encryptedData = new byte[dataLength];
                int totalBytesRead = 0;
                while (totalBytesRead < dataLength)
                {
                    int bytesRead = stream.Read(encryptedData, totalBytesRead, dataLength - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        throw new InvalidDataException($"文件数据不完整，组名：{dataGroup.GroupName}，期望 {dataLength} 字节，实际读取 {totalBytesRead} 字节");
                    }

                    totalBytesRead += bytesRead;
                }

                // 验证文件尾
                long remainingBytes = stream.Length - stream.Position;
                if (remainingBytes >= FileFooter.Length)
                {
                    byte[] footerBuffer = new byte[FileFooter.Length];
                    int footerBytesRead = stream.Read(footerBuffer, 0, FileFooter.Length);
                    if (footerBytesRead == FileFooter.Length && !ArrayEquals(footerBuffer, FileFooter))
                    {
                        SaveSystemLog.Error($"文件尾标识不匹配，但继续加载，组名：{dataGroup.GroupName}");
                    }
                }
                
                // 解密 + 反序列化
                var loadedDic = ParseBytes(encryptedData);

                if (loadedDic != null)
                {
                    // 清空现有数据并加载新数据
                    dataGroup.DataDic.Clear();
                    foreach (var kvp in loadedDic)
                    {
                        dataGroup.DataDic[kvp.Key] = kvp.Value;
                    }
                    SaveSystemLog.Info($"成功加载组数据：{dataGroup.GroupName}，包含 {loadedDic.Count} 个键值对");
                }
                else
                {
                    SaveSystemLog.Error($"反序列化结果为空，组名：{dataGroup.GroupName}");
                }
            }
        }

        /// <summary>
        /// 比较两个字节数组是否相等
        /// </summary>
        private static bool ArrayEquals(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }

            return true;
        }
    }
}