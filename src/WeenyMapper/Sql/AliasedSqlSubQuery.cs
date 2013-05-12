using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;

namespace WeenyMapper.Sql
{
    public class AliasedSqlSubQuery
    {
        public AliasedSqlSubQuery()
        {
            ExplicitlySpecifiedColumnsToSelect = new List<string>();
        }

        public string TableName { get; set; }
        public IList<string> AllSelectableColumnNames { get; set; }
        public IList<string> ExplicitlySpecifiedColumnsToSelect { get; set; }
        public string PrimaryKeyColumnName { get; set; }
        public string Alias { get; set; }

        public bool HasCustomAlias
        {
            get { return !string.IsNullOrWhiteSpace(Alias); }
        }

        public string TableIdentifier
        {
            get { return Alias ?? TableName; }
        }

        public bool HasExplicitlySpecifiedColumnsToSelect
        {
            get { return ExplicitlySpecifiedColumnsToSelect.Any(); }
        }

        public static AliasedSqlSubQuery CreateFor<T>()
        {
            return new AliasedSqlSubQuery()
                {
                    TableName = typeof(T).Name,
                };
        }


        public IList<string> GetColumnNamesToSelect()
        {
            if (ExplicitlySpecifiedColumnsToSelect.Any())
            {
                return ExplicitlySpecifiedColumnsToSelect;
            }

            return AllSelectableColumnNames;
        }

        public static AliasedSqlSubQuery Create<T>(string alias, IConventionReader conventionReader)
        {
            return Create(alias, typeof(T), conventionReader);
        }

        public static AliasedSqlSubQuery Create(string alias, Type type, IConventionReader conventionReader)
        {
            var subQuery = new AliasedSqlSubQuery()
            {
                Alias = alias,
                TableName = conventionReader.GetTableName(type),
                AllSelectableColumnNames = conventionReader.GetSelectableColumNames(type).ToList(),
                PrimaryKeyColumnName = conventionReader.TryGetPrimaryKeyColumnName(type)
            };

            return subQuery;
        }

    }
}