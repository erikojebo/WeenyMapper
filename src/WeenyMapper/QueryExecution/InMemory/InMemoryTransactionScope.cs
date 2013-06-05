namespace WeenyMapper.QueryExecution.InMemory
{
    public class InMemoryTransactionScope : TransactionScope
    {
        private readonly InMemoryDatabase _inMemoryDatabase;
        private bool _isCommitted;

        public InMemoryTransactionScope(InMemoryDatabase inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
        }

        public override void CommitTransaction()
        {
            _isCommitted = true;
            _inMemoryDatabase.Commit(this);
        }

        public override void Rollback()
        {
            _inMemoryDatabase.Rollback(this);
        }

        public override void Dispose()
        {
            if (!_isCommitted)
                Rollback();
        }
    }
}