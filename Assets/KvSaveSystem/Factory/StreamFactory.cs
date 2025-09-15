using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ES3Internal;
using ZeroFormatter;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace KVSaveSystem
{
    public static class StreamFactory
    {
        // 固定的AES密钥 (32字节用于AES-256)
        private static readonly byte[] FixedAESKey = 
        {
            0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
            0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
            0x0F, 0x1E, 0x2D, 0x3C, 0x4B, 0x5A, 0x69, 0x78,
            0x87, 0x96, 0xA5, 0xB4, 0xC3, 0xD2, 0xE1, 0xF0
        };
        
        // 固定的IV (16字节)
        private static readonly byte[] FixedIV = 
        {
            0xF0, 0xE1, 0xD2, 0xC3, 0xB4, 0xA5, 0x96, 0x87,
            0x78, 0x69, 0x5A, 0x4B, 0x3C, 0x2D, 0x1E, 0x0F
        };

        public static ICryptoTransform AESEncryptor
        {
            get
            {
                ICryptoTransform encryptor = null;
                using (var alg = Aes.Create())
                {
                    alg.Mode = CipherMode.CBC;
                    alg.Padding = PaddingMode.PKCS7;
                    alg.Key = FixedAESKey;
                    alg.IV = FixedIV;
                    encryptor = alg.CreateEncryptor();
                }

                return encryptor;
            }
        }

        public static ICryptoTransform AESDecryptor
        {
            get
            {
                ICryptoTransform decryptor = null;
                using (var alg = Aes.Create())
                {
                    alg.Mode = CipherMode.CBC;
                    alg.Padding = PaddingMode.PKCS7;
                    alg.Key = FixedAESKey;
                    alg.IV = FixedIV;
                    decryptor = alg.CreateDecryptor();
                }

                return decryptor;
            }
        }
        
        public static Stream CreateFileStream(string path, FileMode fileMode, FileAccess fileAccess)
        {
            try
            {
                Stream stream = new FileStream(path, fileMode, fileAccess);
                bool isWriteStream = (fileAccess | FileAccess.Write) == FileAccess.Write;
                
                // Encryption
                stream = isWriteStream ? 
                    new CryptoStream(stream, StreamFactory.AESEncryptor, CryptoStreamMode.Write):
                    new CryptoStream(stream, StreamFactory.AESDecryptor, CryptoStreamMode.Read);
                
                // Compression
                // stream = isWriteStream ? 
                //     new GZipStream(stream, CompressionMode.Compress) : 
                //     new GZipStream(stream, CompressionMode.Decompress);

                return stream;
            }
            catch (System.Exception e)
            {
                throw new IOException($"Failed to create file stream for path {path}: {e.Message}", e);
            }
        }
    }
}