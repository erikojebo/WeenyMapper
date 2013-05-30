using System.Data;
using NSubstitute;
using NUnit.Framework;
using WeenyMapper.Logging;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
{
    [TestFixture]
    public class DbCommandExecutorSpecs
    {
        private IDbCommandFactory _commandFactory;
        private ISqlCommandLogger _logger;
        private DbCommandExecutor _executor;
        private TestDbCommand _command;
        private TestDbConnection _returnThis;

        [SetUp]
        public void SetUp()
        {
            _commandFactory = Substitute.For<IDbCommandFactory>();
            _logger = Substitute.For<ISqlCommandLogger>();
            _executor = new DbCommandExecutor(_logger, _commandFactory);

            _command = new TestDbCommand();
            _returnThis = new TestDbConnection();

            _commandFactory.CreateConnection("connection string").Returns(_returnThis);
        }

        [Test]
        public void A_new_connection_is_created_and_closed_if_no_connection_to_supplied_when_executing_a_non_query()
        {
            _executor.ExecuteNonQuery(_command, "connection string");

            Assert.AreEqual(ConnectionState.Closed, _returnThis.State);
            Assert.IsTrue(_returnThis.IsDisposed);
        }
        
        [Test]
        public void A_new_connection_is_created_and_closed_if_no_connection_to_supplied_when_executing_a_scalar_command()
        {
            _command.StubScalarResult = 1;

            _executor.ExecuteScalar<int>(_command, "connection string");

            Assert.AreEqual(ConnectionState.Closed, _returnThis.State);
            Assert.IsTrue(_returnThis.IsDisposed);
        }

        [Test]
        public void A_new_connection_is_created_and_closed_if_no_connection_to_supplied_when_executing_a_query()
        {
            _executor.ExecuteQuery(_command, "connection string");

            Assert.AreEqual(ConnectionState.Closed, _returnThis.State);
            Assert.IsTrue(_returnThis.IsDisposed);
        }
    }
}