namespace WeenyMapper.Builders
{
    public class ConnectionString
    {
        public static string CreateWithTrustedConnection(string server, string database)
        {
            return string.Format("Server={0};Database={1};Trusted_Connection=True;", server, database);
        }

        public static string CreateWithUserIdAndPassword(string server, string database, string userId, string password)
        {
            return string.Format("Server={0};Database={1};User Id={2};Password={3};", server, database, userId, password); 
        }
    }
}