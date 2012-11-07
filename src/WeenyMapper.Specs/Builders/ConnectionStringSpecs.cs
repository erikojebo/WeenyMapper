using NUnit.Framework;
using WeenyMapper.Builders;

namespace WeenyMapper.Specs.Builders
{
    [TestFixture]
    public class ConnectionStringSpecs
    {
        [Test]
        public void Connection_string_with_trusted_connection_includes_server()
        {
            var connectionString = ConnectionString.CreateWithTrustedConnection("SomeServer", "SomeDatabase");

            Assert.That(connectionString, Is.StringContaining("Server=SomeServer;"));
        }

        [Test]
        public void Connection_string_with_trusted_connection_includes_database()
        {
            var connectionString = ConnectionString.CreateWithTrustedConnection("SomeServer", "SomeDatabase");

            Assert.That(connectionString, Is.StringContaining("Database=SomeDatabase;"));
        }
        
        [Test]
        public void Connection_string_with_trusted_connection_includes_Trusted_Connection()
        {
            var connectionString = ConnectionString.CreateWithTrustedConnection("SomeServer", "SomeDatabase");

            Assert.That(connectionString, Is.StringContaining("Trusted_Connection=True;"));
        }
        
        [Test]
        public void Connection_string_with_user_id_and_password_includes_server()
        {
            var connectionString = ConnectionString.CreateWithUserIdAndPassword("SomeServer", "SomeDatabase", "SomeUsername", "SomePassword");

            Assert.That(connectionString, Is.StringContaining("Server=SomeServer;"));
        }

        [Test]
        public void Connection_string_with_user_id_and_password_includes_database()
        {
            var connectionString = ConnectionString.CreateWithUserIdAndPassword("SomeServer", "SomeDatabase", "SomeUsername", "SomePassword");

            Assert.That(connectionString, Is.StringContaining("Database=SomeDatabase;"));
        }
        
        [Test]
        public void Connection_string_with_user_id_and_password_includes_user_id_and_password()
        {
            var connectionString = ConnectionString.CreateWithUserIdAndPassword("SomeServer", "SomeDatabase", "SomeUsername", "SomePassword");

            Assert.That(connectionString, Is.StringContaining("User Id=SomeUsername;Password=SomePassword;"));
        }

    }
}