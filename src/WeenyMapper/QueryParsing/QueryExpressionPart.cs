namespace WeenyMapper.QueryParsing
{
    public class QueryExpressionPart
    {
        public readonly QueryExpression QueryExpression;
        public readonly QueryExpressionMetaData MetaData;

        public QueryExpressionPart(QueryExpression queryExpression, QueryExpressionMetaData metaData)
        {
            QueryExpression = queryExpression;
            MetaData = metaData;
        }
    }
}