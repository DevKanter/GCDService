using Microsoft.Data.SqlClient;
using System.Data;

namespace GCDService.DB
{
    public static class GameDB
    {
        private static string? _connectionString;
        public static void Initialize()
        {
            var sb = new SqlConnectionStringBuilder();
            sb.UserID = "sa";
            sb.Password = "1yQA2xWS3cED4vRF";
            //sb.DataSource = "ALEXDEVLAPTOP";
            sb.DataSource = "VPSNZITZ1671265\\SQLEXPRESS";
            sb.InitialCatalog = "SUNOnline_CH_1204";
            sb.TrustServerCertificate = true;
            _connectionString = sb.ToString();
        }

        public static GameDBResult CreateUser(string username,string password, out int userID)
        {
            userID = 0;
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_UserCreate", con);
            cmd.CommandType = CommandType.StoredProcedure;
            var returnParameter = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add("@UserID", SqlDbType.VarChar).Value = username;
            cmd.Parameters.Add("@PassWD", SqlDbType.VarChar).Value = password;
            cmd.Parameters.Add("@UserSts", SqlDbType.TinyInt).Value = 1;


            con.Open();
            var reader = cmd.ExecuteReader();

            if (!reader.HasRows) return GameDBResult.FAIL;

            while (reader.Read())
            {
                userID = reader.GetInt32(0);
            }
            return GameDBResult.SUCCESS;
        }
    }

    public enum GameDBResult
    {
        FAIL = -1,
        SUCCESS = 0,

        CREATE_USER_USERNAME_ALREADY_IN_USE
    }
}
