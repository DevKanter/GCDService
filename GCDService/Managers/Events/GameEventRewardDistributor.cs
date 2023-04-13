using GCDService.DB;
using GCDService.Managers.Events.Models;

namespace GCDService.Managers.Events;

public static class GameEventRewardDistributor
{
    private static readonly Dictionary<EventRewardType, Func<GameEventReward, bool>> _distributeAction = new()
    {
        {EventRewardType.ITEM, DistributeItem},
        {EventRewardType.TITLE, DistributeTitle}
    };
    public static bool DistributeReward(GameEventReward reward)
    {
        if (!_distributeAction.TryGetValue(reward.RewardInfo.RewardType, out var action))
        {
            throw new($"Invalid EventRewardAction[{reward.RewardInfo.RewardType}]");
        }

        return action(reward);

    }

    private static bool DistributeTitle(GameEventReward reward)
    {
        var titleId = Convert.ToInt32(reward.RewardInfo.RewardParams[1]);

        if (titleId == 0)
        {
            EventLogger.Log(reward.GameEvent,$"Cant distribute Title with ID 0");
        }

        GameDB.SetTitle(reward.CharId, titleId);
        return false;
    }

    private static bool DistributeItem(GameEventReward reward)
    {
        if (GameDB.GetUserAndCharName(reward.UserId, reward.CharId, out var userName, out var charName) !=
            GameDBResult.SUCCESS)
        {
            EventLogger.Log(reward.GameEvent,$"Username[{reward.UserId}] or CharName[{reward.CharId}] could not be retrieved, while trying to distribute an item!");
            return false;
        }
        var itemCode = Convert.ToInt32(reward.RewardInfo.RewardParams[0]);
        var itemCount = Convert.ToByte(reward.RewardInfo.RewardParams[1]);
        var timeLimit = Convert.ToInt32(reward.RewardInfo.RewardParams[2]);

        return GameDB.InsertEventItem(itemCode, itemCount, reward.UserId, userName, charName, timeLimit) ==
               GameDBResult.SUCCESS;

    }


}