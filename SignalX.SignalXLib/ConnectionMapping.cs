namespace SignalXLib.Lib
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class ConnectionMapping<T>
    {
        private readonly ConcurrentDictionary<T, HashSet<string>> _connections = new ConcurrentDictionary<T, HashSet<string>>();

        public int Count => this._connections.Count;

        public string FirstOrDefault()
        {
            return this.GetAll()?.FirstOrDefault();
        }

        public void Add(SignalX SignalX, T key, string connectionId)
        {
            if (!SignalX.Settings.ManageUserConnections)
                return;
            HashSet<string> connections;
            if (!this._connections.TryGetValue(key, out connections))
            {
                connections = new HashSet<string>();
                this._connections.GetOrAdd(key, connections);
            }

            connections.Add(connectionId);
        }

        public IEnumerable<string> GetByKey(T key)
        {
            HashSet<string> connections;
            if (this._connections.TryGetValue(key, out connections))
                return connections;

            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetAll()
        {
            return this._connections.Keys.SelectMany(x => this.GetByKey(x));
        }

        public void Remove(SignalX SignalX, T key, string connectionId)
        {
            if (!SignalX.Settings.ManageUserConnections)
                return;
            HashSet<string> connections;
            if (!this._connections.TryGetValue(key, out connections))
                return;
            connections.Remove(connectionId);

            if (connections.Count == 0)
                this._connections.TryRemove(key, out connections);
        }

        public void RemoveAll(SignalX SignalX)
        {
            this._connections.Clear();
            SignalX.Settings.HasOneOrMoreConnections = false;
        }
    }
}