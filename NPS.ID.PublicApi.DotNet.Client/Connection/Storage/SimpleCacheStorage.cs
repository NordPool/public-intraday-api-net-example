using Extend;
using NPS.ID.PublicApi.DotNet.Client.Connection.Enums;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Storage;

public class SimpleCacheStorage
{
    private readonly Dictionary<WebSocketClientTarget, Dictionary<string, List<object>>> _data = new();

    public void SetCache<T>(WebSocketClientTarget clientTarget, List<T> data, bool overrideValue = false)
    {
        if (!_data.TryGetValue(clientTarget, out _))
        {
            _data.Add(clientTarget, new Dictionary<string, List<object>>());
        }

        var targetDataTypeKey = typeof(T).ToString();
        if (!_data[clientTarget].TryGetValue(targetDataTypeKey, out _))
        {
            _data[clientTarget].Add(targetDataTypeKey, []);
        }

        var entry = _data[clientTarget][targetDataTypeKey];
        if (overrideValue)
        {
            entry.Clear();
        }
        var dataAsObjects = data.Cast<object>();
        entry.AddRange(dataAsObjects);
    }

    public List<T> GetFromCache<T>(WebSocketClientTarget clientTarget)
    {
        _data.TryGetValue(clientTarget, out var dataByClient);
        if (dataByClient.IsNullOrEmpty())
        {
            return [];
        }
        
        var targetDataTypeKey = typeof(T).ToString();
        _data[clientTarget].TryGetValue(targetDataTypeKey, out var dataByType);
        if (dataByType.IsNullOrEmpty())
        {
            return [];
        }

        return _data[clientTarget][targetDataTypeKey]
            .Cast<T>()
            .ToList();
    }
}