namespace DistributedCaching
{
    public interface IDistributedCache<T>
    {
        void AddNode(string node);
        T Get(string key);
        void RemoveNode(string node);
        void Set(string key, T value);
    }
}