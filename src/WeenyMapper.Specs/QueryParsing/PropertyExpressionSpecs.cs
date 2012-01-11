using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.QueryParsing;
using WeenyMapper.Specs.TestClasses.Conventions;

namespace WeenyMapper.Specs.QueryParsing
{
    [TestFixture]
    public class PropertyExpressionSpecs
    {
        [Test]
        public void Translates_property_name_to_column_name_when_translated()
        {
            var expression = new PropertyExpression("PropertyName");

            var expectedExpression = new PropertyExpression("PROPERTYNAME");
            var actualExpression = expression.Translate(new UpperCaseConvention());

            Assert.AreEqual(expectedExpression, actualExpression);
        }        
    }
}