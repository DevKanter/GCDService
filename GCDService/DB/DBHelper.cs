using Microsoft.Data.SqlClient;

namespace GCDService.DB
{
    public static class DBHelper
    {


        public static string GetConnectionString(DBType type)
        {
            var sb = new SqlConnectionStringBuilder
            {
                UserID = "sa",
                Password = "1yQA2xWS3cED4vRF",
                DataSource = GetDataSource(),
                InitialCatalog = GetInitCatalogString(type),
                TrustServerCertificate = true
            };
            return sb.ToString();
        }
        private static string GetDataSource()
        {
#if DEBUG
            return "ALEXDEVLAPTOP";
#else
            return "VPSNZITZ1671265\\SQLEXPRESS";
#endif
        }

        private static string GetInitCatalogString(DBType type)
        {
            return type switch
            {
                DBType.GAME => "SUNOnline_CH_1204",
                DBType.WEBSITE => "GCD_Website",
                DBType.EVENT => "GCD_Events",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }

    public enum DBType
    {
        INVALID,
        WEBSITE,
        GAME,
        EVENT
    }
}
