using System.Collections.Generic;
using System.Data.Common;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public interface ISqlGenerator
    {
        DbCommand GenerateSelectQuery(SqlQuerySpecification querySpecification);
        DbCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues);
        DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, QueryExpression constraintExpression, IDictionary<string, object> columnSetters);
        DbCommand CreateCountCommand(string tableName, QueryExpression queryExpression);
        DbCommand CreateDeleteCommand(string tableName, QueryExpression queryExpression);
    }
}