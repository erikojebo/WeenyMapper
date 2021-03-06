using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
{
    [TestFixture]
    public class QueryOptimizerSpecs
    {
        private QueryOptimizer _optimizer;

        [SetUp]
        public void SetUp()
        {
            _optimizer = new QueryOptimizer();
        }

        [Test]
        public void Optimizing_parens_for_empty_query_yields_empty_query()
        {
            AssertParenReduction("", "");
        }
        
        [Test]
        public void Optimizing_parens_for_query_without_parent_yields_same_query()
        {
            AssertParenReduction("query", "query");
        }

        [Test]
        public void Optimizing_parens_for_query_only_consisting_of_two_matched_parens_yields_empty_query()
        {
            AssertParenReduction("", "()");
        }
        
        [Test]
        public void Optimizing_parens_for_query_only_consisting_of_two_unmatched_parens_yields_same_query()
        {
            AssertParenReduction(")(", ")(");
        }

        [Test]
        public void Optimizing_parens_for_query_surrounded_by_matched_parens_yields_query_with_outer_parens_stripped()
        {
            AssertParenReduction("expression", "(expression)");
        }
        
        [Test]
        public void Optimizing_parens_for_query_surrounded_by_double_matched_parens_yields_query_with_outer_parens_stripped()
        {
            AssertParenReduction("expression", "((expression))");
        }

        [Test]
        public void Optimizing_parens_for_query_consisting_of_two_parenthesized_sub_expressions_yields_same_expression()
        {
            AssertParenReduction("(sub expression) AND (other sub expression)", "(sub expression) AND (other sub expression)");
        }

        [Test]
        public void Optimizing_parens_for_query_consisting_of_two_parenthesized_sub_expressions_surrounded_by_matching_parens_yields_expression_without_surrounding_parens()
        {
            AssertParenReduction("(sub expression) AND (other sub expression)", "((sub expression) AND (other sub expression))");
        }

        private void AssertParenReduction(string expected, string expression)
        {
            Assert.AreEqual(expected, _optimizer.ReduceParens(expression));
        }
    }

}