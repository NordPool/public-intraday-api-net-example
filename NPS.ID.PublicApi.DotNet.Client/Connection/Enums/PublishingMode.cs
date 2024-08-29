namespace NPS.ID.PublicApi.DotNet.Client.Connection.Enums;

public enum PublishingMode
{
    /// <summary>
    /// Published all outgoing messages as soon as they appear. 
    /// </summary>
    Streaming,
    
    /// <summary>
    /// Aggregates messages for given time interval publishing only latest versions.
    /// </summary>
    Conflated
}