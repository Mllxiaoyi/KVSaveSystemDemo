namespace KVSaveSystem
{
    /// <summary>
    /// Xor 混淆加密算法
    /// </summary>
    public class XorEncryptionAlgorithm
    {
        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return result;
        }

        public static byte[] Decrypt(byte[] bytes, byte[] key)
        {
            byte[] result = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                result[i] = (byte)(bytes[i] ^ key[i % key.Length]);
            }
            return result;
        }
    }
}