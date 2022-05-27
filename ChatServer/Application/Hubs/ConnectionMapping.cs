using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace ChatServer.Hubs
{
    public class ConnectionMapping<T>
    {
        private readonly ConcurrentDictionary<T, HashSet<string>> _connections =
     new ConcurrentDictionary<T, HashSet<string>>();
        public int Count
        {
            get => _connections.Count;
        }

        public void Add(T key, string connectionId)
        {
            HashSet<string> connections;
            if (!_connections.TryGetValue(key, out connections))
            {
                connections = new HashSet<string>();
                _connections.TryAdd(key, connections);
            }

            lock (connections)
            {
                connections.Add(connectionId);
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(T key, string connectionId)
        {

            HashSet<string> connections;
            if (!_connections.TryGetValue(key, out connections))
            {
                return;
            }

            lock (connections)
            {
                connections.Remove(connectionId);

                if (connections.Count == 0)
                {
                    _connections.TryRemove(key, out _);
                }
            }
        }

    }
}
