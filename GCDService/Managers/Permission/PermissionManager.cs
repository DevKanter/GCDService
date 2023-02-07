using GCDService.DB;
using GCDService.Managers.Request;
using System.Reflection.Metadata.Ecma335;

namespace GCDService.Managers.Permission
{
    public static class PermissionManager
    {
        private static Dictionary<int, Permission[]> _accountTypePermissions = new();
        private static Dictionary<int, Permission[]> _accountPermissions = new();
        private static Dictionary<AuthRequestType, Permission[]> _authRequestPermissions = new();

        private static Permission[] _allPermissions { get; set; } = new Permission[0];
        public static void Initialize()
        {
            _allPermissions = WebsiteDB.GetAllPermissions();
            var accountTypePermissionIDs = WebsiteDB.GetAccountTypePermissions();
           
            foreach (var node in accountTypePermissionIDs)
            {
                _accountTypePermissions.Add(node.Key, _allPermissions.Where(perm => node.Value.Contains(perm.PermissionID)).ToArray());
            }

            foreach (AuthRequestType requestType in (AuthRequestType[])Enum.GetValues(typeof(AuthRequestType)))
            {
                var permissionIds = WebsiteDB.GetRequestPermissionIds(requestType);
                var permissions = GetPermissionsByIds(permissionIds);
                _authRequestPermissions.Add(requestType, permissions);
            }
        }
        public static void LoadPermission(int accountID)
        {
            var permissionIds = WebsiteDB.GetAccountPermissionIds(accountID);
            var permissions = GetPermissionsByIds(permissionIds);
            _accountPermissions.TryAdd(accountID, permissions);
        }
        public static void UnloadPermission(int accountID)
        {
            if (!_accountTypePermissions.ContainsKey(accountID)) return;
            _accountPermissions.Remove(accountID);
        }

        public static bool IsPermitted(int accountID,AuthRequestType authRequestType)
        {
            if (authRequestType == AuthRequestType.NONE) return true;
            var requiredPermissions = _authRequestPermissions[authRequestType];

            var permissions = new List<Permission>();

            if(authRequestType != AuthRequestType.LOGIN) {
                permissions = _accountPermissions[accountID].ToList();
            }
            
            if(WebsiteDB.GetAccountInfo(accountID, out var accountInfo)==WebsiteDBResult.SUCCESS)
            {
                if (accountInfo!.AccountType == 1) return true;

                permissions.AddRange(_accountTypePermissions[accountInfo.AccountType]);
            }

            foreach (var requiredPermission in requiredPermissions)
            {
                if(!permissions.Contains(requiredPermission)) return false;
            }
            return true;
        }

        private static Permission[] GetPermissionsByIds(int[] ids)
        {
            return _allPermissions.Where(perm => ids.Contains(perm.PermissionID)).ToArray();
        }
    }
}
