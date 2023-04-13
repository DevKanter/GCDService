using GCDService.DB;
using GCDService.Managers.Events.EventTypes.Abstract;

namespace GCDService.Managers.Events
{
    public static class EventLogger
    {
        public static void Log(GameEvent gameEvent, string message)
        {
            EventDB.Log(gameEvent, message);
        }

        public static void Log(GameEvent gameEvent, Exception ex)
        {
            EventDB.Log(gameEvent,ex.Message,true);
        }
    }
}
