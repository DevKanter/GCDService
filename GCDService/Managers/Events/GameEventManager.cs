using System.Timers;
using GCDService.DB;
using GCDService.Managers.Events.EventTypes.Abstract;

namespace GCDService.Managers.Events
{
    public static class GameEventManager
    {
        private static readonly Dictionary<int, GameEvent> _activeEvents = new();
        private static System.Timers.Timer _timer;
        public static void Initialize()
        {
            _timer = new()
            {
                AutoReset = true,
                Enabled = true,
                Interval = 5000
            };
            _timer.Elapsed += delegate { Update();  };
            _timer.Start();

            var eventInfos = EventDB.GetCurrentEvents();
            foreach (var eventInfo in eventInfos) 
            {
                try
                {
                    var gameEvent = EventFactory.CreateEvent(eventInfo);
                    _activeEvents.TryAdd(gameEvent.BaseInfo.EventID, gameEvent);
                }
                catch (Exception ex)
                {
                    var gameEvent = GameEvent.Empty(eventInfo);
                    EventLogger.Log(gameEvent,ex);
                }

            }
        }

        public static void Update()
        {
            foreach (var eventNode in _activeEvents)
            {
                try
                {
                    eventNode.Value.Update();
                }
                catch (Exception ex)
                {
                    EventLogger.Log(eventNode.Value,ex);
                }

            }
        }
    }
}
