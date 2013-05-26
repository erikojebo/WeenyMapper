using System.Data;
using System.Data.Common;
using NUnit.Framework;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
{
    [TestFixture]
    public class SqlServerCommandFactorySpecs : AcceptanceSpecsBase
    {
        private SqlServerCommandFactory _commandFactory;

        protected override void PerformSetUp()
        {
            _commandFactory = new SqlServerCommandFactory();
        }

        [Test]
        public void New_connections_are_created_for_each_successive_call_for_the_same_connection_string_without_connection_scope()
        {
            var connection1 = _commandFactory.CreateConnection("server=localhost");
            var connection2 = _commandFactory.CreateConnection("server=localhost");

            Assert.AreNotSame(connection1, connection2);
        }

        [Test]
        public void Creating_a_connection_within_a_connection_scope_always_returns_the_same_connection_for_a_given_connection_string()
        {
            DbConnection connection1, connection2;

            using (_commandFactory.BeginConnection("server=localhost"))
            {
                connection1 = _commandFactory.CreateConnection("server=localhost");
                connection2 = _commandFactory.CreateConnection("server=localhost");
            }

            Assert.AreSame(connection1, connection2);
        }

        [Test]
        public void Creating_a_connection_within_a_connection_scope_returns_different_connections_for_a_different_connection_strings()
        {
            DbConnection connection1, connection2, connection3, connection4;

            using (_commandFactory.BeginConnection("server=localhost"))
            using (_commandFactory.BeginConnection("server=remote"))
            {
                connection1 = _commandFactory.CreateConnection("server=localhost");
                connection2 = _commandFactory.CreateConnection("server=remote");
                connection3 = _commandFactory.CreateConnection("server=localhost");
                connection4 = _commandFactory.CreateConnection("server=remote");
            }

            Assert.AreSame(connection1, connection3);
            Assert.AreSame(connection2, connection4);
            Assert.AreNotSame(connection1, connection2);
        }

        [Test]
        public void Creating_a_connection_after_a_scope_has_ended_returns_a_new_connection()
        {
            DbConnection connection1, connection2;

            using (_commandFactory.BeginConnection("server=localhost"))
            {
                connection1 = _commandFactory.CreateConnection("server=localhost");
            }

            connection2 = _commandFactory.CreateConnection("server=localhost");

            Assert.AreNotSame(connection1, connection2);
        }

        [Test]
        public void The_connection_is_closed_after_the_scope_has_ended()
        {
            DbConnection connection1;

            using (_commandFactory.BeginConnection(TestConnectionString))
            {
                connection1 = _commandFactory.CreateConnection(TestConnectionString);
                connection1.Open();
            }

            Assert.AreEqual(ConnectionState.Closed, connection1.State);
        }

        [Test]
        public void Connection_from_active_scope_is_still_open_after_ending_another_scope_for_different_connection_string()
        {
            DbConnection connection1, connection2;

            using (_commandFactory.BeginConnection(TestConnectionString))
            {
                connection1 = _commandFactory.CreateConnection(TestConnectionString);
                connection1.Open();

                using (_commandFactory.BeginConnection("server=localhost"))
                {
                    connection2 = _commandFactory.CreateConnection(TestConnectionString);
                }

                Assert.AreEqual(ConnectionState.Open, connection1.State);
            }
        }

        [Test]
        public void Connection_from_active_scope_is_still_open_after_ending_nested_scope_for_same_connection_string()
        {
            DbConnection connection1, connection2;

            using (_commandFactory.BeginConnection(TestConnectionString))
            {
                connection1 = _commandFactory.CreateConnection(TestConnectionString);
                connection1.Open();

                using (_commandFactory.BeginConnection(TestConnectionString))
                {
                    connection2 = _commandFactory.CreateConnection(TestConnectionString);
                }

                Assert.AreEqual(ConnectionState.Open, connection1.State);
            }
        }
    }
}