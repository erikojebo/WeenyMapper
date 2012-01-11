using System.Collections.Generic;
using System.Data.Common;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public interface ISqlGenerator
    {
        DbCommand GenerateSelectQuery(string tableName, IEnumerable<string> columnNamesToSelect, IDictionary<string, object> constraints);
        DbCommand GenerateSelectQuery(string tableName, IEnumerable<string> columnsToSelect, QueryExpression queryExpression);
        DbCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues);
        DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, IDictionary<string, object> propertyValues);
        DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, IDictionary<string, object> columnConstraints, IDictionary<string, object> columnSetters);
        DbCommand CreateDeleteCommand(string tableName, IDictionary<string, object> constraints);
        DbCommand CreateCountCommand(string tableName, IDictionary<string, object> columnConstraints);
    }
}