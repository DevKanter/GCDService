using GCDService.Managers.Events.EventTypes.Abstract;

namespace GCDService.Managers.Events.Models;

public class GameEventReward
{
    public GameEventReward(int charId, int userId, GameEvent gameEvent)
    {
        CharId = charId;
        UserId = userId;
        RewardInfo = gameEvent.BaseInfo.RewardInfo!;
        GameEvent = gameEvent;
    }
    public GameEvent GameEvent { get; init; }
    public int CharId { get; init; }
    public int UserId { get; init; }
    public GameEventRewardInfo RewardInfo { get; init; }
}