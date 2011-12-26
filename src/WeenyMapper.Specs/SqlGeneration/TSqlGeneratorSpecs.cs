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
    }
}