using GCDService.Managers.Request;

namespace GCDService.Controllers.Account
{
    public class CryptRequest
    {
        public string? Data { get; set; }
    }
    public class UserLoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class UserLoginResponse
    {
        public int ResponseCode { get; set; }
        public string? SessionID { get; set; } = string.Empty;
    }
    public class UserRegisterRequest : BaseRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
    }
    public class UserRegisterResponse
    {
        public bool Success { get; set; }
    }

    public class GetAccountInfoRequest : AuthRequest
    {
    }

    public class GetAccountInfoResponse
    {
        public int AccountType { get; set; }
        public string Nickname { get; set; } = string.Empty;
    }

    public class UserLogoutRequest: AuthRequest
    {

    }
    public class UserLogoutResponse
    {
        public bool Success { get; set; }
    }

    public enum AccountType
    {
        NONE,
        SUPER_ADMIN,
        WEBSITE_ADMIN,
        WEBSITE_MOD,
        MEMBER
    }
}
