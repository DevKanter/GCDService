using GCDService.DB;
using GCDService.Managers.Events.Models;

namespace GCDService.Managers.Events.EventTypes.Abstract
{
    public abstract class GameEvent
    {
        protected GameEvent(BaseEventInfo baseInfo)
        {
            BaseInfo = baseInfo;
            State = EventState.WAITING;
        }

        public static GameEvent Empty(BaseEventInfo info)
        {
            return new EmptyGameEvent(new BaseEventInfo()
            {
                EventID = info.EventID,
                EventName = info.EventName,
                EventDescription = info.EventDescription,
                EventBeginTime = info.EventBeginTime,
                EventEndTime = info.EventEndTime,
                EventParams = info.EventParams,
                EventType =info.EventType,
                RewardInfo = null
            });
        }
        public BaseEventInfo BaseInfo { get; init; }
        public EventState State { get; private set; }


        public virtual void Start()
        {
            if (State > EventState.WAITING)
                throw new("Event cant be started!");

            State = EventState.ONGOING;
        }

        public virtual void Stop(EventStopReason reason)
        {
            if (State != EventState.ONGOING)
                throw new("Non Ongoing event cant be stopped!");
            EventLogger.Log(this,$"Event[{BaseInfo.EventName}] has ended. [{reason}]");
            EventDB.TerminateEvent(BaseInfo.EventID);
        }

        public virtual void Update()
        {
            if (BaseInfo.EventEndTime < DateTime.UtcNow) return;

            Stop(EventStopReason.TIME_END);

        }



    }

    public class EmptyGameEvent : GameEvent
    {
        public EmptyGameEvent(BaseEventInfo baseInfo) : base(baseInfo)
        {
        }
    }
}
