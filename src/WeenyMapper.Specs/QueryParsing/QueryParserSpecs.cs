using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Specs.QueryParsing
{
    [TestFixture]
    public class QueryParserSpecs
    {
        private QueryParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new QueryParser();            
        }

        [Test]
        public void Select_query_with_class_name_and_single_constraint_property_is_parsed_correctly()
        {
            var query = _parser.ParseSelectQuery("UserByUsername");

            Assert.AreEqual("User", query.ClassName);
            Assert.AreEqual(1, query.ConstraintProperties.Count);
            Assert.AreEqual("Username", query.ConstraintProperties[0]);
        }
    }
}