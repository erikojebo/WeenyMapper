using System.Data;
using System.Data.Common;

namespace WeenyMapper.Specs.Sql
{
    public class TestDbCommand : DbCommand
    {
        private readonly DbParameterCollection _parameters = new TestDbParameterCollection();

        public override void Prepare()
        {
        }

        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbTransaction DbTransaction { get; set; }
        public override bool DesignTimeVisible { get; set; }

        public int StubNonQueryResult { get; set; }
        public int StubScalarResult { get; set; }
        public TestDbDataReader StubDataReader { get; set; }
        public bool HasExecutedNonQuery { get; set; }
        public bool HasExecutedScalar { get; set; }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _parameters; }
        }

        public override void Cancel()
        {
        }

        protected override DbParameter CreateDbParameter()
        {
            return new TestDbParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return StubDataReader ?? new TestDbDataReader();
        }

        public override int ExecuteNonQuery()
        {
            HasExecutedNonQuery = true;
            return StubNonQueryResult;
        }

        public override object ExecuteScalar()
        {
            HasExecutedScalar = true;
            return StubScalarResult;
        }
    }
}