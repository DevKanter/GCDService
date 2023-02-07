using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json;
using GCDService.Helpers;

using GCDService.DB;
using GCDService.Managers;
using GCDService.Managers.Request;
using Microsoft.AspNetCore.WebUtilities;
using GCDService.Managers.Permission;
using GCDService.Managers.Session;

namespace GCDService.Controllers.Account
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;

        }
        [HttpPost]
        public UserRegisterResponse RegisterAccount([FromBody] CryptRequest crypt)
        {
            var userRegisterRequest = CryptManager.DecryptRequest<UserRegisterRequest>(crypt);
            
            var result = WebsiteDB.RegisterAccount(userRegisterRequest, out var accountID);
            Console.WriteLine($"RegisterdUser[{userRegisterRequest.Username}] Result[{result}]");
            return new UserRegisterResponse() { Success = true };
        }

        [HttpPost]
        public UserLoginResponse Login([FromBody] CryptRequest crypt)
        {
            var userLoginRequest = CryptManager.DecryptRequest<UserLoginRequest>(crypt);

            var result = WebsiteDB.LoginAccount(userLoginRequest, out var session);

            Console.WriteLine($"LoggedIn User[{userLoginRequest.Username}] Result[{result}]");


            if (result == WebsiteDBResult.SUCCESS)
                session.OnLogin();
            return new UserLoginResponse()
            {
                ResponseCode = (int) result,
                SessionID = session?.SessionID.ToString(),

            };
        }

        [HttpPost]
        public UserLogoutResponse Logout([FromBody] CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<UserLogoutRequest>(crypt, out var session);


            return new UserLogoutResponse()
            {
                Success = SessionManager.RemoveUserSession(long.Parse(request!.SessionID))
            };
        } 
        
        [HttpPost]
        public GetAccountInfoResponse GetInfo([FromBody] CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<GetAccountInfoRequest>(crypt, out var session);
            if(request == null) throw new ArgumentNullException(nameof(request));

            var result = WebsiteDB.GetAccountInfo(session!.AccountID, out var response);
            if (result != WebsiteDBResult.SUCCESS) throw new InvalidOperationException();
            return response!;
        }

    }


}