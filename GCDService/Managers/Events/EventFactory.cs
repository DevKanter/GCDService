using GCDService.Managers.Events.EventTypes;
using GCDService.Managers.Events.EventTypes.Abstract;
using GCDService.Managers.Events.Models;

namespace GCDService.Managers.Events
{
    public static class EventFactory
    {
        public static GameEvent CreateEvent(BaseEventInfo info)
        {
            switch (info.EventType)
            {
                case EventType.LEVEL_RACE_EVENT:
                    return new LevelRaceEvent(info);
                case EventType.ONGOING_MISSION_CLEAR:
                    return new MissionClearEvent(info);
                case EventType.INVALID:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
