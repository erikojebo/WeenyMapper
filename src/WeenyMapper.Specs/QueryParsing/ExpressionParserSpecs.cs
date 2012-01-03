using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.QueryParsing;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs.QueryParsing
{
    [TestFixture]
    public class ExpressionParserSpecs
    {
        [Test]
        public void Single_equality_comparison_is_parsed_into_property_name_and_value()
        {
            var parser = new ExpressionParser();

            var expression = parser.Parse<User>(x => x.Username == "a username");

            var expectedExpression = new AndExpression(new PropertyExpression("Username"), new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }
    }
}