using System;
using System.IO;
using System.Security.Cryptography;

namespace KVSaveSystem
{
    public class ChaCha20Poly1305 : IDisposable
    {
        public ChaCha20Poly1305(byte[] key)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
        }

        public void Encrypt(byte[] nonce, byte[] plaintext, byte[] ciphertext, byte[] tag)
        {
            throw new NotImplementedException();
        }

        public void Decrypt(byte[] nonce, byte[] ciphertext, byte[] tag, byte[] plaintext) => throw new NotImplementedException();
    }
    
    /// <summary>
    /// ChaCha20-Poly1305 加密流包装器
    /// 提供更高的安全性和性能，支持AEAD（认证加密）
    /// </summary>
    public class ChaCha20Stream : Stream
    {
        private readonly Stream _baseStream;
        private readonly ChaCha20Poly1305 _cipher;
        private readonly byte[] _nonce;
        private readonly bool _isWriting;
        private readonly MemoryStream _buffer;
        private bool _headerWritten;
        private bool _disposed;

        public ChaCha20Stream(Stream baseStream, byte[] key, byte[] nonce, bool isWriting)
        {
            _baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            _cipher = new ChaCha20Poly1305(key);
            _nonce = new byte[12];
            Array.Copy(nonce, 0, _nonce, 0, Math.Min(nonce.Length, 12));
            _isWriting = isWriting;
            _buffer = new MemoryStream();
            _headerWritten = false;
        }

        public override bool CanRead => !_isWriting && _baseStream.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => _isWriting && _baseStream.CanWrite;
        public override long Length => throw new NotSupportedException();
        public override long Position 
        { 
            get => throw new NotSupportedException(); 
            set => throw new NotSupportedException(); 
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_isWriting)
                throw new InvalidOperationException("Stream is not writable");

            _buffer.Write(buffer, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_isWriting)
                throw new InvalidOperationException("Stream is not readable");

            // 如果是第一次读取，先解密整个数据
            if (!_headerWritten)
            {
                DecryptData();
                _headerWritten = true;
                _buffer.Position = 0;
            }

            return _buffer.Read(buffer, offset, count);
        }

        public override void Flush()
        {
            if (_isWriting && _buffer.Length > 0)
            {
                EncryptAndWrite();
            }
            _baseStream.Flush();
        }

        private void EncryptAndWrite()
        {
            if (_buffer.Length == 0) return;

            var plaintext = _buffer.ToArray();
            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[16]; // Poly1305 tag size

            _cipher.Encrypt(_nonce, plaintext, ciphertext, tag);

            // 写入格式: nonce(12) + ciphertext + tag(16)
            _baseStream.Write(_nonce, 0, _nonce.Length);
            _baseStream.Write(ciphertext, 0, ciphertext.Length);
            _baseStream.Write(tag, 0, tag.Length);

            _buffer.SetLength(0);
            _headerWritten = true;
        }

        private void DecryptData()
        {
            // 读取 nonce (12 bytes)
            var nonce = new byte[12];
            if (_baseStream.Read(nonce, 0, 12) != 12)
                throw new InvalidDataException("Invalid encrypted data: missing nonce");

            // 读取剩余数据 (ciphertext + tag)
            var remainingLength = (int)(_baseStream.Length - _baseStream.Position);
            if (remainingLength < 16)
                throw new InvalidDataException("Invalid encrypted data: too short");

            var tagLength = 16;
            var ciphertextLength = remainingLength - tagLength;

            var ciphertext = new byte[ciphertextLength];
            var tag = new byte[tagLength];

            if (_baseStream.Read(ciphertext, 0, ciphertextLength) != ciphertextLength)
                throw new InvalidDataException("Invalid encrypted data: incomplete ciphertext");

            if (_baseStream.Read(tag, 0, tagLength) != tagLength)
                throw new InvalidDataException("Invalid encrypted data: incomplete tag");

            // 解密
            var plaintext = new byte[ciphertextLength];
            try
            {
                _cipher.Decrypt(nonce, ciphertext, tag, plaintext);
                _buffer.Write(plaintext, 0, plaintext.Length);
            }
            catch (CryptographicException ex)
            {
                throw new InvalidDataException("Decryption failed: data may be corrupted or tampered", ex);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_isWriting && _buffer.Length > 0)
                {
                    EncryptAndWrite();
                }

                _cipher?.Dispose();
                _buffer?.Dispose();
                _baseStream?.Dispose();
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}