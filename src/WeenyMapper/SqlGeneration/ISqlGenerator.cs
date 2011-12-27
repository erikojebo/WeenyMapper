using System.Collections.Generic;
using System.Data.Common;

namespace WeenyMapper.SqlGeneration
{
    public interface ISqlGenerator
    {
        DbCommand GenerateSelectQuery(string tableName, IEnumerable<string> columnNamesToSelect, IDictionary<string, object> constraints);
        DbCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues);
        DbCommand CreateUpdateCommand(string tableName, string primaryKeyColumn, Dictionary<string, object> propertyValues);
    }
}