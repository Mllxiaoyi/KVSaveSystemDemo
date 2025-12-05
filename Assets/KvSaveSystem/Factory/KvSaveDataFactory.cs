namespace KVSaveSystem
{
    public class KvSaveDataFactory
    {
        public static ISaveDataObj GetSaveDataObj<T>(T t)
        {
            var obj = new KvSaveDataObj<T>
            {
                Value = t
            };
            return obj;
        }
    }
}