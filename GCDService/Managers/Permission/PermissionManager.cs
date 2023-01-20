using GCDService.DB;
using GCDService.Managers.Request;

namespace GCDService.Managers.Permission
{
    public static class PermissionManager
    {
        private static Dictionary<int, Permissions[]> _accountPermissions = new();
        private static Dictionary<AuthRequestType, Permissions[]> _authRequestPermissions = new();

        private static Permission[] _allPermissions { get; set; }
        public static void Initialize()
        {
            _allPermissions = WebsiteDB.GetAllPermissions();
            foreach (AuthRequestType requestType in (AuthRequestType[])Enum.GetValues(typeof(AuthRequestType)))
            {
                var permissions = WebsiteDB.GetRequestPermissions(requestType);
                _authRequestPermissions.Add(requestType, permissions);
            }
        }
        public static void LoadPermission(int accountID)
        {
            var permissions = WebsiteDB.GetAccountPermissions(accountID);
            _accountPermissions.TryAdd(accountID, permissions);
        }
        public static void UnloadPermission(int accountID)
        {
            if (!_accountPermissions.ContainsKey(accountID)) return;
            _accountPermissions.Remove(accountID);
        }

        public static bool IsPermitted(int accountID,AuthRequestType authRequestType)
        {
            if (authRequestType == AuthRequestType.NONE) return true;
            var requiredPermissions = _authRequestPermissions[authRequestType];
            var permissions = _accountPermissions[accountID];

            foreach(var requiredPermission in requiredPermissions)
            {
                if(!permissions.Contains(requiredPermission)) return false;
            }
            return true;
        }
        public static Permissions[] GetDefaultPermissions()
        {
            return new Permissions[]
            {
                Permissions.CAN_LOGIN,
                Permissions.CAN_SEE_ACCOUNT_INFO
            };
        }
    }
}
