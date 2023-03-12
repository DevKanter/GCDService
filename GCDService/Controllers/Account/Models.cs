using System.Net.NetworkInformation;
using GCDService.Managers.Request;

namespace GCDService.Controllers.Account
{
    public class CryptRequest
    {
        public string? Data { get; set; }
    }

    public abstract class BaseResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
    public class UserLoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class UserLoginResponse : BaseResponse
    {
        public string? SessionID { get; set; } = string.Empty;
    }
    public class UserRegisterRequest : BaseRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
    }
    public class UserRegisterResponse : BaseResponse
    {
    }

    public class GetAccountInfoRequest : AuthRequest
    {
    }

    public class GetAccountInfoResponse : BaseResponse
    {
        public int? AccountType { get; set; }
        public string? Nickname { get; set; } = string.Empty;
    }

    public class UserLogoutRequest: AuthRequest
    {

    }
    public class UserLogoutResponse :BaseResponse
    {
    }

    public class CharacterListRequest : AuthRequest
    {

    }

    public class CharacterListResponse : BaseResponse
    {
        public IEnumerable<CharacterListEntry> CharacterList { get; set; } = Enumerable.Empty<CharacterListEntry>();
    }
    public class CharacterListEntry
    {
        public int ClassCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
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
