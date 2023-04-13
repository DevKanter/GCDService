namespace GCDService.Managers.Events.Models;

public class GameEventRewardInfo
{
    public EventRewardType RewardType { get; init; }
    public string?[] RewardParams { get; init; } = new string?[10];
}