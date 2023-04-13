using GCDService.Controllers.Account;
using GCDService.Controllers.Post;
using GCDService.Controllers.Product;
using GCDService.Managers.Permission;
using GCDService.Managers.Session;

namespace GCDService.Managers.Request
{
    public static class RequestManager
    {
        private static Dictionary<Type, AuthRequestType> _requestRef = new()
        {
            {typeof(UserLoginRequest),AuthRequestType.LOGIN},
            {typeof(GetAccountInfoRequest),AuthRequestType.ACCOUNT_INFO_GET},
            {typeof(CreatePostRequest),AuthRequestType.CREATE_POST},
            {typeof(DeletePostRequest),AuthRequestType.DELETE_POST},
            {typeof(EditPostRequest),AuthRequestType.EDIT_POST},
            {typeof(GetCashAmountRequest),AuthRequestType.ACCOUNT_CASH_GET},
            {typeof(CharacterListRequest),AuthRequestType.ACCOUNT_CHARS_GET}
        };
        private static AuthRequestType GetAuthRequestType(Type requestType)
        {
            if (!_requestRef.TryGetValue(requestType, out AuthRequestType authType))
                return AuthRequestType.NONE;
            return authType;
        }

        public static T ParseAndAuthenticate<T>(CryptRequest request, out UserSession session)
        {
            session = null;
            var decryptRequest = CryptManager.DecryptRequest<T>(request);
            if (!(decryptRequest is AuthRequest authRequest)) return decryptRequest;
            if (!SessionManager.TryGetSession(long.Parse(authRequest.SessionID), out session))
            {
                Console.WriteLine($"Request[{typeof(T)}] was requested without active session!");
                throw new Exception("Not Authorized!");
            }

            if(session!.State == SessionState.EXPIRED)
            {
                Console.WriteLine($"An expired session was still present in active sessions!!!!");
                throw new Exception("Not Authorized!");
            }

            var requestType = GetAuthRequestType(typeof(T));

            if (!PermissionManager.IsPermitted(session.AccountID, requestType))
            {
                Console.WriteLine($"Request[{typeof(T)}] is not permitted for Account[{session.AccountID}]!");
                throw new Exception("Not Authorized!");
            }
            if (decryptRequest == null) throw new Exception("Request cannot be null!");
            return decryptRequest;
        }
        public static bool Authenticate<T>(AuthRequest request, out UserSession session)
        {
            if (!SessionManager.TryGetSession(long.Parse(request.SessionID), out session))
            {
                Console.WriteLine($"Request[{typeof(T)}] was requested without active session!");
                return false;
            }

            if (session!.State == SessionState.EXPIRED)
            {
                Console.WriteLine($"An expired session was still present in active sessions!!!!");
                return false;
            }

            var requestType = GetAuthRequestType(typeof(T));

            if (!PermissionManager.IsPermitted(session.AccountID, requestType))
            {
                Console.WriteLine($"Request[{typeof(T)}] is not permitted for Account[{session.AccountID}]!");
                return false;
            }
            return true;
        }
    }
}
