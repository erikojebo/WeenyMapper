using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.SqlGeneration;

namespace WeenyMapper.Specs.SqlGeneration
{
    [TestFixture]
    public class TSqlGeneratorSpecs
    {
        private TSqlGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generator = new TSqlGenerator();    
        }

        [Test]
        public void Generating_select_without_constraints_generates_select_star_without_where_clause()
        {
            var constraints = new Dictionary<string, object>();
            var query = _generator.GenerateSelectQuery("TableName", constraints);

            Assert.AreEqual("select * from TableName", query.CommandText);
        }        

        [Test]
        public void Generating_select_with_single_constraints_generates_select_star_with_where_clause()
        {
            var constraints = new Dictionary<string, object>();
            constraints["ColumnName"] = "value";

            var query = _generator.GenerateSelectQuery("TableName", constraints);

            Assert.AreEqual("select * from TableName where ColumnName = 'value'", query.CommandText);
        }

        [Test]
        public void Generating_insert_statement_for_an_object_creates_insert_command_with_column_name_and_value_for_each_property()
        {
            var propertyValues = new Dictionary<string, object>();

            propertyValues["ColumnName1"] = "value 1";
            propertyValues["ColumnName2"] = "value 2";

            var sqlCommand = _generator.CreateInsertCommand("TableName", propertyValues);

            Assert.AreEqual("insert into TableName (ColumnName1, ColumnName2) values ('value 1', 'value 2')", sqlCommand.CommandText);
        }
    }
}