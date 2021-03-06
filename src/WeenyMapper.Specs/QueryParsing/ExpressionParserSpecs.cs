using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
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

            var expectedExpression = new EqualsExpression(new PropertyExpression("Username", typeof(string)),
                                                          new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_equality_comparison_with_variable_is_parsed_into_property_name_and_value()
        {
            var localVariable = "a username";
            var expression = _parser.Parse<User>(x => x.Username == localVariable);

            var expectedExpression = new EqualsExpression(new PropertyExpression("Username", typeof(string)),
                                                          new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_equality_comparison_with_method_invocation_is_parsed_into_property_name_and_value()
        {
            var expression = _parser.Parse<User>(x => x.Username == GetUsername());

            var expectedExpression = new EqualsExpression(new PropertyExpression("Username", typeof(string)),
                                                          new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_equality_comparison_with_object_property_invocation_is_parsed_into_property_name_and_value()
        {
            var user = new User { Username = "a username" };

            var expression = _parser.Parse<User>(x => x.Username == user.Username);

            var expectedExpression = new EqualsExpression(new PropertyExpression("Username", typeof(string)),
                                                          new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_equality_comparison_with_new_object_is_parsed_into_property_name_and_value()
        {
            var expression = _parser.Parse<Comment>(x => x.PublishDate == new DateTime(2012, 1, 2));

            var expectedExpression = new EqualsExpression(new PropertyExpression("PublishDate", typeof(DateTime)),
                                                          new ValueExpression(new DateTime(2012, 1, 2)));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_equality_comparison_with_method_parameter_is_parsed_into_property_name_and_value()
        {
            AssertParsingOfEqualsExpressionWithMethodParameter("a username");
        }

        private void AssertParsingOfEqualsExpressionWithMethodParameter(string username)
        {
            var expression = _parser.Parse<User>(x => x.Username == username);

            var expectedExpression = new EqualsExpression(new PropertyExpression("Username", typeof(string)),
                                                          new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_equality_comparison_with_lambda_parameter_is_parsed_into_property_name_and_value()
        {
            Func<string, QueryExpression> func = s => _parser.Parse<User>(x => x.Username == s);
            var expression = func("a username");

            var expectedExpression = new EqualsExpression(new PropertyExpression("Username", typeof(string)),
                                                          new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_equality_comparison_with_property_acces_on_lambda_parameter_is_parsed_into_property_name_and_value()
        {
            var user = new User { Username = "a username" };

            Func<User, QueryExpression> func = u => _parser.Parse<User>(x => x.Username == u.Username);
            var expression = func(user);

            var expectedExpression = new EqualsExpression(new PropertyExpression("Username", typeof(string)),
                                                          new ValueExpression("a username"));

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
                new EqualsExpression(new PropertyExpression("AuthorName", typeof(string)), new ValueExpression("An author name")),
                new EqualsExpression(new PropertyExpression("Title", typeof(string)), new ValueExpression("a title")),
                new EqualsExpression(new PropertyExpression("PageCount", typeof(int)), new ValueExpression(123)),
                new EqualsExpression(new PropertyExpression("Isbn", typeof(string)), new ValueExpression("123-456-789")));

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
                new EqualsExpression(new PropertyExpression("AuthorName", typeof(string)), new ValueExpression("An author name")),
                new EqualsExpression(new PropertyExpression("Title", typeof(string)), new ValueExpression("a title")),
                new EqualsExpression(new PropertyExpression("PageCount", typeof(int)), new ValueExpression(123)),
                new EqualsExpression(new PropertyExpression("Isbn", typeof(string)), new ValueExpression("123-456-789")));

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
                new EqualsExpression(new PropertyExpression("AuthorName", typeof(string)), new ValueExpression("An author name")),
                new AndExpression(
                    new EqualsExpression(new PropertyExpression("Title", typeof(string)), new ValueExpression("a title")),
                    new EqualsExpression(new PropertyExpression("PageCount", typeof(int)), new ValueExpression(123)),
                    new EqualsExpression(new PropertyExpression("Isbn", typeof(string)), new ValueExpression("123-456-789"))));

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
                    new EqualsExpression(new PropertyExpression("AuthorName", typeof(string)), new ValueExpression("An author name")),
                    new EqualsExpression(new PropertyExpression("Title", typeof(string)), new ValueExpression("a title"))),
                new OrExpression(
                    new EqualsExpression(new PropertyExpression("PageCount", typeof(int)), new ValueExpression(123)),
                    new EqualsExpression(new PropertyExpression("Isbn", typeof(string)), new ValueExpression("123-456-789"))));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_less_comparison_with_constant_is_parsed_into_LessExpression_with_property_name_and_value()
        {
            var expression = _parser.Parse<Book>(x => x.PageCount < 500);

            var expectedExpression = new LessExpression(new PropertyExpression("PageCount", typeof(int)), new ValueExpression(500));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_greater_comparison_with_constant_is_parsed_into_LessExpression_with_property_name_and_value()
        {
            var expression = _parser.Parse<Book>(x => x.PageCount > 500);

            var expectedExpression = new GreaterExpression(new PropertyExpression("PageCount", typeof(int)), new ValueExpression(500));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_greater_or_equal_comparison_with_constant_is_parsed_into_LessExpression_with_property_name_and_value()
        {
            var expression = _parser.Parse<Book>(x => x.PageCount >= 500);

            var expectedExpression = new GreaterOrEqualExpression(new PropertyExpression("PageCount", typeof(int)),
                                                                  new ValueExpression(500));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Single_less_or_equal_comparison_with_constant_is_parsed_into_LessExpression_with_property_name_and_value()
        {
            var expression = _parser.Parse<Book>(x => x.PageCount <= 500);

            var expectedExpression = new LessOrEqualExpression(new PropertyExpression("PageCount", typeof(int)),
                                                               new ValueExpression(500));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Linq_contains_call_with_ienumerable_of_string_is_parsed_into_InExpression_with_property_name_and_values()
        {
            IEnumerable<string> titles = new List<string> { "Title 1", "Title 2" };

            var expression = _parser.Parse<Book>(x => titles.Contains(x.Title));

            var expectedExpression = new InExpression(
                new PropertyExpression("Title", typeof(string)),
                new ArrayValueExpression(titles));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Linq_contains_call_with_ienumerable_of_int_is_parsed_into_InExpression_with_property_name_and_values()
        {
            IEnumerable<int> pageCounts = new List<int> { 0, 1 };

            var expression = _parser.Parse<Book>(x => pageCounts.Contains(x.PageCount));

            var expectedExpression = new InExpression(
                new PropertyExpression("PageCount", typeof(int)),
                new ArrayValueExpression(pageCounts));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Contains_call_on_generic_list_of_string_is_parsed_into_InExpression_with_property_name_and_values()
        {
            IList<string> titles = new List<string> { "Title 1", "Title 2" };

            var expression = _parser.Parse<Book>(x => titles.Contains(x.Title));

            var expectedExpression = new InExpression(
                new PropertyExpression("Title", typeof(string)),
                new ArrayValueExpression(titles));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Contains_call_on_generic_list_of_int_is_parsed_into_InExpression_with_property_name_and_values()
        {
            var pageCounts = new List<int> { 0, 1 };

            var expression = _parser.Parse<Book>(x => pageCounts.Contains(x.PageCount));

            var expectedExpression = new InExpression(
                new PropertyExpression("PageCount", typeof(int)),
                new ArrayValueExpression(pageCounts));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void String_Contains_call_is_parsed_into_like_expression_with_starting_and_ending_wildcard()
        {
            var expression = _parser.Parse<Book>(x => x.AuthorName.Contains("Steve"));

            var expectedExpression = new LikeExpression(new PropertyExpression("AuthorName", typeof(string)), "Steve")
                {
                    HasStartingWildCard = true,
                    HasEndingWildCard = true
                };

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void String_StartsWith_call_is_parsed_into_like_expression_with_only_ending_wildcard()
        {
            var expression = _parser.Parse<Book>(x => x.AuthorName.StartsWith("Steve"));

            var expectedExpression = new LikeExpression(new PropertyExpression("AuthorName", typeof(string)), "Steve")
                {
                    HasStartingWildCard = false,
                    HasEndingWildCard = true
                };

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void String_EndsWith_call_is_parsed_into_like_expression_with_only_starting_wildcard()
        {
            var expression = _parser.Parse<Book>(x => x.AuthorName.EndsWith("Steve"));

            var expectedExpression = new LikeExpression(new PropertyExpression("AuthorName", typeof(string)), "Steve")
                {
                    HasStartingWildCard = true,
                    HasEndingWildCard = false
                };

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Equals_expression_with_constant_and_property_access_on_related_entity_yields_equals_expression_with_value_and_entity_reference_expression()
        {
            var expression = _parser.Parse<BlogPost>(x => x.Blog.Id == 1);

            var blogProperty = Reflector<BlogPost>.GetProperty(x => x.Blog);
            var blogIdProperty = Reflector<Blog>.GetProperty(x => x.Id);

            var expectedExpression = new EqualsExpression(
                new EntityReferenceExpression(blogProperty, blogIdProperty),
                new ValueExpression(1));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Expression_with_not_equals_operator_yields_NotEqualsExpression()
        {
            var expression = _parser.Parse<User>(x => x.Username != "a username");

            var expectedExpression = new NotEqualExpression(
                new PropertyExpression("Username", typeof(string)),
                new ValueExpression("a username"));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Negated_expression_yields_NotExpression()
        {
            var expression = _parser.Parse<User>(x => !(x.Username == "a username" && x.Password == "a password"));

            var expectedExpression = new NotExpression(
                new AndExpression(
                    new EqualsExpression(
                        new PropertyExpression("Username", typeof(string)),
                        new ValueExpression("a username")),
                    new EqualsExpression(
                        new PropertyExpression("Password", typeof(string)),
                        new ValueExpression("a password"))));

            Assert.AreEqual(expectedExpression, expression);
        }

        [Test]
        public void Bool_property_access_without_explicit_comparison_yields_PropertyExpression_with_boolean_property_type()
        {
            var expression = _parser.Parse<Book>(x => x.IsPublicDomain);

            var expectedExpression = new PropertyExpression("IsPublicDomain", typeof(bool));

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