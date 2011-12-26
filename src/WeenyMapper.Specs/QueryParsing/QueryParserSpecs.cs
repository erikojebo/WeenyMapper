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

        [Test]
        public void Select_query_can_specify_multiple_constraint_properties_on_the_format_Constraint1AndConstraint2()
        {
            var query = _parser.ParseSelectQuery("UserByUsernameAndPassword");

            Assert.AreEqual("User", query.ClassName);
            Assert.AreEqual(2, query.ConstraintProperties.Count);
            Assert.AreEqual("Username", query.ConstraintProperties[0]);
            Assert.AreEqual("Password", query.ConstraintProperties[1]);
        }
    }
}