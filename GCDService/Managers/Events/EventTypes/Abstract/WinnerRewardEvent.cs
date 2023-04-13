using GCDService.Managers.Events.Models;

namespace GCDService.Managers.Events.EventTypes.Abstract;

public abstract class WinnerRewardEvent : RewardEvent
{
    private readonly int _maxWinnerCount;
    private int _winnerCount;
    protected WinnerRewardEvent(BaseEventInfo baseInfo) : base(baseInfo)
    {
        RewardEventType = RewardEventType.FIXED_WINNER_COUNT;
        _maxWinnerCount = Convert.ToInt32(baseInfo.RewardInfo!.RewardParams[0]);
        if (_maxWinnerCount == 0)
            throw new Exception($"WinnerRewardEvent[{baseInfo.EventName}] cant have 0 maxWinners");
    }
    public override void Update()
    {
        if (FindRewardable(out var rewardables))
        {
            foreach (var rewardable in rewardables)
            {
                DistributeReward(rewardable.charId, rewardable.userId);
                _winnerCount++;
                if (_winnerCount >= _maxWinnerCount)
                    Stop(EventStopReason.ALL_WINNERS_FOUND);
            }
        }
        base.Update();
    }

}