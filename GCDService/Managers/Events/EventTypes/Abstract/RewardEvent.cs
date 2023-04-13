using GCDService.Managers.Events.Models;

namespace GCDService.Managers.Events.EventTypes.Abstract;

public abstract class RewardEvent : GameEvent
{
    public RewardEventType RewardEventType { get; init; }

    protected RewardEvent(BaseEventInfo baseInfo) : base(baseInfo)
    {
        if (!baseInfo.IsRewardEvent())
        {
            throw new($"RewardEvent[{baseInfo.EventName}] cant be initialized without a reward!");
        }
    }
    public abstract bool FindRewardable(out List<(int charId, int userId)> rewardable);

    public void DistributeReward(int charId, int userId)
    {
        var reward = new GameEventReward(charId, userId, this);
        GameEventRewardDistributor.DistributeReward(reward);
    }


}