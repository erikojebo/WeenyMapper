using System.Data.SqlClient;

namespace WeenyMapper.Specs
{
    public class AcceptanceSpecsBase {
        public const string TestConnectionString = @"Data source=.\SQLEXPRESS;Initial Catalog=WeenyMapper;Trusted_Connection=true";

        protected void DeleteAllExistingTestData()
        {
            using (var connection = new SqlConnection(TestConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("delete from [User]", connection))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SqlCommand("delete from [t_Books]", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}