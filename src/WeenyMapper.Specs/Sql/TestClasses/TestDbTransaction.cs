using System.Data;
using System.Data.Common;

namespace WeenyMapper.Specs.Sql
{
    public class TestDbTransaction : DbTransaction
    {
        private DbConnection _connection;

        public TestDbTransaction()
        {
            
        }

        public TestDbTransaction(DbConnection connection)
        {
            _connection = connection;
        }

        protected bool IsCommitted { get; set; }
        protected bool IsRolledBack { get; set; }

        public override void Commit()
        {
            IsCommitted = true;
        }

        public override void Rollback()
        {
            IsRolledBack = true;
        }

        protected override DbConnection DbConnection
        {
            get { return _connection; }
        }

        public override IsolationLevel IsolationLevel
        {
            get { return IsolationLevel.Unspecified; }
        }
    }
}