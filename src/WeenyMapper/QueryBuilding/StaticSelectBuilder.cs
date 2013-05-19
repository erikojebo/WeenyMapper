using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using WeenyMapper.Async;
using WeenyMapper.QueryExecution;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryBuilding
{
    public class StaticSelectBuilder<T> : StaticCommandBuilderBase<T> where T : new()
    {
        private readonly ISqlQueryExecutor _sqlQueryExecutor;
        private readonly IExpressionParser _expressionParser;
        private readonly SqlQuery _sqlQuery;

        public StaticSelectBuilder(ISqlQueryExecutor sqlQueryExecutor, IExpressionParser expressionParser, IConventionReader conventionReader)
        {
            _sqlQueryExecutor = sqlQueryExecutor;
            _expressionParser = expressionParser;
            _sqlQuery = SqlQuery.Create<T>(conventionReader);
        }

        internal string PrimaryAlias
        {
            set { _sqlQuery.PrimaryAlias = value; }
        }

        public StaticSelectBuilder<T> Where(Expression<Func<T, bool>> queryExpression)
        {
            return AndWhere(queryExpression);
        }

        public StaticSelectBuilder<T> Where<TAliasedEntity>(string alias, Expression<Func<TAliasedEntity, bool>> queryExpression)
        {
            return AndWhere(alias, queryExpression);
        }

        public StaticSelectBuilder<T> AndWhere(Expression<Func<T, bool>> queryExpression)
        {
            return AndWhere(null, queryExpression);
        }

        public StaticSelectBuilder<T> AndWhere<TAliasedEntity>(Expression<Func<TAliasedEntity, bool>> queryExpression)
        {
            return AndWhere(null, queryExpression);
        }

        public StaticSelectBuilder<T> AndWhere<TAliasedEntity>(string alias, Expression<Func<TAliasedEntity, bool>> queryExpression)
        {
            _sqlQuery.AddConjunctionExpression(typeof(TAliasedEntity), alias, _expressionParser.Parse(queryExpression));

            return this;
        }

        public StaticSelectBuilder<T> OrWhere(Expression<Func<T, bool>> queryExpression)
        {
            return OrWhere(null, queryExpression);
        }

        public StaticSelectBuilder<T> OrWhere<TAliasedEntity>(Expression<Func<TAliasedEntity, bool>> queryExpression)
        {
            return OrWhere(null, queryExpression);
        }

        public StaticSelectBuilder<T> OrWhere<TAliasedEntity>(string alias, Expression<Func<TAliasedEntity, bool>> queryExpression)
        {
            _sqlQuery.AddDisjunctionExpression(typeof(TAliasedEntity), alias, _expressionParser.Parse(queryExpression));

            return this;
        }

        public T Execute()
        {
            return ExecuteList().FirstOrDefault();
        }

        public IList<T> ExecuteList()
        {
            return _sqlQueryExecutor.Find<T>(_sqlQuery);
        }

        public StaticSelectBuilder<T> Select(params Expression<Func<T, object>>[] propertySelectors)
        {
            return Select<T>(propertySelectors);
        }

        public StaticSelectBuilder<T> Select<TAliasedEntity>(params Expression<Func<TAliasedEntity, object>>[] propertySelectors)
        {
            return Select(null, propertySelectors);
        }

        public StaticSelectBuilder<T> Select<TAliasedEntity>(string alias, params Expression<Func<TAliasedEntity, object>>[] propertySelectors)
        {
            var propertyNames = propertySelectors.Select(GetPropertyName);

            _sqlQuery.AddPropertiesToSelect<TAliasedEntity>(alias, propertyNames);

            return this;
        }

        public void ExecuteAsync(Action<T> callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(Execute, callback, errorCallback);
        }

        public void ExecuteListAsync(Action<IList<T>> callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(ExecuteList, callback, errorCallback);
        }

        public void ExecuteScalarAsync<TScalar>(Action<TScalar> callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(ExecuteScalar<TScalar>, callback, errorCallback);
        }

        public TScalar ExecuteScalar<TScalar>()
        {
            return _sqlQueryExecutor.FindScalar<T, TScalar>(_sqlQuery);
        }

        public void ExecuteScalarListAsync<TScalar>(Action<IList<TScalar>> callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(ExecuteScalarList<TScalar>, callback, errorCallback);
        }

        public IList<TScalar> ExecuteScalarList<TScalar>()
        {
            return _sqlQueryExecutor.FindScalarList<T, TScalar>(_sqlQuery);
        }

        public StaticSelectBuilder<T> OrderBy(params Expression<Func<T, object>>[] getters)
        {
            return OrderBy<T>(getters);
        }

        public StaticSelectBuilder<T> OrderBy<TEntity>(params Expression<Func<TEntity, object>>[] getters)
        {
            return OrderBy(null, getters);
        }

        public StaticSelectBuilder<T> OrderBy<TEntity>(string alias, params Expression<Func<TEntity, object>>[] getters)
        {
            AddOrderByStatements(getters, OrderByDirection.Ascending, alias);
            return this;
        }

        public StaticSelectBuilder<T> OrderByDescending(params Expression<Func<T, object>>[] getters)
        {
            return OrderByDescending<T>(getters);
        }

        public StaticSelectBuilder<T> OrderByDescending<TEntity>(params Expression<Func<TEntity, object>>[] getters)
        {
            return OrderByDescending(null, getters);
        }

        public StaticSelectBuilder<T> OrderByDescending<TEntity>(string alias, params Expression<Func<TEntity, object>>[] getters)
        {
            AddOrderByStatements(getters, OrderByDirection.Descending, alias);
            return this;
        }

        private void AddOrderByStatements<TEntity>(IEnumerable<Expression<Func<TEntity, object>>> getters, OrderByDirection orderByDirection, string alias = null)
        {
            var propertyNames = getters.Select(GetPropertyName).ToList();

            _sqlQuery.AddOrderByStatements<TEntity>(propertyNames, orderByDirection, alias);
        }

        public StaticSelectBuilder<T> Top(int rowCount)
        {
            _sqlQuery.RowCountLimit = rowCount;

            return this;
        }

        public StaticSelectBuilder<T> Page(int pageIndex, int pageSize)
        {
            _sqlQuery.Page = new Page { PageIndex = pageIndex, PageSize = pageSize };

            return this;
        }

        public StaticSelectBuilder<T> Join<TChild>(Expression<Func<T, IList<TChild>>> parentProperty, Expression<Func<TChild, int>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            return Join<T, TChild>(parentProperty, foreignKeyProperty, childAlias, parentAlias);
        }

        public StaticSelectBuilder<T> Join<TChild>(Expression<Func<T, IList<TChild>>> parentProperty, Expression<Func<TChild, Guid>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            return Join<T, TChild>(parentProperty, foreignKeyProperty, childAlias, parentAlias);
        }

        public StaticSelectBuilder<T> Join<TChild>(Expression<Func<T, IList<TChild>>> parentProperty, Expression<Func<TChild, string>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            return Join<T, TChild>(parentProperty, foreignKeyProperty, childAlias, parentAlias);
        }

        public StaticSelectBuilder<T> Join<TParent, TChild>(Expression<Func<TParent, IList<TChild>>> parentProperty, Expression<Func<TChild, int>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            var foreignKeyPropertyInfo = Reflector<TChild>.GetProperty(foreignKeyProperty);            
            return JoinWithForeignKeyProperty(parentProperty, foreignKeyPropertyInfo, childAlias, parentAlias);
        }

        public StaticSelectBuilder<T> Join<TParent, TChild>(Expression<Func<TParent, IList<TChild>>> parentProperty, Expression<Func<TChild, Guid>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            var foreignKeyPropertyInfo = Reflector<TChild>.GetProperty(foreignKeyProperty);            
            return JoinWithForeignKeyProperty(parentProperty, foreignKeyPropertyInfo, childAlias, parentAlias);
        }

        public StaticSelectBuilder<T> Join<TParent, TChild>(Expression<Func<TParent, IList<TChild>>> parentProperty, Expression<Func<TChild, string>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            var foreignKeyPropertyInfo = Reflector<TChild>.GetProperty(foreignKeyProperty);            
            return JoinWithForeignKeyProperty(parentProperty, foreignKeyPropertyInfo, childAlias, parentAlias);
        }

        public StaticSelectBuilder<T> Join<TChild>(Expression<Func<T, IList<TChild>>> parentProperty, Expression<Func<TChild, long>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            var foreignKeyPropertyInfo = Reflector<TChild>.GetProperty(foreignKeyProperty);            
            return JoinWithForeignKeyProperty(parentProperty, foreignKeyPropertyInfo, childAlias, parentAlias);
        }
        
        public StaticSelectBuilder<T> Join<TChild>(Expression<Func<T, IList<TChild>>> parentProperty, Expression<Func<TChild, int?>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            return Join<T, TChild>(parentProperty, foreignKeyProperty, childAlias, parentAlias);
        }

        public StaticSelectBuilder<T> Join<TChild>(Expression<Func<T, IList<TChild>>> parentProperty, Expression<Func<TChild, Guid?>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            return Join<T, TChild>(parentProperty, foreignKeyProperty, childAlias, parentAlias);
        }

        public StaticSelectBuilder<T> Join<TParent, TChild>(Expression<Func<TParent, IList<TChild>>> parentProperty, Expression<Func<TChild, int?>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            var foreignKeyPropertyInfo = Reflector<TChild>.GetProperty(foreignKeyProperty);            
            return JoinWithForeignKeyProperty(parentProperty, foreignKeyPropertyInfo, childAlias, parentAlias);
        }

        public StaticSelectBuilder<T> Join<TParent, TChild>(Expression<Func<TParent, IList<TChild>>> parentProperty, Expression<Func<TChild, Guid?>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            var foreignKeyPropertyInfo = Reflector<TChild>.GetProperty(foreignKeyProperty);            
            return JoinWithForeignKeyProperty(parentProperty, foreignKeyPropertyInfo, childAlias, parentAlias);
        }

        public StaticSelectBuilder<T> Join<TChild>(Expression<Func<T, IList<TChild>>> parentProperty, Expression<Func<TChild, long?>> foreignKeyProperty, string childAlias = null, string parentAlias = null)
        {
            var foreignKeyPropertyInfo = Reflector<TChild>.GetProperty(foreignKeyProperty);            
            return JoinWithForeignKeyProperty(parentProperty, foreignKeyPropertyInfo, childAlias, parentAlias);
        }

        private StaticSelectBuilder<T> JoinWithForeignKeyProperty<TParent, TChild>(Expression<Func<TParent, IList<TChild>>> parentProperty, PropertyInfo foreignKeyPropertyInfo, string childAlias = null, string parentAlias = null)
        {
            var parentPropertyInfo = Reflector<T>.GetProperty(parentProperty);

            var joinSpecification = ObjectSubQueryJoin.CreateParentToChild(parentPropertyInfo, foreignKeyPropertyInfo);

            Join(joinSpecification, childAlias, parentAlias);

            return this;
        }

        public StaticSelectBuilder<T> Join<TParent>(Expression<Func<T, TParent>> childProperty, string childAlias = null, string parentAlias = null)
        {
            return Join<TParent, T>(childProperty, childAlias, parentAlias);
        }
        
        public StaticSelectBuilder<T> Join<TParent, TChild>(Expression<Func<TChild, TParent>> childProperty, string childAlias = null, string parentAlias = null)
        {
            var childPropertyInfo = Reflector<T>.GetProperty(childProperty);

            var joinSpecification = ObjectSubQueryJoin.CreateChildToParent(childPropertyInfo, typeof(TParent));

            Join(joinSpecification, childAlias, parentAlias);

            return this;
        }

        public StaticSelectBuilder<T> Join<TParent>(
            Expression<Func<TParent, IList<T>>> parentProperty,
            Expression<Func<T, TParent>> childProperty, string childAlias = null, string parentAlias = null)
        {
            return Join<TParent, T>(parentProperty, childProperty, childAlias, parentAlias);
        }
        
        public StaticSelectBuilder<T> Join<TChild>(
            Expression<Func<T, IList<TChild>>> parentProperty,
            Expression<Func<TChild, T>> childProperty, string childAlias = null, string parentAlias = null)
        {
            return Join<T, TChild>(parentProperty, childProperty, childAlias, parentAlias);
        }

        public StaticSelectBuilder<T> Join<TParent, TChild>(
            Expression<Func<TParent, IList<TChild>>> parentProperty,
            Expression<Func<TChild, TParent>> childProperty, string childAlias = null, string parentAlias = null)
        {
            var parentPropertyInfo = Reflector<TParent>.GetProperty(parentProperty);
            var childPropertyInfo = Reflector<TChild>.GetProperty(childProperty);

            var joinSpecification = ObjectSubQueryJoin.CreateTwoWay(parentPropertyInfo, childPropertyInfo);
            Join(joinSpecification, childAlias, parentAlias);

            return this;
        }

        private void Join(ObjectSubQueryJoin joinSpecification, string childAlias, string parentAlias)
        {
            _sqlQuery.AddJoin(joinSpecification, childAlias, parentAlias);
        }
    }
}