using Application.Services.CacheService.Intefaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SharedProject.Models;


namespace Application.Worker;

public class BackgroundTaskService
{
    private readonly ILogger<BackgroundTaskService> _logger;
    private readonly IMapper _mapper;
    private readonly IOrdinaryDistributedCache _ordinaryDistributedCache;
    
    
    private const string KeyIndexSet = "tracked-keys";

    public BackgroundTaskService(ILogger<BackgroundTaskService> logger, IMapper mapper,
        IOrdinaryDistributedCache ordinaryDistributedCache)
    { 
        _logger = logger;
        _mapper = mapper;
        _ordinaryDistributedCache = ordinaryDistributedCache;
    }
    
    private async Task TrackKeyAsync(string key, CancellationToken cancellationToken)
    {
        var existingKeys = await _ordinaryDistributedCache.GetStringAsync(KeyIndexSet, cancellationToken);
        var keysSet = existingKeys != null 
            ? JsonConvert.DeserializeObject<HashSet<string>>(existingKeys) 
            : new HashSet<string>();

        if (!keysSet.Contains(key))
        {
            keysSet.Add(key);
            await _ordinaryDistributedCache.SetStringAsync(KeyIndexSet, JsonConvert.SerializeObject(keysSet), cancellationToken);
        }
    }
    
    private async Task UntrackKeyAsync(string key, CancellationToken cancellationToken)
    {
        var existingKeys = await _ordinaryDistributedCache.GetStringAsync(KeyIndexSet, cancellationToken);
        if (existingKeys != null)
        {
            var keysSet = JsonConvert.DeserializeObject<HashSet<string>>(existingKeys);
            keysSet?.Remove(key);
            await _ordinaryDistributedCache.SetStringAsync(KeyIndexSet, JsonConvert.SerializeObject(keysSet), cancellationToken);
        }
    }
    public async Task ProcessDataAnalyzeOneDay(CancellationToken cancellationToken)
    {
        var trackedKeysJson = await _ordinaryDistributedCache.GetStringAsync(KeyIndexSet, cancellationToken);
        var trackedKeys = trackedKeysJson != null 
            ? JsonConvert.DeserializeObject<HashSet<string>>(trackedKeysJson) 
            : new HashSet<string>();

        var listDataRedisJson = new List<UserDataAnalyseModel>();

        foreach (var key in trackedKeys)
        {
            // Retrieve each value associated with a key
            var value = await _ordinaryDistributedCache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(value))
            {
                // Deserialize the value to your model
                var data = JsonConvert.DeserializeObject<UserDataAnalyseModel>(value);
                if (data != null)
                {
                    listDataRedisJson.Add(data);
                }
            }
        }
        //get kafka data
        //process data
        //mapping to model 
        //produce to user, document, etc
        _logger.LogInformation("Background Service updated ");
    }
    
}
