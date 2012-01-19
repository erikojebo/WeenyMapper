using System.Collections.Generic;
using System.Data.Common;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public interface ISqlGenerator
    {
        DbCommand GenerateSelectQuery(SqlQuery query);
        DbCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues);
        DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, IDictionary<string, object> propertyValues);
        DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, IDictionary<string, object> columnConstraints, IDictionary<string, object> columnSetters);
        DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, QueryExpression constraintExpression, IDictionary<string, object> columnSetters);
        DbCommand CreateCountCommand(string tableName, IDictionary<string, object> columnConstraints);
        DbCommand CreateDeleteCommand(string tableName, IDictionary<string, object> constraints);
        DbCommand CreateDeleteCommand(string tableName, QueryExpression queryExpression);
    }
}