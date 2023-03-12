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
            Console.WriteLine($"RegisteredUser[{userRegisterRequest.Username}] Result[{result}]");

            
            return new UserRegisterResponse()
            {
                Success = result == WebsiteDBResult.SUCCESS,
                Error = WebsiteDB.GetErrorMessage(result)
            };
        }

        [HttpPost]
        public UserLoginResponse Login([FromBody] CryptRequest crypt)
        {
            var userLoginRequest = CryptManager.DecryptRequest<UserLoginRequest>(crypt);

            var result = WebsiteDB.LoginAccount(userLoginRequest, out var session);

            Console.WriteLine($"LoggedIn User[{userLoginRequest.Username}] Result[{result}]");


            if (result == WebsiteDBResult.SUCCESS)
                session!.OnLogin();

            return new UserLoginResponse()
            {
                Success = result == WebsiteDBResult.SUCCESS,
                SessionID = session?.SessionID.ToString(),
                Error = WebsiteDB.GetErrorMessage(result)
            };
        }

        [HttpPost]
        public UserLogoutResponse Logout([FromBody] CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<UserLogoutRequest>(crypt, out var session);


            return new UserLogoutResponse()
            {
                Success = SessionManager.RemoveUserSession(long.Parse(request!.SessionID)),
                Error = "THIS SHOULD NEVER HAPPEN LOL!"
            };
        } 
        
        [HttpPost]
        public GetAccountInfoResponse GetInfo([FromBody] CryptRequest crypt)
        {
            RequestManager.ParseAndAuthenticate<GetAccountInfoRequest>(crypt, out var session);

            var result = WebsiteDB.GetAccountInfo(session!.AccountID, out var response);
            if (result != WebsiteDBResult.SUCCESS)
                return new GetAccountInfoResponse()
                {
                    Error = WebsiteDB.GetErrorMessage(result),
                    Success = false
                };
            return response!;
        }

        [HttpPost]
        public CharacterListResponse GetCharacterList([FromBody] CryptRequest crypt)
        {
            RequestManager.ParseAndAuthenticate<GetAccountInfoRequest>(crypt, out var session);

            var result = WebsiteDB.GetCharacterList(session.AccountID, out var charList);
            return new CharacterListResponse()
            {
                Success = result == WebsiteDBResult.SUCCESS,
                Error = WebsiteDB.GetErrorMessage(result),
                CharacterList = charList
            };
        }

    }


}