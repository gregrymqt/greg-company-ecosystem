using System.Collections.Concurrent;
using System.Linq;

namespace MeuCrudCsharp.Features.Hubs
{
    public class ConnectionMapping<T>
        where T : notnull
    {
        private readonly ConcurrentDictionary<T, HashSet<string>> _keyToConnections = new();

        private readonly ConcurrentDictionary<string, T> _connectionToKey = new();

        public void Add(T key, string connectionId)
        {
            _connectionToKey.TryAdd(connectionId, key);

            var connections = _keyToConnections.GetOrAdd(key, _ => new HashSet<string>());
            lock (connections)
            {
                connections.Add(connectionId);
            }
        }

        public T? GetKey(string connectionId)
        {
            return _connectionToKey.TryGetValue(connectionId, out var key) ? key : default;
        }

        public IEnumerable<string> GetConnections(T key)
        {
            return _keyToConnections.TryGetValue(key, out var connections)
                ? connections
                : Enumerable.Empty<string>();
        }

        public void Remove(string connectionId)
        {
            if (!_connectionToKey.TryRemove(connectionId, out var key))
            {
                return;
            }

            if (!_keyToConnections.TryGetValue(key, out var connections)) return;
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _keyToConnections.TryRemove(key, out _);
                }
            }
        }
    }
}
