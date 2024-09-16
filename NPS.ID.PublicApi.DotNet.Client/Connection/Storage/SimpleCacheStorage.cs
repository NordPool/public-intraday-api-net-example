using Extend;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Storage;

public class SimpleCacheStorage
{
    private readonly Dictionary<string, List<object>> _data = new();

    public void SetCache<T>(List<T> data, bool overrideValue = false)
    {
        var targetDataTypeKey = typeof(T).ToString();
        if (!_data.TryGetValue(targetDataTypeKey, out _))
        {
            _data.Add(targetDataTypeKey, []);
        }

        var entry = _data[targetDataTypeKey];
        if (overrideValue)
        {
            entry.Clear();
        }
        var dataAsObjects = data.Cast<object>();
        entry.AddRange(dataAsObjects);
    }

    public List<T> GetFromCache<T>()
    {
        var targetDataTypeKey = typeof(T).ToString();
        _data.TryGetValue(targetDataTypeKey, out var dataByType);
        if (dataByType.IsNullOrEmpty())
        {
            return [];
        }

        return _data[targetDataTypeKey]
            .Cast<T>()
            .ToList();
    }
}