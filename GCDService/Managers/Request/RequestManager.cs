using GCDService.Controllers.Account;
using GCDService.Managers.Permission;
using GCDService.Managers.Session;

namespace GCDService.Managers.Request
{
    public static class RequestManager
    {
        private static Dictionary<Type, AuthRequestType> _requestRef = new()
        {
            {typeof(UserLoginRequest),AuthRequestType.LOGIN},
            {typeof(GetAccountInfoRequest),AuthRequestType.ACCOUNT_INFO}
        };
        private static AuthRequestType GetAuthRequestType(Type requestType)
        {
            if (!_requestRef.TryGetValue(requestType, out AuthRequestType authType))
                return AuthRequestType.NONE;
            return authType;
        }

        public static T? ParseAndAuthenticate<T>(CryptRequest request)
        {
            var decryptRequest = CryptManager.DecryptRequest<T>(request);
            if (!(decryptRequest is AuthRequest authRequest)) return decryptRequest;
            if (!SessionManager.TryGetSession(long.Parse(authRequest.SessionID), out var session))
            {
                Console.WriteLine($"Request[{typeof(T)}] was requested without active session!");
                return default;
            }

            if(session!.State == SessionState.EXPIRED)
            {
                Console.WriteLine($"An expired session was still present in active sessions!!!!");
                return default;
            }

            var requestType = GetAuthRequestType(typeof(T));

            if (!PermissionManager.IsPermitted(session.AccountID, requestType))
            {
                Console.WriteLine($"Request[{typeof(T)}] is not permitted for Account[{session.AccountID}]!");
                return default;
            }

            return decryptRequest;
        }
    }
}
