namespace WeenyMapper.QueryParsing
{
    public class QueryExpressionMetaData
    {
        private int? _orderIndex;

        public QueryExpressionMetaData()
        {
            CombinationOperation = QueryCombinationOperation.And;
        }

        public int OrderIndex
        {
            get { return _orderIndex ?? 0; }
            set { _orderIndex = value; }
        }

        public QueryCombinationOperation CombinationOperation { get; set; }

        public bool HasOrderIndex
        {
            get { return _orderIndex.HasValue; }
        }
    }
}