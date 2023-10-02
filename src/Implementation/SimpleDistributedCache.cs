using ConsistentHashing;

namespace DistributedCaching.Implementation
{
    public class SimpleDistributedCache<T> : IDistributedCache<T>
    {
        private readonly List<string> nodes = new List<string>();
        private readonly Dictionary<string, List<T>> cache = new Dictionary<string, List<T>>();
        private readonly ConsistentHash<string> hashRing = new ConsistentHash<string>();
        private readonly Dictionary<string, LinkedList<string>> accessOrder = new Dictionary<string, LinkedList<string>>();
        private readonly int maxCacheSize = 100; // Maximum number of items to keep in the cache.

        public SimpleDistributedCache(int maxCacheSize = 100)
        {
            this.maxCacheSize = maxCacheSize;
        }

        public void AddNode(string node)
        {
            nodes.Add(node);
            hashRing.AddNode(node);
        }

        public void RemoveNode(string node)
        {
            nodes.Remove(node);
            hashRing.RemoveNode(node);

            // Redistribute data from the removed node to other nodes
            RedistributeData(node);
        }

        public void Set(string key, T value)
        {
            string node = hashRing.GetNode(key);
            if (!cache.ContainsKey(node))
            {
                cache[node] = new List<T>();
                accessOrder[node] = new LinkedList<string>();
            }

            cache[node].Add(value);
            accessOrder[node].AddLast(key);

            // Check cache size and evict if necessary (LRU policy).
            if (cache[node].Count > maxCacheSize)
            {
                string oldestKey = accessOrder[node].First.Value;
                accessOrder[node].RemoveFirst();
                cache[node].RemoveAt(0);
                Console.WriteLine($"Cache on node '{node}' exceeded max size, evicted key '{oldestKey}'.");
            }
        }

        public T Get(string key)
        {
            string node = hashRing.GetNode(key);
            if (cache.ContainsKey(node))
            {
                List<T> nodeCache = cache[node];

                string index = accessOrder[node].Find(key).Value;

                if (!string.IsNullOrEmpty(index))
                {
                    // Move the accessed item to the end of the access order list (LRU).
                    accessOrder[node].Remove(key);
                    accessOrder[node].AddLast(key);
                }

                // In a real application, you might want to implement logic to handle cache misses.
                // For simplicity, we assume cache hits in this example.
                return nodeCache[0];
            }
            return default; // Cache miss
        }

        private void RedistributeData(string removedNode)
        {
            // Iterate through all keys in the removed node's cache and redistribute them
            foreach (var key in cache[removedNode].SelectMany(k => accessOrder[removedNode]))
            {
                string newNode = hashRing.GetNode(key);
                if (!cache.ContainsKey(newNode))
                {
                    cache[newNode] = new List<T>();
                    accessOrder[newNode] = new LinkedList<string>();
                }

                // Move the data to the new node and update access order
                cache[newNode].AddRange(cache[removedNode]);
                accessOrder[newNode].AddLast(key);

                // Check and evict data if necessary on the new node
                while (cache[newNode].Count > maxCacheSize)
                {
                    string oldestKey = accessOrder[newNode].First.Value;
                    accessOrder[newNode].RemoveFirst();
                    cache[newNode].RemoveAt(0);
                    Console.WriteLine($"Cache on node '{newNode}' exceeded max size, evicted key '{oldestKey}'.");
                }
            }

            // Clear the cache and access order of the removed node
            cache.Remove(removedNode);
            accessOrder.Remove(removedNode);
        }
    }

}