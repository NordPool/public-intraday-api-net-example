namespace NPS.ID.PublicApi.Client.Connection.Subscriptions;

public interface ISubscription
{
    public string Id { get; }
    public string Type { get; }
    public string Destination { get; }
}
