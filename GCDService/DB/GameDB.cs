using Microsoft.Data.SqlClient;
using System.Data;
using Azure.Identity;
using System;

namespace GCDService.DB
{
    public static class GameDB
    {
        private static string? _connectionString;
        public static void Initialize()
        {
            _connectionString = DBHelper.GetConnectionString(DBType.GAME);
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

        public static GameDBResult GetCharsWithLevel(int level,int count, out List<(int charId,int userID)> userCharNodes)
        {
            userCharNodes = new();

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("WS_FirstCharsWithLevel", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@level", SqlDbType.Int).Value = level;
            cmd.Parameters.Add("@count", SqlDbType.Int).Value = count;
            con.Open();
            var reader = cmd.ExecuteReader();

            if (!reader.HasRows) return GameDBResult.NOTHING_FOUND;

            while (reader.Read())
            {
                var charId = reader.GetInt32(0);
                var userId = 0;
                userCharNodes.Add((charId,userId));
            }
            return GameDBResult.SUCCESS;
        }

        public static GameDBResult GetMissionCharsByCriteria(out List<(int charId, int userID)> userCharNodes,out int missionSeq,int missionCode, DateTime from, long? clearTime,
            int? partySize)
        {
            userCharNodes = new();
            missionSeq = 0;
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("WS_FirstCharsWithLevel", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@missionCode", SqlDbType.Int).Value = missionCode;
            cmd.Parameters.Add("@missionDate", SqlDbType.DateTime).Value = from;
            if(clearTime != null)
                cmd.Parameters.Add("@clearTime",SqlDbType.BigInt).Value = clearTime;
            if(partySize !=null)
                cmd.Parameters.Add("@partySize",SqlDbType.TinyInt).Value = (byte) partySize;
            con.Open();
            var reader = cmd.ExecuteReader();

            if (!reader.HasRows) return GameDBResult.NOTHING_FOUND;

            while (reader.Read())
            {
                var userId = reader.GetInt32(0);
                var charId = reader.GetInt32(1);
                userCharNodes.Add((charId, userId));
            }
            return GameDBResult.SUCCESS;
        }

        public static GameDBResult GetUserAndCharName(int userId, int charId, out string userName,out string charName)
        {
            userName = string.Empty;
            charName = string.Empty;
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("WS_GetUserAndCharName", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@charId", SqlDbType.Int).Value = charId;    
            con.Open();
            var reader = cmd.ExecuteReader();

            if (!reader.HasRows) return GameDBResult.NOTHING_FOUND;

            while (reader.Read())
            {
                userName = reader.GetString(0);
                charName = reader.GetString(1);
            }
            return GameDBResult.SUCCESS;
        }
        public static GameDBResult SetTitle(int charID, int titleID)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("S_HonorTitles_Add", con);
            cmd.CommandType = CommandType.StoredProcedure;

            var returnParameter = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;


            cmd.Parameters.Add("@CharGuid", SqlDbType.Int).Value = charID;
            cmd.Parameters.Add("@TitleIndex",SqlDbType.Int).Value = titleID;
            con.Open();

            cmd.ExecuteNonQuery();

            return (int)returnParameter.Value == 0 ? GameDBResult.SUCCESS : GameDBResult.FAIL;
        }

        public static GameDBResult InsertEventItem(int itemCode, byte itemCount, int userId, string userName,
            string charName, int timeLimit)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("S_Event_Insert", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@c_itemCode", SqlDbType.Int).Value = itemCode;
            cmd.Parameters.Add("@ti_itemCount", SqlDbType.TinyInt).Value = itemCount;
            cmd.Parameters.Add("@i_UserGuid", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@vc_UserID", SqlDbType.VarChar).Value = userName;
            cmd.Parameters.Add("@vc_CharName",SqlDbType.VarChar).Value = charName;
            cmd.Parameters.Add("@TimeLimit", SqlDbType.Int).Value = timeLimit;
            con.Open();

            var result = cmd.ExecuteNonQuery();

            return result == 1 ? GameDBResult.SUCCESS : GameDBResult.FAIL;
        }
    }

    public enum GameDBResult
    {
        FAIL = -1,
        SUCCESS = 0,

        CREATE_USER_USERNAME_ALREADY_IN_USE,
        NOTHING_FOUND
    }
}
