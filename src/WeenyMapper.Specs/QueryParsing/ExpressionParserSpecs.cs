using System.Collections.Generic;
using NUnit.Framework;
using WeenyMapper.QueryParsing;
using WeenyMapper.Specs.TestClasses.Entities;
using System.Linq;

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
        public void Single_equality_comparison_with_object_property_invocation_is_parsed_into_property_name_and_value()
        {
            var user = new User { Username = "A username" };

            var expression = _parser.Parse<User>(x => x.Username == user.Username);

            var expectedExpression = new EqualsExpression(new PropertyExpression("Username"), new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Conjunction_of_multiple_equality_comparisons_is_parsed_into_AndExpression_with_corresponding_property_names_and_values()
        {
            var pageCount = 123;
            var expression = _parser.Parse<Book>(
                x => x.AuthorName == "An author name" &&
                     x.Title == GetTitle() &&
                     x.PageCount == pageCount &&
                     x.Isbn == "123-456-789");

            var expectedExpression = new AndExpression(
                new EqualsExpression(new PropertyExpression("AuthorName"), new ValueExpression("An author name")),
                new EqualsExpression(new PropertyExpression("Title"), new ValueExpression("a title")),
                new EqualsExpression(new PropertyExpression("PageCount"), new ValueExpression(123)),
                new EqualsExpression(new PropertyExpression("Isbn"), new ValueExpression("123-456-789")));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Disjunction_of_multiple_equality_comparisons_is_parsed_into_OrExpression_with_corresponding_property_names_and_values()
        {
            var pageCount = 123;
            var expression = _parser.Parse<Book>(
                x => x.AuthorName == "An author name" ||
                     x.Title == GetTitle() ||
                     x.PageCount == pageCount ||
                     x.Isbn == "123-456-789");

            var expectedExpression = new OrExpression(
                new EqualsExpression(new PropertyExpression("AuthorName"), new ValueExpression("An author name")),
                new EqualsExpression(new PropertyExpression("Title"), new ValueExpression("a title")),
                new EqualsExpression(new PropertyExpression("PageCount"), new ValueExpression(123)),
                new EqualsExpression(new PropertyExpression("Isbn"), new ValueExpression("123-456-789")));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Mixed_conjunction_and_disjunction_of_multiple_equality_comparisons_is_parsed_into_AndExpression_and_OrExpression_with_corresponding_property_names_and_values()
        {
            var pageCount = 123;
            var expression = _parser.Parse<Book>(
                x => x.AuthorName == "An author name" ||
                     x.Title == GetTitle() &&
                     x.PageCount == pageCount &&
                     x.Isbn == "123-456-789");

            var expectedExpression = new OrExpression(
                new EqualsExpression(new PropertyExpression("AuthorName"), new ValueExpression("An author name")),
                new AndExpression(
                    new EqualsExpression(new PropertyExpression("Title"), new ValueExpression("a title")),
                    new EqualsExpression(new PropertyExpression("PageCount"), new ValueExpression(123)),
                    new EqualsExpression(new PropertyExpression("Isbn"), new ValueExpression("123-456-789"))));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Parenthesized_mixed_conjunction_and_disjunction_of_multiple_equality_comparisons_is_parsed_into_AndExpression_and_OrExpression_with_corresponding_property_names_and_values()
        {
            var pageCount = 123;
            var expression = _parser.Parse<Book>(
                x => (x.AuthorName == "An author name" ||
                      x.Title == GetTitle()) &&
                     (x.PageCount == pageCount ||
                      x.Isbn == "123-456-789"));

            var expectedExpression = new AndExpression(
                new OrExpression(
                    new EqualsExpression(new PropertyExpression("AuthorName"), new ValueExpression("An author name")),
                    new EqualsExpression(new PropertyExpression("Title"), new ValueExpression("a title"))),
                new OrExpression(
                    new EqualsExpression(new PropertyExpression("PageCount"), new ValueExpression(123)),
                    new EqualsExpression(new PropertyExpression("Isbn"), new ValueExpression("123-456-789"))));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_less_comparison_with_constant_is_parsed_into_LessExpression_with_property_name_and_value()
        {
            var expression = _parser.Parse<Book>(x => x.PageCount < 500);

            var expectedExpression = new LessExpression(new PropertyExpression("PageCount"), new ValueExpression(500));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_greater_comparison_with_constant_is_parsed_into_LessExpression_with_property_name_and_value()
        {
            var expression = _parser.Parse<Book>(x => x.PageCount > 500);

            var expectedExpression = new GreaterExpression(new PropertyExpression("PageCount"), new ValueExpression(500));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_greater_or_equal_comparison_with_constant_is_parsed_into_LessExpression_with_property_name_and_value()
        {
            var expression = _parser.Parse<Book>(x => x.PageCount >= 500);

            var expectedExpression = new GreaterOrEqualExpression(new PropertyExpression("PageCount"), new ValueExpression(500));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_less_or_equal_comparison_with_constant_is_parsed_into_LessExpression_with_property_name_and_value()
        {
            var expression = _parser.Parse<Book>(x => x.PageCount <= 500);

            var expectedExpression = new LessOrEqualExpression(new PropertyExpression("PageCount"), new ValueExpression(500));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Linq_contains_call_with_ienumerable_is_parsed_into_InExpression_with_property_name_and_values()
        {
            IEnumerable<string> titles = new List<string> { "Title 1", "Title 2" };
                
            var expression = _parser.Parse<Book>(x => titles.Contains(x.Title));

            var expectedExpression = new InExpression(
                new PropertyExpression("Title"), 
                new ArrayValueExpression(titles));

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