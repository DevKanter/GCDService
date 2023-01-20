namespace GCDService.Managers.Permission
{
    public class Permission
    {
        public int PermissionID { get; init; }
        public string Name { get; init; } = "INVALID";
    }
    public enum Permissions
    {
        NONE,
        CAN_LOGIN,
        CAN_SEE_ACCOUNT_INFO,
    }
}
