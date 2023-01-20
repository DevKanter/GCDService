using GCDService.Controllers.Account;
using GCDService.Managers.Permission;
using GCDService.Managers.Request;
using GCDService.Managers.Session;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Asn1.X509;
using System.Data;
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
                return REGISTER_ACCOUNT_ERROR_INSERT_ACCOUNT_TABLE;
            }
            while (reader.Read())
            {
                accountID = reader.GetInt32(0);
            }

            var createUserResult = GameDB.CreateUser(request.Username!, request.Password!, out var userID);

            if (createUserResult != GameDBResult.SUCCESS) return REGISTER_ACCOUNT_ERROR_CREATING_GAME_ACCOUNT;

            SetSunUserAccount(accountID, userID);
            AddAccountPermission(accountID, Permissions.CAN_LOGIN);
            AddAccountPermission(accountID, Permissions.CAN_SEE_ACCOUNT_INFO);
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
        public static Permission[] GetAllPermissions()
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetAllPermissions", con);

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
        
        public static Permissions[] GetAccountPermissions(int accountID)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetAccountPermissions", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = accountID;

            var result = new List<Permissions>();
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add((Permissions)reader.GetInt32(0));
            }
            return result.ToArray();
        }
        public static Permissions[] GetRequestPermissions(AuthRequestType requestID)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetRequestPermissions", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@requestID", SqlDbType.Int).Value = (int)requestID;

            var result = new List<Permissions>();
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add((Permissions)reader.GetInt32(0));
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

                if(CheckAccountPermission(accountID,Permissions.CAN_LOGIN)!= SUCCESS)
                {
                    Console.WriteLine($"Account[{accountID}] does not have the permission to login!");
                    return FAIL;
                }

                session = SessionManager.CreateUserSession(accountID);
                return SUCCESS;
            }
            
            return FAIL;
        }
        
        
    }

    public enum WebsiteDBResult
    {
        FAIL = -1,
        SUCCESS,

        REGISTER_ACCOUNT_ID_ALREADY_IN_USER, //FIX used in S_RegisterAccount
        REGISTER_ACCOUNT_ERROR_INSERT_ACCOUNT_TABLE, //FIX used in S_RegisterAccount

        REGISTER_ACCOUNT_ERROR_CREATING_GAME_ACCOUNT, 
        LOGIN_ACCOUNT_ID_NOT_FOUND,

        ADD_PERMISSION_ALREADY_EXISTS,
    }
}
