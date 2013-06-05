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
        public void New_connections_are_created_for_each_successive_call_without_connection_scope()
        {
            var connection1 = _commandFactory.CreateConnection();
            var connection2 = _commandFactory.CreateConnection();

            Assert.AreNotSame(connection1, connection2);
        }

        [Test]
        public void Creating_a_connection_within_a_connection_scope_always_returns_the_same_connection()
        {
            DbConnection connection1, connection2;

            using (_commandFactory.BeginConnection())
            {
                connection1 = _commandFactory.CreateConnection();
                connection2 = _commandFactory.CreateConnection();
            }

            Assert.AreSame(connection1, connection2);
        }

        [Test]
        public void Creating_a_connection_after_a_scope_has_ended_returns_a_new_connection()
        {
            DbConnection connection1, connection2;

            using (_commandFactory.BeginConnection())
            {
                connection1 = _commandFactory.CreateConnection();
            }

            connection2 = _commandFactory.CreateConnection();

            Assert.AreNotSame(connection1, connection2);
        }

        [Test]
        public void The_connection_is_closed_after_the_scope_has_ended()
        {
            DbConnection connection1;

            using (_commandFactory.BeginConnection())
            {
                connection1 = _commandFactory.CreateConnection();
            }

            Assert.AreEqual(ConnectionState.Closed, connection1.State);
        }

        [Test]
        public void Connection_from_active_scope_is_still_open_after_ending_nested_scope()
        {
            DbConnection connection1, connection2;

            using (_commandFactory.BeginConnection())
            {
                connection1 = _commandFactory.CreateConnection();

                using (_commandFactory.BeginConnection())
                {
                    connection2 = _commandFactory.CreateConnection();
                }

                Assert.AreEqual(ConnectionState.Open, connection1.State);
            }
        }

        [Test]
        public void Connections_created_in_an_active_scope_are_open()
        {
            using (_commandFactory.BeginConnection())
            {
                var connection = _commandFactory.CreateConnection();
                Assert.AreEqual(ConnectionState.Open, connection.State);
            }
        }

        [Ignore("not implemented")]
        [Test]
        public void Commands_created_within_a_transaction_belong_to_that_transaction()
        {
            using(var transactionScope = _commandFactory.BeginTransaction())
            {
                var command = _commandFactory.CreateCommand();

                Assert.AreSame(transactionScope.Transaction, command.Transaction);
            }
        }

        [Ignore("not implemented")]
        [Test]
        public void Commands_created_with_command_string_within_a_transaction_belong_to_that_transaction()
        {
            using(var transactionScope = _commandFactory.BeginTransaction())
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