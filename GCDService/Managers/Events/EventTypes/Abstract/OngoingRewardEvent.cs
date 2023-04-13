using GCDService.Managers.Events.Models;

namespace GCDService.Managers.Events.EventTypes.Abstract;

public abstract class OngoingRewardEvent : RewardEvent
{
    protected OngoingRewardEvent(BaseEventInfo baseInfo) : base(baseInfo)
    {
        RewardEventType = RewardEventType.ONGOING;
    }
    public override void Update()
    {
        if (FindRewardable(out var rewardables))
        {
            foreach (var rewardable in rewardables)
            {
                DistributeReward(rewardable.charId, rewardable.userId);
            }
        }
        base.Update();
    }
}