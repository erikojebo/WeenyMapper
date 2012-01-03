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
        private ExpressionParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new ExpressionParser();            
        }

        [Test]
        public void Single_equality_comparison_with_constant_is_parsed_into_property_name_and_value()
        {
            var expression = _parser.Parse<User>(x => x.Username == "a username");

            var expectedExpression = new EqualsExpression(new PropertyExpression("Username"), new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_equality_comparison_with_variable_is_parsed_into_property_name_and_value()
        {
            var localVariable = "a username";
            var expression = _parser.Parse<User>(x => x.Username == localVariable);

            var expectedExpression = new EqualsExpression(new PropertyExpression("Username"), new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_equality_comparison_with_method_invocation_is_parsed_into_property_name_and_value()
        {
            var expression = _parser.Parse<User>(x => x.Username == GetUsername());

            var expectedExpression = new EqualsExpression(new PropertyExpression("Username"), new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Conjunction_of_multiple_equality_comparisons_is_parsed_into_AndExpression_with_corresponding_property_names_and_values()
        {
            var pageCount = 123;
            var expression = _parser.Parse<Book>(x => x.AuthorName == "An author name" && x.Title == GetTitle() && x.PageCount == pageCount);

            var expectedExpression = new AndExpression(
                new EqualsExpression(new PropertyExpression("AuthorName"), new ValueExpression("An author name")),
                new EqualsExpression(new PropertyExpression("Title"), new ValueExpression("a title")),
                new EqualsExpression(new PropertyExpression("PageCount"), new ValueExpression(123)));

            Assert.AreEqual(expectedExpression, expression);
        }

        private string GetUsername()
        {
            return "a username";
        }

        private string GetTitle()
        {
            return "a title";
        }
    }
}