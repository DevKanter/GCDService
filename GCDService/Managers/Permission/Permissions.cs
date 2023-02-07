namespace GCDService.Managers.Permission
{
    public class Permission
    {
        public int PermissionID { get; init; }
        public string Name { get; init; } = "INVALID";
       
        public static bool operator == (Permission left, Permission right)
        {
            return left.PermissionID == right.PermissionID;
        }
        public static bool operator != (Permission left, Permission right)
        {
            return left.PermissionID != right.PermissionID;
        }
    }
    public enum Permissions
    {
        NONE,
        CAN_LOGIN,
        CAN_SEE_ACCOUNT_INFO,
    }
}
