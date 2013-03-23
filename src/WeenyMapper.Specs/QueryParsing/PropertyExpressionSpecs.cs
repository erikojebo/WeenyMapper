using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.QueryParsing;
using WeenyMapper.Specs.TestClasses.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs.QueryParsing
{
    [TestFixture]
    public class ReflectedPropertyExpressionSpecs
    {
        [Test]
        public void Translates_property_name_to_column_name_when_translated()
        {
            var expression = new ReflectedPropertyExpression(typeof(User).GetProperty("Username"));

            var expectedExpression = new PropertyExpression("USERNAME", typeof(string));
            var actualExpression = expression.Translate(new UpperCaseConvention());

            Assert.AreEqual(expectedExpression, actualExpression);
        }        
    }
}