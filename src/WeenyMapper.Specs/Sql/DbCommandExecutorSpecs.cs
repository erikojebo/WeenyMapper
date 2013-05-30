using System.Data;
using NSubstitute;
using NUnit.Framework;
using WeenyMapper.Logging;
using WeenyMapper.Sql;
using WeenyMapper.Extensions;

namespace WeenyMapper.Specs.Sql
{
    [TestFixture]
    public class DbCommandExecutorSpecs
    {
        private IDbCommandFactory _commandFactory;
        private ISqlCommandLogger _logger;
        private DbCommandExecutor _executor;
        private TestDbCommand _command;
        private TestDbConnection _connection;

        [SetUp]
        public void SetUp()
        {
            _commandFactory = Substitute.For<IDbCommandFactory>();
            _logger = Substitute.For<ISqlCommandLogger>();
            _executor = new DbCommandExecutor(_logger, _commandFactory);

            _command = new TestDbCommand();
            _connection = new TestDbConnection();

            _command.StubScalarResult = 1;

            _commandFactory.CreateConnection("connection string").Returns(_connection);
        }

        [Test]
        public void A_new_connection_is_created_and_closed_if_no_connection_to_supplied_when_executing_a_non_query()
        {
            _executor.ExecuteNonQuery(_command, "connection string");

            AssertConnectionIsDisposed();
        }

        [Test]
        public void A_new_connection_is_created_and_closed_if_no_connection_to_supplied_when_executing_a_scalar_command()
        {
            _executor.ExecuteScalar<int>(_command, "connection string");

            AssertConnectionIsDisposed();
        }

        [Test]
        public void A_new_connection_is_created_and_closed_if_no_connection_to_supplied_when_executing_a_query()
        {
            _executor.ExecuteQuery(_command, "connection string");

            AssertConnectionIsDisposed();
        }

        [Test]
        public void A_new_connection_is_created_and_closed_if_no_connection_to_supplied_when_executing_many_non_queries()
        {
            _executor.ExecuteNonQuery(_command.AsList(), "connection string");

            AssertConnectionIsDisposed();
        }

        [Test]
        public void A_new_connection_is_created_and_closed_if_no_connection_to_supplied_when_executing_scalar_list_query()
        {
            _executor.ExecuteScalarList<int>(_command, "connection string");

            AssertConnectionIsDisposed();
        }

        [Test]
        public void A_new_connection_is_created_and_closed_if_no_connection_to_supplied_when_executing_scalar_list_query_for_many_commands()
        {
            _executor.ExecuteScalarList<int>(_command.AsList(), "connection string");

            AssertConnectionIsDisposed();
        }
        
        [Test]
        public void A_new_connection_is_created_and_closed_if_no_connection_to_supplied_when_executing_scalar_list_query_for_many_scalar_commands()
        {
            _executor.ExecuteScalarList<int>(new [] { new ScalarCommand { ResultCommand = _command } }, "connection string");

            AssertConnectionIsDisposed();
        }

        [Test]
        public void A_new_connection_is_created_and_closed_if_no_connection_to_supplied_when_executing_a_typed_query()
        {
            _executor.ExecuteQuery<object>(_command, values => null, "connection string");

            AssertConnectionIsDisposed();
        }

        [Test]
        public void Already_open_connection_is_not_closed_when_executing_a_non_query()
        {
            _connection.Open();

            _executor.ExecuteNonQuery(_command, "connection string");

            AssertConnectionIsStillOpen();
        }

        [Test]
        public void Already_open_connection_is_not_closed_when_executing_a_non_query_a_scalar_command()
        {
            _connection.Open();

            _executor.ExecuteScalar<int>(_command, "connection string");

            AssertConnectionIsStillOpen();
        }

        [Test]
        public void Already_open_connection_is_not_closed_when_executing_a_non_query_a_query()
        {
            _connection.Open();

            _executor.ExecuteQuery(_command, "connection string");

            AssertConnectionIsStillOpen();
        }

        [Test]
        public void Already_open_connection_is_not_closed_when_executing_a_non_query_many_non_queries()
        {
            _connection.Open();

            _executor.ExecuteNonQuery(_command.AsList(), "connection string");

            AssertConnectionIsStillOpen();
        }

        [Test]
        public void Already_open_connection_is_not_closed_when_executing_a_non_query_scalar_list_query()
        {
            _connection.Open();

            _executor.ExecuteScalarList<int>(_command, "connection string");

            AssertConnectionIsStillOpen();
        }

        [Test]
        public void Already_open_connection_is_not_closed_when_executing_a_non_query_scalar_list_query_for_many_commands()
        {
            _connection.Open();

            _executor.ExecuteScalarList<int>(_command.AsList(), "connection string");

            AssertConnectionIsStillOpen();
        }

        [Test]
        public void Already_open_connection_is_not_closed_when_executing_a_non_query_scalar_list_query_for_many_scalar_commands()
        {
            _connection.Open();

            _executor.ExecuteScalarList<int>(new[] { new ScalarCommand { ResultCommand = _command } }, "connection string");

            AssertConnectionIsStillOpen();
        }

        [Test]
        public void Already_open_connection_is_not_closed_when_executing_a_non_query_a_typed_query()
        {
            _connection.Open();

            _executor.ExecuteQuery<object>(_command, values => null, "connection string");

            AssertConnectionIsStillOpen();
        }

        private void AssertConnectionIsDisposed()
        {
            Assert.AreEqual(ConnectionState.Closed, _connection.State);
            Assert.IsTrue(_connection.IsDisposed);
        }

        private void AssertConnectionIsStillOpen()
        {
            Assert.AreEqual(ConnectionState.Open, _connection.State);
            Assert.IsFalse(_connection.IsDisposed);
        }
    }
}