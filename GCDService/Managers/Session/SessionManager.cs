using GCDService.Managers.Permission;

namespace GCDService.Managers.Session
{
    public static class SessionManager
    {
        private static Random _r = new Random();
        private static Dictionary<long, UserSession> _activeSessions = new();

        public static void Update()
        {
            foreach (var session in _activeSessions.Values.ToList())
            {
                if (session.EndTime < DateTime.UtcNow) continue;

                RemoveUserSession(session);
            }
        }
        public static UserSession CreateUserSession(int accountID)
        {
            var sessionID = CreateSessionKey();
            var session = new UserSession(sessionID, accountID);
            _activeSessions.Add(sessionID, session);
            return session;
        }
        public static bool TryGetSession(long sessionKey,out UserSession? session)
        {
            return _activeSessions.TryGetValue(sessionKey, out session);
        }
        public static bool RemoveUserSession(long sessionKey)
        {
            if (!_activeSessions.ContainsKey(sessionKey)) return true;

            var session = _activeSessions[sessionKey];
            RemoveUserSession(session);
            return true;

        }
        private static void RemoveUserSession(UserSession session)
        {
            session.OnExpire();
            _activeSessions.Remove(session.SessionID);
        }
        private static long CreateSessionKey()
        {
            byte[] bytes = new byte[8];
            _r.NextBytes(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}