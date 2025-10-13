using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroFormatter;

namespace KVSaveSystem
{
    public class ZeroFormatterFileArchiveOperation
    {
        // 文件头标识
        public static readonly byte[] FileHeader = Encoding.UTF8.GetBytes("#FileModeDataArchiveOperation#");

        // 文件尾标识
        public static readonly byte[] FileFooter = Encoding.UTF8.GetBytes("#EndOfGroup#");

        // 文件版本标识
        private static readonly byte[] VersionMarker = { 0x01 }; // 版本1

        // 用于跟踪每个文件的取消令牌源，避免并发写入冲突
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _fileCancellationTokens =
            new ConcurrentDictionary<string, CancellationTokenSource>();

        public static void SaveToDisk(KvSaveDataGroup dataGroup)
        {
            var filePath = SaveConfig.GetGroupFilePath(dataGroup.GroupName);
            var tmpFilePath = filePath + ".bak";

            if (File.Exists(tmpFilePath))
            {
                File.Delete(tmpFilePath);
            }
            
            // if (!Directory.Exists(SaveConfig.UserArchiveDirectoryPath))
            // {
            //     Directory.CreateDirectory(SaveConfig.UserArchiveDirectoryPath);
            // }

            using (Stream stream = StreamFactory.CreateFileStream(tmpFilePath, FileMode.Create, FileAccess.Write))
            {
                // 1. 写入文件头标识
                stream.Write(FileHeader, 0, FileHeader.Length);

                // 2. 写入版本标记
                stream.Write(VersionMarker, 0, VersionMarker.Length);

                // 3. 序列化数据到内存缓冲区
                byte[] serializedData = ZeroFormatterSerializer.Serialize(dataGroup.DataDic);

                // 4. 写入数据长度（4字节）
                byte[] lengthBytes = BitConverter.GetBytes(serializedData.Length);
                stream.Write(lengthBytes, 0, lengthBytes.Length);

                // 5. 写入序列化数据
                stream.Write(serializedData, 0, serializedData.Length);

                // 6. 写入文件尾标识
                stream.Write(FileFooter, 0, FileFooter.Length);

                // 7. 强制刷新到磁盘
                stream.Flush();
            }

            // 原子化
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.Copy(tmpFilePath, filePath);
        }

        public static void LoadFromDisk(string groupName)
        {
            var filePath = SaveConfig.GetGroupFilePath(groupName);
            if (!File.Exists(filePath))
                return;

            using (Stream stream = StreamFactory.CreateFileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // 1. 验证文件头
                byte[] headerBuffer = new byte[FileHeader.Length];
                stream.Read(headerBuffer, 0, headerBuffer.Length);

                if (!headerBuffer.SequenceEqual(FileHeader))
                    throw new InvalidDataException("无效的文件格式：文件头不匹配");

                // 2. 读取版本标记
                byte[] versionBuffer = new byte[VersionMarker.Length];
                stream.Read(versionBuffer, 0, versionBuffer.Length);

                // 这里可以添加版本兼容性处理
                if (!versionBuffer.SequenceEqual(VersionMarker))
                    throw new InvalidDataException("不支持的文件版本");

                // 3. 读取数据长度
                byte[] lengthBuffer = new byte[4];
                stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

                // if (dataLength <= 0 || dataLength > stream.Length - stream.Position - FileFooter.Length)
                //     throw new InvalidDataException("无效的数据长度");

                // 4. 读取序列化数据
                byte[] dataBuffer = new byte[dataLength];
                stream.Read(dataBuffer, 0, dataLength);

                // 5. 验证文件尾
                // long footerPosition = stream.Length - FileFooter.Length;
                // if (stream.Position != footerPosition)
                // {
                //     // 如果当前位置不是文件尾开始位置，说明数据读取有问题
                //     throw new InvalidDataException("文件格式损坏：数据长度不匹配");
                // }

                byte[] footerBuffer = new byte[FileFooter.Length];
                stream.Read(footerBuffer, 0, footerBuffer.Length);

                if (!footerBuffer.SequenceEqual(FileFooter))
                    throw new InvalidDataException("无效的文件格式：文件尾不匹配");

                // 6. 反序列化数据
                var dic = ZeroFormatterSerializer.Deserialize<IDictionary<string, ISaveDataObj>>(dataBuffer);
                if (dic == null)
                {
                    throw new InvalidDataException("数据反序列化失败");
                }

                foreach (var kv in dic)
                {
                    switch (kv.Value.TypeKey)
                    {
                        case 1:
                            if (kv.Value is FloatKvSaveDataObj floatObj)
                                KvSaveSystem.SetValue(kv.Key, floatObj.Value, groupName);
                            break;
                        case 2:
                            if (kv.Value is StringKvSaveDataObj stringObj)
                                KvSaveSystem.SetValue(kv.Key, stringObj.Value, groupName);
                            break;
                        case 3:
                            if (kv.Value is IntKvSaveDataObj intObj)
                                KvSaveSystem.SetValue(kv.Key, intObj.Value, groupName);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public static async Task SaveToDiskAsync(KvSaveDataGroup dataGroup,
            CancellationToken cancellationToken = default)
        {
            var filePath = SaveConfig.GetGroupFilePath(dataGroup.GroupName);
            var tmpFilePath = filePath + ".bak";

            // 取消之前对同一文件的写入操作，避免并发冲突
            if (_fileCancellationTokens.TryGetValue(filePath, out var previousCts))
            {
                previousCts.Cancel();
                previousCts.Dispose();
            }

            // 创建新的取消令牌源，并与外部传入的令牌组合
            var currentCts = new CancellationTokenSource();
            _fileCancellationTokens[filePath] = currentCts;

            // 组合外部令牌和内部令牌
            using var combinedCts =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, currentCts.Token);
            var combinedToken = combinedCts.Token;

            try
            {
                // 检查取消请求
                combinedToken.ThrowIfCancellationRequested();

                // 1. 在后台线程中执行序列化操作（CPU密集型任务）
                byte[] serializedData = await Task.Run(() =>
                {
                    combinedToken.ThrowIfCancellationRequested();
                    return ZeroFormatterSerializer.Serialize(dataGroup.DataDic);
                }, combinedToken).ConfigureAwait(false);

                combinedToken.ThrowIfCancellationRequested();

                // 2. 在后台线程中执行文件操作
                await Task.Run(async () =>
                {
                    if (File.Exists(tmpFilePath))
                    {
                        File.Delete(tmpFilePath);
                    }

                    combinedToken.ThrowIfCancellationRequested();

                    using (Stream stream = StreamFactory.CreateFileStream(tmpFilePath, FileMode.Create, FileAccess.Write))
                    {
                        // 写入文件头标识
                        await stream.WriteAsync(FileHeader, 0, FileHeader.Length, combinedToken).ConfigureAwait(false);

                        // 写入版本标记
                        await stream.WriteAsync(VersionMarker, 0, VersionMarker.Length, combinedToken).ConfigureAwait(false);

                        // 写入数据长度（4字节）
                        byte[] lengthBytes = BitConverter.GetBytes(serializedData.Length);
                        await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length, combinedToken).ConfigureAwait(false);

                        // 写入序列化数据（大数据块）
                        await stream.WriteAsync(serializedData, 0, serializedData.Length, combinedToken).ConfigureAwait(false);

                        // 写入文件尾标识
                        await stream.WriteAsync(FileFooter, 0, FileFooter.Length, combinedToken).ConfigureAwait(false);

                        // 强制刷新到磁盘
                        await stream.FlushAsync(combinedToken).ConfigureAwait(false);
                    }

                    combinedToken.ThrowIfCancellationRequested();

                    // 原子化替换文件
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    File.Copy(tmpFilePath, filePath);

                    // 清理临时文件
                    if (File.Exists(tmpFilePath))
                    {
                        File.Delete(tmpFilePath);
                    }

                }, combinedToken).ConfigureAwait(false);
            }
            finally
            {
                // 操作完成后清理令牌源
                if (_fileCancellationTokens.TryGetValue(filePath, out var cts) && cts == currentCts)
                {
                    _fileCancellationTokens.TryRemove(filePath, out _);
                    currentCts.Dispose();
                }
            }
        }

        public static async Task LoadFromDiskAsync(string groupName, CancellationToken cancellationToken = default)
        {
            var filePath = SaveConfig.GetGroupFilePath(groupName);

            // 检查取消请求
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(filePath))
                return;

            // 检查取消请求
            cancellationToken.ThrowIfCancellationRequested();

            using (Stream stream = StreamFactory.CreateFileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // 1. 验证文件头 - 可取消的异步读取
                byte[] headerBuffer = new byte[FileHeader.Length];
                await stream.ReadAsync(headerBuffer, 0, headerBuffer.Length, cancellationToken);

                if (!headerBuffer.SequenceEqual(FileHeader))
                    throw new InvalidDataException("无效的文件格式：文件头不匹配");

                // 2. 读取版本标记 - 可取消的异步读取
                byte[] versionBuffer = new byte[VersionMarker.Length];
                await stream.ReadAsync(versionBuffer, 0, versionBuffer.Length, cancellationToken);

                // 这里可以添加版本兼容性处理
                if (!versionBuffer.SequenceEqual(VersionMarker))
                    throw new InvalidDataException("不支持的文件版本");

                // 3. 读取数据长度 - 可取消的异步读取
                byte[] lengthBuffer = new byte[4];
                await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length, cancellationToken);
                int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

                if (dataLength <= 0 || dataLength > stream.Length - stream.Position - FileFooter.Length)
                    throw new InvalidDataException("无效的数据长度");

                // 4. 读取序列化数据 - 可取消的异步读取（大数据块）
                byte[] dataBuffer = new byte[dataLength];
                await stream.ReadAsync(dataBuffer, 0, dataLength, cancellationToken);

                // 5. 验证文件尾
                long footerPosition = stream.Length - FileFooter.Length;
                if (stream.Position != footerPosition)
                {
                    // 如果当前位置不是文件尾开始位置，说明数据读取有问题
                    throw new InvalidDataException("文件格式损坏：数据长度不匹配");
                }

                // 读取文件尾 - 可取消的异步读取
                byte[] footerBuffer = new byte[FileFooter.Length];
                await stream.ReadAsync(footerBuffer, 0, footerBuffer.Length, cancellationToken);

                if (!footerBuffer.SequenceEqual(FileFooter))
                    throw new InvalidDataException("无效的文件格式：文件尾不匹配");

                // 6. 反序列化数据
                var dic = ZeroFormatterSerializer.Deserialize<IDictionary<string, ISaveDataObj>>(dataBuffer);
                if (dic == null)
                {
                    throw new InvalidDataException("数据反序列化失败");
                }

                cancellationToken.ThrowIfCancellationRequested();

                foreach (var kv in dic)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    switch (kv.Value.TypeKey)
                    {
                        case 1:
                            if (kv.Value is FloatKvSaveDataObj floatObj)
                                KvSaveSystem.SetValue(kv.Key, floatObj.Value, groupName);
                            break;
                        case 2:
                            if (kv.Value is StringKvSaveDataObj stringObj)
                                KvSaveSystem.SetValue(kv.Key, stringObj.Value, groupName);
                            break;
                        case 3:
                            if (kv.Value is IntKvSaveDataObj intObj)
                                KvSaveSystem.SetValue(kv.Key, intObj.Value, groupName);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}