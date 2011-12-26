using System.Collections.Generic;
using System.Data.SqlClient;

namespace WeenyMapper.SqlGeneration
{
    public interface ISqlGenerator
    {
        SqlCommand GenerateSelectQuery(string tableName, IDictionary<string, object> constraints);
        SqlCommand CreateInsertCommand(string tableName, IDictionary<string, object> propertyValues);
    }
}