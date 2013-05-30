using System;
using System.Data;
using System.Data.Common;

namespace WeenyMapper.Specs.Sql
{
    public class TestDbConnection : DbConnection
    {
        private ConnectionState _state;

        public bool IsDisposed { get; set; }

        protected override void Dispose(bool disposing)
        {
            Close();
            IsDisposed = true;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new TestDbTransaction();
        }

        public override void Close()
        {
            if (State == ConnectionState.Closed)
                throw new Exception("Connection is already closed");

            _state = ConnectionState.Closed;
        }

        public override void ChangeDatabase(string databaseName)
        {
        }

        public override void Open()
        {
            if (State == ConnectionState.Open)
                throw new Exception("Connection is already open");

            _state = ConnectionState.Open;
        }

        public override string ConnectionString { get; set; }

        public override string Database
        {
            get { return null; }
        }

        public override ConnectionState State
        {
            get { return _state; }
        }

        public override string DataSource
        {
            get { return null; }
        }

        public override string ServerVersion
        {
            get { return null; }
        }

        protected override DbCommand CreateDbCommand()
        {
            return new TestDbCommand();
        }
    }
}