using GCDService.DB;
using GCDService.Managers.Events.EventTypes.Abstract;
using GCDService.Managers.Events.Models;

namespace GCDService.Managers.Events.EventTypes;

public class LevelRaceEvent : WinnerRewardEvent
{
    private readonly int _level;
    public LevelRaceEvent(BaseEventInfo baseInfo) : base(baseInfo)
    {
        _level = Convert.ToInt32(baseInfo.EventParams[0]);
        if (baseInfo.EventType != EventType.LEVEL_RACE_EVENT)
            throw new ArgumentException(
                $"EventType[{baseInfo.EventType}] doesn't match Constructor[{nameof(LevelRaceEvent)}]");
    }

    public override bool FindRewardable(out List<(int charId, int userId)> winners)
    {
        return GameDB.GetCharsWithLevel(_level,1, out winners) == GameDBResult.SUCCESS;
    }
}