using GCDService.DB;
using GCDService.Helpers;
using GCDService.Managers.Events.EventTypes.Abstract;
using GCDService.Managers.Events.Models;

namespace GCDService.Managers.Events.EventTypes;

public class MissionClearEvent : OngoingRewardEvent
{
    private readonly int _missionCode;
    private readonly int? _requiredPartySize;
    private readonly long? _requiredClearTime;
    public MissionClearEvent(BaseEventInfo baseInfo) : base(baseInfo)
    {
        _missionCode = Convert.ToInt32(baseInfo.EventParams[0]);
        if (_missionCode == 0)
            throw new($"MissionClearEvent[{baseInfo.EventName}] has no missionCode specified!");
        _requiredPartySize = baseInfo.EventParams[1].ToNullInt();
        _requiredClearTime = baseInfo.EventParams[2].ToNullLong();

        EventDB.MissionClearEvent_SetDate(baseInfo.EventID,baseInfo.EventBeginTime);
    }

    public override bool FindRewardable(out List<(int charId, int userId)> rewardable)
    {
        if (!EventDB.MissionClearEven_GetLastCheckedDate(BaseInfo.EventID, out var lastChecked))
            lastChecked = BaseInfo.EventBeginTime;
        var newCheckDate = DateTime.UtcNow;
        var result = GameDB.GetMissionCharsByCriteria(out rewardable, out var missionSeq, _missionCode,
            lastChecked!.Value,
            _requiredClearTime, _requiredPartySize);

        EventDB.MissionClearEvent_SetDate(BaseInfo.EventID,newCheckDate);

        if (result != GameDBResult.SUCCESS) return false;

        BaseInfo.RewardInfo!.RewardParams[3] = missionSeq.ToString();
        return true;
    }

    
}