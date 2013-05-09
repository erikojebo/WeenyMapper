using System;
using System.Collections.Generic;
using WeenyMapper.Conventions;
using WeenyMapper.QueryParsing;
using System.Linq;
using WeenyMapper.Reflection;

namespace WeenyMapper.Sql
{
    public abstract class QueryExpressionTree
    {
        public abstract void Accept(IQueryExpressionTreeVisitor visitor);

        public virtual bool IsEmpty()
        {
            return false;
        }

        public abstract QueryExpressionTree Translate(IConventionReader conventionReader);

        public QueryExpressionTree And(QueryExpressionTree leaf)
        {
            if (IsEmpty())
                return leaf;

            return new QueryExpressionTreeAndBranch(this, leaf);
        }
        
        public QueryExpressionTree Or(QueryExpressionTree leaf)
        {
            if (IsEmpty())
                return leaf;
            
            return new QueryExpressionTreeOrBranch(this, leaf);
        }
    }

    public class QueryExpressionTreeLeaf : QueryExpressionTree
    {
        public QueryExpression QueryExpression { get; set; }
        public TableIdentifier TableIdentifier { get; set; }

        public QueryExpressionTreeLeaf(QueryExpression queryExpression, TableIdentifier tableIdentifier)
        {
            QueryExpression = queryExpression;
            TableIdentifier = tableIdentifier;
        }

        public override void Accept(IQueryExpressionTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override QueryExpressionTree Translate(IConventionReader conventionReader)
        {
            return new TranslatedQueryExpressionTreeLeaf(QueryExpression.Translate(conventionReader), TableIdentifier.GetTableIdentifier(conventionReader));
        }
    }

    public class TranslatedQueryExpressionTreeLeaf : QueryExpressionTree
    {
        public QueryExpression QueryExpression { get; set; }
        public string TableIdentifier { get; set; }

        public TranslatedQueryExpressionTreeLeaf(QueryExpression queryExpression, string tableIdentifier)
        {
            QueryExpression = queryExpression;
            TableIdentifier = tableIdentifier;
        }

        public override void Accept(IQueryExpressionTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override QueryExpressionTree Translate(IConventionReader conventionReader)
        {
            return this;
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

        public override QueryExpressionTree Translate(IConventionReader conventionReader)
        {
            return new QueryExpressionTreeAndBranch(Nodes.Select(x => x.Translate(conventionReader)).ToArray());
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
        public override QueryExpressionTree Translate(IConventionReader conventionReader)
        {
            return new QueryExpressionTreeOrBranch(Nodes.Select(x => x.Translate(conventionReader)).ToArray());
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

        public override QueryExpressionTree Translate(IConventionReader conventionReader)
        {
            return this;
        }
    }
}