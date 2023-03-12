using Azure;
using GCDService.Controllers.Account;
using GCDService.Controllers.Post;
using GCDService.Controllers.Product;
using GCDService.Managers.Cash;
using GCDService.Managers.Permission;
using GCDService.Managers.Request;
using GCDService.Managers.Session;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using Org.BouncyCastle.Asn1.X509;
using System.Data;
using System.IO.Pipelines;
using System.Reflection.PortableExecutable;
using static GCDService.DB.WebsiteDBResult;


namespace GCDService.DB
{
    public static class WebsiteDB
    {
        private static string? _connectionString;
        public static void Initialize() {
            var sb = new SqlConnectionStringBuilder();
            sb.UserID = "sa";
            sb.Password = "1yQA2xWS3cED4vRF";
            //sb.DataSource = "ALEXDEVLAPTOP";
            sb.DataSource = "VPSNZITZ1671265\\SQLEXPRESS";
            sb.InitialCatalog = "GCD_Website";
            sb.TrustServerCertificate = true;
             _connectionString = sb.ToString();

        }

        public static WebsiteDBResult RegisterAccount(UserRegisterRequest request, out int accountID)
        {
            accountID = 0;
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_RegisterAccount", con);

            cmd.CommandType = CommandType.StoredProcedure;
            var returnParameter = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add("@username", SqlDbType.NVarChar).Value = request.Username;
            cmd.Parameters.Add("@password", SqlDbType.NVarChar).Value = request.Password;
            cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = request.Email;

            con.Open();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    accountID = reader.GetInt32(0);

                }
            }

            var result = (WebsiteDBResult)(int)returnParameter.Value;
            if (result != SUCCESS)
            {
                return result;
            }

            var createUserResult = GameDB.CreateUser(request.Username!, request.Password!, out var userID);

            if (createUserResult != GameDBResult.SUCCESS) return REGISTER_ACCOUNT_ERROR_CREATING_GAME_ACCOUNT;

            SetSunUserAccount(accountID, userID);
 
            WelcomeGift(userID);

           
            return SUCCESS;
        }
        public static WebsiteDBResult SetSunUserAccount(int accountID,int userID)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_SetSunUserID", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = accountID;
            cmd.Parameters.Add("@userID", SqlDbType.Int).Value = userID;

            con.Open();
            var reader = cmd.ExecuteNonQuery();
            return SUCCESS;
        }
        public static WebsiteDBResult WelcomeGift(int userGuid)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_WelcomeGift", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userGuid; ;

            con.Open();
            var reader = cmd.ExecuteNonQuery();
            return SUCCESS;
        }
        public static Permission[] GetAllPermissions()
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetAllPermissions", con);
            cmd.CommandType = CommandType.StoredProcedure;
            var result = new List<Permission>();

            con.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var permission = new Permission()
                {
                    PermissionID = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };
                result.Add(permission);
            }
            return result.ToArray();
        }
        public static WebsiteDBResult AddAccountPermission(int accountID,Permissions permission)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_AddAccountPermission", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = accountID;
            cmd.Parameters.Add("@permissionID", SqlDbType.Int).Value = (int) permission;
            var returnParameter = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;

            con.Open();
            cmd.ExecuteNonQuery();

            var result =(int) returnParameter.Value;

            if (result != 0) Console.WriteLine($"Permission[{permission}] was not added to Account[{accountID}] Error[{result}]");

            return (WebsiteDBResult) result;
        }
        public static WebsiteDBResult CheckAccountPermission(int accountID,Permissions permission)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_CheckAccountPermission", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = accountID;
            cmd.Parameters.Add("@permissionID", SqlDbType.Int).Value = (int)permission;
            var returnParameter = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;

            con.Open();
            cmd.ExecuteNonQuery();

            var result = (int)returnParameter.Value;

            if (result != 0) Console.WriteLine($"Account[{accountID}] tried unpermissioned action[{permission}]");

            return (WebsiteDBResult)result;
        }
        
        public static IEnumerable<Post> GetPosts(PostCategory postCategory, bool isDev=false)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetPosts", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@category", SqlDbType.Int).Value =(int) postCategory;
            cmd.Parameters.Add("@isDev",SqlDbType.Bit).Value = isDev;
            var result = new List<Post>();

            con.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var post = new Post()
                {
                    Id = reader.GetInt32(0),
                    PostCategory = (PostCategory)reader.GetInt32(1),
                    Title = reader.GetString(2),
                    Description = reader.GetString(3),
                    Content = reader.GetString(4),
                    Posted = reader.GetDateTime(5),
                    Modified = reader.GetDateTime(6),
                    PostVisibility = (PostVisibility)reader.GetInt32(7),
                    PostedBy = reader.GetString(8)
                };
                result.Add(post);
            }
            return result;
        }
        public static WebsiteDBResult AddPost(CreatePostData post, int accountID)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_AddPost", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@postCategory", SqlDbType.Int).Value = (int)post.Category;
            cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = post.Title;
            cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = post.Description;
            cmd.Parameters.Add("@content", SqlDbType.VarChar).Value = post.Content;
            cmd.Parameters.Add("@postVisibility", SqlDbType.Int).Value = (int)post.Visibility;
            cmd.Parameters.Add("@postedBy", SqlDbType.Int).Value = accountID;

            con.Open();
            var result = cmd.ExecuteNonQuery();

            return result == 1 ? SUCCESS : FAIL;
        }

        public static WebsiteDBResult DeletePost(int postID)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_DeletePost", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@postId", SqlDbType.Int).Value = postID;
            con.Open();
            var result = cmd.ExecuteNonQuery();
            return result == 1 ? SUCCESS: FAIL;
        }
        
        public static WebsiteDBResult EditPost(EditPostData post)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_EditPost", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@id", SqlDbType.VarChar).Value = post.PostId;
            cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = post.Title;
            cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = post.Description;
            cmd.Parameters.Add("@content", SqlDbType.VarChar).Value = post.Content;
            cmd.Parameters.Add("@visibility", SqlDbType.Int).Value = (int) post.Visibility;
            con.Open();
            var result = cmd.ExecuteNonQuery();

            return result == 1 ? SUCCESS : FAIL;

        }
        public static Dictionary<int, List<int>> GetAccountTypePermissions()
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetAccountTypePermissions", con);
            cmd.CommandType = CommandType.StoredProcedure;

            var result = new Dictionary<int, List<int>>();

            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var accountTypeId = reader.GetInt32(0);
                var permissionId = reader.GetInt32(1);
                if(!result.ContainsKey(accountTypeId)) result.Add(accountTypeId, new List<int>());

                result[accountTypeId].Add(permissionId);
            }

            return result;
        }
        public static int[] GetAccountPermissionIds(int accountID)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetAccountPermissions", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = accountID;

            var result = new List<int>();
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader.GetInt32(0));
            }
            return result.ToArray();
        }
        public static int[] GetRequestPermissionIds(AuthRequestType requestID)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetRequestPermissions", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@requestID", SqlDbType.Int).Value = (int)requestID;

            var result = new List<int>();
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader.GetInt32(0));
            }
            return result.ToArray();
        }
        public static WebsiteDBResult LoginAccount(UserLoginRequest request, out UserSession? session)
        {
            session = null;
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_LoginAccount", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@username", SqlDbType.NVarChar).Value = request.Username;
            cmd.Parameters.Add("@password", SqlDbType.NVarChar).Value = request.Password;

            con.Open();
            var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                return LOGIN_ACCOUNT_ID_NOT_FOUND;
            }
            while (reader.Read())
            {
                var accountID = reader.GetInt32(0);

                if(!PermissionManager.IsPermitted(accountID,AuthRequestType.LOGIN))
                {
                    Console.WriteLine($"Account[{accountID}] does not have the permission to login!");
                    return ERROR_NOT_PERMITTED;
                }

                session = SessionManager.CreateUserSession(accountID);
                CashProductManager.OnUserLogin(session);
                return SUCCESS;
            }
            
            return FAIL;
        }
        
        public static WebsiteDBResult GetAccountInfo(int accountID, out GetAccountInfoResponse? response)
        {
            response = null;
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetAccountInfo", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = accountID;

            con.Open();
            var reader = cmd.ExecuteReader();
            if (!reader.HasRows) return ERROR_RETRIEVING_ACCOUNT_INFO;
            while (reader.Read())
            {
                response = new GetAccountInfoResponse()
                {
                    AccountType = reader.GetInt32(0),
                    Nickname = reader.GetString(1),
                    Success = true
                };
            }
            return SUCCESS;
        }

        public static CashProduct[] GetCashProductList() {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_CashProduct_Select", con);
            cmd.CommandType = CommandType.StoredProcedure;

            var result = new List<CashProduct>();
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var product = new CashProduct()
                {
                    Id = reader.GetInt32(0),
                    Amount = reader.GetInt32(1),
                    Price = reader.GetInt32(2),
                    ProductName = reader.GetString(3)
                };

                result.Add(product);
            }

            return result.ToArray();
        }

        public static int GetCashAmount(int accountID)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetCashBalance", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = accountID;

            con.Open();
            var reader = cmd.ExecuteReader();
            if (!reader.HasRows) return 0;
            while (reader.Read())
            {
                var amount = reader.GetInt32(0);
                return amount;
            }

            return 0;
        }

        public static WebsiteDBResult IncreaseCashAmount(int accountID, int amount)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_IncreaseCashBalance", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = accountID;
            cmd.Parameters.Add("@incAmount", SqlDbType.Int).Value = amount;

            con.Open();

            var result = cmd.ExecuteNonQuery();

            return result == 1 ? SUCCESS : FAIL;

        }
        public static void LogCashProductCheckoutProcess(CashProductCheckoutProcess process, CheckoutProcessEndReason reason,string message = "")
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_LOG_CashProductCheckout", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@cashProductId", SqlDbType.Int).Value = process.Product.Id;
            cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = process.Session.AccountID;
            cmd.Parameters.Add("@checkoutState", SqlDbType.VarChar).Value = process.State.ToString();
            cmd.Parameters.Add("@requestTime", SqlDbType.DateTime).Value = process.ApprovedRequest?.TransactionDate;
            cmd.Parameters.Add("@transactionId", SqlDbType.VarChar).Value = process.ApprovedRequest?.TransactionId;
            cmd.Parameters.Add("@transactionStatus", SqlDbType.VarChar).Value = process.GetTransactionStatus().ToString();
            cmd.Parameters.Add("@payerId", SqlDbType.VarChar).Value = process.ApprovedRequest?.Payer?.PayerId;
            cmd.Parameters.Add("@payerEmail", SqlDbType.VarChar).Value = process.ApprovedRequest?.Payer?.Email;
            cmd.Parameters.Add("@logmessage", SqlDbType.VarChar).Value = message;

            con.Open();

            cmd.ExecuteNonQuery();
        }

        public static WebsiteDBResult GetCharacterList(int accountID, out IEnumerable<CharacterListEntry> characterList)
        {
            characterList = Enumerable.Empty<CharacterListEntry>();
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetCharacterList", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = accountID;

            con.Open();
            var reader = cmd.ExecuteReader();

            if (!reader.HasRows) return NO_CHARACTERS_FOUND_FOR_ACCOUNT;

            var result = new List<CharacterListEntry>();
            while (reader.Read())
            {
                var entry = new CharacterListEntry()
                {
                    ClassCode = reader.GetByte(0),
                    Name = reader.GetString(1),
                    Level = reader.GetInt16(2)
                };
                result.Add(entry);
            }

            characterList = result;
            return SUCCESS;
        }

        public static string? GetErrorMessage(WebsiteDBResult result)
        {
            switch (result)
            {
                case FAIL:
                    return "Failed!";
                case SUCCESS:
                    return "Success!";
                case REGISTER_ACCOUNT_ID_ALREADY_IN_USE:
                    return "Username is already taken!";
                case REGISTER_ACCOUNT_DATABASE_ERROR:
                    return "Database error!";
                case REGISTER_ACCOUNT_ERROR_CREATING_GAME_ACCOUNT:
                    return "Failed to create game account!";
                case LOGIN_ACCOUNT_ID_NOT_FOUND:
                    return "Username could not be found!";
                case ADD_PERMISSION_ALREADY_EXISTS:
                    break;
                case ERROR_RETRIEVING_ACCOUNT_INFO:
                    return "Could not retrieve account info!";
                case ERROR_NOT_PERMITTED:
                    return "Not authorized!";
                case REGISTER_ACCOUNT_EMAIL_ALREADY_IN_USE:
                    return "Email is already registered";
                case NO_CHARACTERS_FOUND_FOR_ACCOUNT:
                    return "This account has no characters yet.";
                default:
                    return null;
            }

            return null;
        }
    }

    public enum WebsiteDBResult
    {
        FAIL = -1,
        SUCCESS,

        REGISTER_ACCOUNT_ID_ALREADY_IN_USE, //FIX used in S_RegisterAccount
        REGISTER_ACCOUNT_DATABASE_ERROR, //FIX used in S_RegisterAccount

        REGISTER_ACCOUNT_ERROR_CREATING_GAME_ACCOUNT, 
        LOGIN_ACCOUNT_ID_NOT_FOUND,

        ADD_PERMISSION_ALREADY_EXISTS,
        ERROR_RETRIEVING_ACCOUNT_INFO,

        ERROR_NOT_PERMITTED,

        REGISTER_ACCOUNT_EMAIL_ALREADY_IN_USE,

        NO_CHARACTERS_FOUND_FOR_ACCOUNT
    }
}
