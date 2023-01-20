namespace GCDService.Managers.Request
{
    public abstract class BaseRequest
    {
        public bool IsAuthRequest { get; set; } = false;
    }

    public abstract class AuthRequest : BaseRequest
    {
        public string SessionID { get; set; } = string.Empty;
        protected AuthRequest() { IsAuthRequest = true; }
    }
}
