using System;
using System.Data;
using System.Data.Common;
using NUnit.Framework;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
{
    [TestFixture]
    public class SqlServerCommandFactorySpecs
    {
        private TestDbCommandFactory _commandFactory;
        private const string ConnectionString = "server=localhost";

        [SetUp]
        public void SetUp()
        {
            _commandFactory = new TestDbCommandFactory(null);
        }

        [Test]
        public void New_connections_are_created_for_each_successive_call_for_the_same_connection_string_without_connection_scope()
        {
            var connection1 = _commandFactory.CreateConnection(ConnectionString);
            var connection2 = _commandFactory.CreateConnection(ConnectionString);

            Assert.AreNotSame(connection1, connection2);
        }

        [Test]
        public void Creating_a_connection_within_a_connection_scope_always_returns_the_same_connection_for_a_given_connection_string()
        {
            DbConnection connection1, connection2;

            using (_commandFactory.BeginConnection(ConnectionString))
            {
                connection1 = _commandFactory.CreateConnection(ConnectionString);
                connection2 = _commandFactory.CreateConnection(ConnectionString);
            }

            Assert.AreSame(connection1, connection2);
        }

        [Test]
        public void Creating_a_connection_within_a_connection_scope_returns_different_connections_for_a_different_connection_strings()
        {
            DbConnection connection1, connection2, connection3, connection4;

            using (_commandFactory.BeginConnection(ConnectionString))
            using (_commandFactory.BeginConnection("server=remote"))
            {
                connection1 = _commandFactory.CreateConnection(ConnectionString);
                connection2 = _commandFactory.CreateConnection("server=remote");
                connection3 = _commandFactory.CreateConnection(ConnectionString);
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

            using (_commandFactory.BeginConnection(ConnectionString))
            {
                connection1 = _commandFactory.CreateConnection(ConnectionString);
            }

            connection2 = _commandFactory.CreateConnection(ConnectionString);

            Assert.AreNotSame(connection1, connection2);
        }

        [Test]
        public void The_connection_is_closed_after_the_scope_has_ended()
        {
            DbConnection connection1;

            using (_commandFactory.BeginConnection(ConnectionString))
            {
                connection1 = _commandFactory.CreateConnection(ConnectionString);
            }

            Assert.AreEqual(ConnectionState.Closed, connection1.State);
        }

        [Test]
        public void Connection_from_active_scope_is_still_open_after_ending_another_scope_for_different_connection_string()
        {
            DbConnection connection1, connection2;

            using (_commandFactory.BeginConnection(ConnectionString))
            {
                connection1 = _commandFactory.CreateConnection(ConnectionString);

                using (_commandFactory.BeginConnection(ConnectionString))
                {
                    connection2 = _commandFactory.CreateConnection(ConnectionString);
                }

                Assert.AreEqual(ConnectionState.Open, connection1.State);
            }
        }

        [Test]
        public void Connection_from_active_scope_is_still_open_after_ending_nested_scope_for_same_connection_string()
        {
            DbConnection connection1, connection2;

            using (_commandFactory.BeginConnection(ConnectionString))
            {
                connection1 = _commandFactory.CreateConnection(ConnectionString);

                using (_commandFactory.BeginConnection(ConnectionString))
                {
                    connection2 = _commandFactory.CreateConnection(ConnectionString);
                }

                Assert.AreEqual(ConnectionState.Open, connection1.State);
            }
        }

        [Test]
        public void Connections_created_in_an_active_scope_are_open()
        {
            using (_commandFactory.BeginConnection(ConnectionString))
            {
                var connection = _commandFactory.CreateConnection(ConnectionString);
                Assert.AreEqual(ConnectionState.Open, connection.State);
            }
        }

        [Ignore("not implemented")]
        [Test]
        public void Commands_created_within_a_transaction_belong_to_that_transaction()
        {
            using(var transactionScope = _commandFactory.BeginTransaction(ConnectionString))
            {
                var command = _commandFactory.CreateCommand();

                Assert.AreSame(transactionScope.Transaction, command.Transaction);
            }
        }

        [Ignore("not implemented")]
        [Test]
        public void Commands_created_with_command_string_within_a_transaction_belong_to_that_transaction()
        {
            using(var transactionScope = _commandFactory.BeginTransaction(ConnectionString))
            {
                var command = _commandFactory.CreateCommand("command string");

                Assert.AreSame(transactionScope.Transaction, command.Transaction);
            }
        }

        [Ignore("not implemented")]
        [Test]
        public void A_transaction_scope_creates_an_implicit_connection_scope_if_there_isnt_already_an_existing_one()
        {
            throw new NotImplementedException();
        }

        [Ignore("not implemented")]
        [Test]
        public void An_implicitly_created_connection_scope_is_closed_after_the_transaction_scope_is_closed()
        {
            throw new NotImplementedException();
        }
    }
}