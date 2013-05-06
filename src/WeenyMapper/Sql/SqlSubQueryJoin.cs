namespace WeenyMapper.Sql
{
    public class SqlSubQueryJoin
    {
        public string ParentTableName { get; set; }
        public string ChildTableName { get; set; }
        public string ParentPrimaryKeyColumnName { get; set; }
        public string ChildForeignKeyColumnName { get; set; }

        public AliasedSqlSubQuery ChildSubQuery { get; set; }
        public AliasedSqlSubQuery ParentSubQuery { get; set; }
    }
}