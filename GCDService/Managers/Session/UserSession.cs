using GCDService.Managers.Permission;

namespace GCDService.Managers.Session
{
    public class UserSession
    {
        public SessionState State { get; private set; }
        public long SessionID { get; init; }
        public int AccountID { get; init; }
        public DateTime CreationTime { get; init; }
        public DateTime EndTime { get; private set; }

        public UserSession(long sessionID, int accountID)
        {
            SessionID = sessionID;
            AccountID = accountID;

            CreationTime = DateTime.UtcNow;
            EndTime = DateTime.UtcNow.AddMinutes(10);
            OnCreate();
        }
        public void OnCreate()
        {
            PermissionManager.LoadPermission(AccountID);
        }
        public void OnExpire()
        {
            PermissionManager.UnloadPermission(AccountID);
            State = SessionState.EXPIRED;
        }
        public void OnLogin()
        {
            State = SessionState.CONNECTED;
        }
    }
    public enum SessionState
    {
        INACTIVE,
        CONNECTED,
        EXPIRED
    }
}
