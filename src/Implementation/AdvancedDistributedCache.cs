using ConsistentHashing;

namespace DistributedCaching.Implementation
{
    public class AdvancedDistributedCache<T> : IDistributedCache<T> where T : class
    {
        private readonly List<string> nodes = new List<string>();
        private readonly Dictionary<string, List<T>> primaryCache = new Dictionary<string, List<T>>();
        private readonly Dictionary<string, List<T>> secondaryCache = new Dictionary<string, List<T>>();
        private readonly ConsistentHash<string> hashRing = new ConsistentHash<string>();
        private readonly Dictionary<string, LinkedList<string>> accessOrder = new Dictionary<string, LinkedList<string>>();
        private readonly int maxCacheSize = 100; // Maximum number of items to keep in the cache.

        public AdvancedDistributedCache(int maxCacheSize = 100)
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
            string primaryNode = hashRing.GetNode(key);
            string secondaryNode = GetSecondaryNode(primaryNode);

            if (!primaryCache.ContainsKey(primaryNode))
            {
                primaryCache[primaryNode] = new List<T>();
                accessOrder[primaryNode] = new LinkedList<string>();
            }

            primaryCache[primaryNode].Add(value);
            accessOrder[primaryNode].AddLast(key);

            // Replicate data to the secondary node
            ReplicateData(key, value, secondaryNode);

            // Check cache size and evict if necessary (LRU policy).
            if (primaryCache[primaryNode].Count > maxCacheSize)
            {
                string oldestKey = accessOrder[primaryNode].First.Value;
                accessOrder[primaryNode].RemoveFirst();
                primaryCache[primaryNode].RemoveAt(0);
                Console.WriteLine($"Cache on primary node '{primaryNode}' exceeded max size, evicted key '{oldestKey}'.");
            }
        }

        public T Get(string key)
        {
            string primaryNode = hashRing.GetNode(key);

            if (primaryCache.ContainsKey(primaryNode))
            {
                List<T> primaryNodeCache = primaryCache[primaryNode];

                string index = accessOrder[primaryNode].Find(key).Value;

                if (!string.IsNullOrEmpty(index))
                {
                    // Move the accessed item to the end of the access order list (LRU).
                    accessOrder[primaryNode].Remove(key);
                    accessOrder[primaryNode].AddLast(key);
                }

                // In a real application, you might want to implement logic to handle cache misses.
                // For simplicity, we assume cache hits in this example.
                return primaryNodeCache[0];
            }

            return default; // Cache miss
        }

        private string GetSecondaryNode(string primaryNode)
        {
            // In a real system, you would have a more advanced logic for secondary node selection.
            // For simplicity, we'll just select the next node in the ring.
            var orderedNodes = nodes.OrderBy(n => n);
            var primaryNodeIndex = orderedNodes.ToList().IndexOf(primaryNode);
            var secondaryNodeIndex = (primaryNodeIndex + 1) % orderedNodes.Count();
            return orderedNodes.ElementAt(secondaryNodeIndex);
        }

        private void ReplicateData(string key, T value, string secondaryNode)
        {
            if (!secondaryCache.ContainsKey(secondaryNode))
            {
                secondaryCache[secondaryNode] = new List<T>();
            }

            secondaryCache[secondaryNode].Add(value);
        }

        private void RedistributeData(string removedNode)
        {
            // Iterate through all keys in the removed node's cache and redistribute them
            foreach (var key in primaryCache[removedNode].SelectMany(k => accessOrder[removedNode]))
            {
                string newNode = hashRing.GetNode(key);
                if (!primaryCache.ContainsKey(newNode))
                {
                    primaryCache[newNode] = new List<T>();
                    accessOrder[newNode] = new LinkedList<string>();
                }

                // Move the data to the new primary node and update access order
                primaryCache[newNode].AddRange(primaryCache[removedNode]);
                accessOrder[newNode].AddLast(key);

                // Check and evict data if necessary on the new primary node
                while (primaryCache[newNode].Count > maxCacheSize)
                {
                    string oldestKey = accessOrder[newNode].First.Value;
                    accessOrder[newNode].RemoveFirst();
                    primaryCache[newNode].RemoveAt(0);
                    Console.WriteLine($"Cache on primary node '{newNode}' exceeded max size, evicted key '{oldestKey}'.");
                }
            }

            // Clear the cache and access order of the removed node
            primaryCache.Remove(removedNode);
            accessOrder.Remove(removedNode);
        }
    }

}