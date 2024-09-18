using Extend;
using Microsoft.Extensions.Caching.Memory;

namespace NPS.ID.PublicApi.Client.Connection.Storage;

public class MemoryCacheProxy
{
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    public void SetCache<T>(List<T> data)
    {
        var targetDataTypeKey = typeof(T).ToString();
        
        if (!_memoryCache.TryGetValue<List<T>>(targetDataTypeKey, out var dataByType))
        {
            dataByType = [];
        }
        
        dataByType!.AddRange(data);
        
        _ = _memoryCache.Set(targetDataTypeKey, dataByType);
    }

    public ICollection<T> GetFromCache<T>()
    {
        var targetDataTypeKey = typeof(T).ToString();
        
        _ = _memoryCache.TryGetValue<List<T>>(targetDataTypeKey, out var dataByType);
        
        return dataByType.IsNullOrEmpty() 
            ? [] 
            : dataByType!.ToList();
    }
}