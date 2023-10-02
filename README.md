# Distributed Caching System
This is a C# implementation of a distributed caching system with support for consistent hashing and LRU (Least Recently Used) cache eviction policy. The system allows you to distribute and manage cached data across multiple nodes.

## Features

- **Consistent Hashing:** Data distribution across nodes is based on a consistent hashing algorithm, ensuring even distribution and minimal data movement when nodes are added or removed.

- **LRU Cache Eviction:** The cache uses an LRU policy to ensure that the cache size does not exceed a specified maximum limit.

- **Node Management:** You can dynamically add and remove nodes from the cache, and the system will redistribute data accordingly.

## Getting Started
To use this distributed caching system in your C# application, follow these steps:

1. **Clone this repository to your local machine:**


   ```bash
	git clone https://github.com/Ugwum/DistributedCaching.git
2. **Include the necessary files (ConsistentHash.cs, IDistributedCache.cs, SimpleDistributedCache.cs, AdvancedDistributedCache.cs) in your project.**

3. **Create instances of the caching classes (SimpleDistributedCache or AdvancedDistributedCache) and use them to cache and retrieve data.**

4. **Customize the cache size and other settings as needed.**

5. **Build and run your application.**

## Usage
Here's an example of how to use the SimpleDistributedCache:


	```bash
	// Create a distributed cache with a maximum cache size of 100 items
	var cache = new SimpleDistributedCache<string>(maxCacheSize: 100);

	// Add nodes to the cache
	cache.AddNode("Node1");
	cache.AddNode("Node2");

	// Set data in the cache
	cache.Set("Key1", "Value1");
	cache.Set("Key2", "Value2");

	// Get data from the cache
	string value = cache.Get("Key1");

	// Remove a node from the cache
	cache.RemoveNode("Node1");

## Contributing
Contributions are welcome! If you'd like to contribute to this project, please follow these guidelines:

1. Fork the repository and create a new branch for your feature or bug fix.

2. Make your changes and test them thoroughly.

3. Write clear and concise commit messages.

4. Submit a pull request with a detailed description of your changes.

## License
This distributed caching system is licensed under the MIT License.

## Credits
This project was created by Obinna Agim .

