using System;
using System.Collections.Generic;
using WeenyMapper.QueryParsing;
using System.Linq;

namespace WeenyMapper.Sql
{
    public abstract class QueryExpressionTree
    {
        public abstract void Accept(IQueryExpressionTreeVisitor visitor);

        public virtual bool IsEmpty()
        {
            return false;
        }
    }

    public class QueryExpressionTreeLeaf : QueryExpressionTree
    {
        public QueryExpression QueryExpression { get; set; }
        public Type Type { get; set; }

        public QueryExpressionTreeLeaf(QueryExpression queryExpression, Type type)
        {
            QueryExpression = queryExpression;
            Type = type;
        }

        public override void Accept(IQueryExpressionTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class QueryExpressionTreeAndBranch : QueryExpressionTree
    {
        public QueryExpressionTreeAndBranch(params QueryExpressionTree[] nodes)
        {
            Nodes = nodes.ToList();
        }

        public IList<QueryExpressionTree> Nodes { get; set; }

        public override void Accept(IQueryExpressionTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class QueryExpressionTreeOrBranch : QueryExpressionTree
    {
        public QueryExpressionTreeOrBranch(params QueryExpressionTree[] nodes)
        {
            Nodes = nodes.ToList();
        }

        public IList<QueryExpressionTree> Nodes { get; set; }

        public override void Accept(IQueryExpressionTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class EmptyQueryExpressionTree : QueryExpressionTree
    {
        public override void Accept(IQueryExpressionTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool IsEmpty()
        {
            return true;
        }
    }
}