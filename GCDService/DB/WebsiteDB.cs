using GCDService.Controllers.Account;
using GCDService.Controllers.Post;
using GCDService.Managers.Permission;
using GCDService.Managers.Request;
using GCDService.Managers.Session;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
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
            var reader = cmd.ExecuteReader();

            if(!reader.HasRows)
            {
                return REGISTER_ACCOUNT_DATABASE_ERROR;
            }
            while (reader.Read())
            {
                accountID = reader.GetInt32(0);
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
        
        public static IEnumerable<Post> GetPosts(int postCategory)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetPosts", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@category", SqlDbType.Int).Value = postCategory;
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
                    PostVisiblity = (PostVisibility)reader.GetInt32(7),
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
            cmd.Parameters.Add("@postVisiblity", SqlDbType.Int).Value = (int)post.Visibility;
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
                    Nickname = reader.GetString(1)
                };
            }
            return SUCCESS;
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
    }
}
