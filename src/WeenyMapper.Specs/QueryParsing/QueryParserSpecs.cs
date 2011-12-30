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
            var constraintProperties = _parser.GetConstraintProperties("WhereUsername", "Where");

            Assert.AreEqual(1, constraintProperties.Count);
            Assert.AreEqual("Username", constraintProperties[0]);
        }

        [Test]
        public void Select_query_can_specify_multiple_constraint_properties_on_the_format_Constraint1AndConstraint2()
        {
            var constraintProperties = _parser.GetConstraintProperties("WhereUsernameAndPassword", "Where");

            Assert.AreEqual(2, constraintProperties.Count);
            Assert.AreEqual("Username", constraintProperties[0]);
            Assert.AreEqual("Password", constraintProperties[1]);
        }
    }
}