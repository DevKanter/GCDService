namespace GCDService.Managers.Events.Models;

public class BaseEventInfo
{
    public required int EventID { get; init; }
    public required EventType EventType { get; init; }
    public required string EventName { get; init; }
    public required string EventDescription { get; init; }
    public required DateTime EventBeginTime { get; init; }
    public required DateTime EventEndTime { get; init; }
    public required string?[] EventParams { get; init; } = new string?[10];
    public GameEventRewardInfo? RewardInfo { get; init; }

    public bool IsRewardEvent()
    {
        return RewardInfo != null;
    }
}