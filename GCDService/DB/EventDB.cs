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
using GCDService.Managers.Events;
using GCDService.Managers.Events.EventTypes.Abstract;
using static GCDService.DB.WebsiteDBResult;
using GCDService.Managers.Events.Models;
using Microsoft.Extensions.Logging;

namespace GCDService.DB
{
    public static class EventDB
    {
        private static string? _connectionString;

        public static void Initialize()
        {
            _connectionString = DBHelper.GetConnectionString(DBType.EVENT);
        }

        public static void Log(GameEvent gameEvent,string message,bool isError = false)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand( isError ? "S_InsertEventErrorLog" : "S_InsertEventLog", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@eventId", SqlDbType.Int).Value = gameEvent.BaseInfo.EventID;
            cmd.Parameters.Add("@eventName", SqlDbType.VarChar).Value = gameEvent.BaseInfo.EventName;
            cmd.Parameters.Add("@message", SqlDbType.VarChar).Value = message;

            con.Open();

            cmd.ExecuteNonQuery();
        }
        public static List<BaseEventInfo> GetCurrentEvents()
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_GetRunningEvents", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();

            var reader = cmd.ExecuteReader();

            var result = new List<BaseEventInfo>();
            while (reader.Read())
            {
                var baseInfo = new BaseEventInfo()
                {
                    EventID = reader.GetInt32(0),
                    EventType = (EventType) reader.GetInt32(1),
                    EventName = reader.GetString(2),
                    EventDescription = reader.GetString(3),
                    EventBeginTime = reader.GetDateTime(4),
                    EventEndTime = reader.GetDateTime(5),
                    EventParams = new []
                    {
                        reader[6].ToString(),
                        reader[7].ToString(),
                        reader[8].ToString(),
                        reader[9].ToString(),
                        reader[10].ToString(),
                        reader[11].ToString(),
                        reader[12].ToString(),
                        reader[13].ToString(),
                        reader[14].ToString(),
                        reader[15].ToString()
                    },
                    RewardInfo = reader.IsDBNull(16) ? null:
                        new GameEventRewardInfo()
                        {
                            RewardType = (EventRewardType)reader.GetInt32(16),
                            RewardParams = new[]
                            {
                                reader[17].ToString(),
                                reader[18].ToString(),
                                reader[19].ToString(),
                                reader[20].ToString(),
                                reader[21].ToString(),
                                reader[22].ToString(),
                                reader[23].ToString(),
                                reader[24].ToString(),
                                reader[25].ToString(),
                                reader[26].ToString()
                            }
                        }
                    
                };
                result.Add(baseInfo);
            }

            return result;
        }

        public static void TerminateEvent(int eventId)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_TerminateEvent", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@eventId",SqlDbType.Int).Value = eventId;

            con.Open();

            cmd.ExecuteNonQuery();
        }

        public static void MissionClearEvent_SetDate(int eventId, DateTime startTime)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_MissionClearEventSetDate", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@eventId", SqlDbType.Int).Value = eventId;
            cmd.Parameters.Add("@time",SqlDbType.DateTime).Value = startTime;

            con.Open();

            cmd.ExecuteNonQuery();
        }

        public static bool MissionClearEven_GetLastCheckedDate(int eventId, out DateTime? checkDate)
        {
            checkDate = null;
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("S_MissionClearEventGetCheckDate", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@eventId", SqlDbType.Int).Value = eventId;

            con.Open();

            var reader = cmd.ExecuteReader();

            if (!reader.HasRows) return false;

            while (reader.Read())
            {
                checkDate = reader.GetDateTime(0);
            }
            return true;
        }
    }
}
