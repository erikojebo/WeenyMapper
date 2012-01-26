namespace WeenyMapper.Sql
{
    public class SqlQueryJoinSpecification
    {
        public string ParentTableName { get; set; }
        public string ChildTableName { get; set; }
        public string ParentPrimaryKeyColumnName { get; set; }
        public string ChildForeignKeyColumnName { get; set; }

        public SqlQuerySpecification SqlQuerySpecification { get; set; }
    }
}