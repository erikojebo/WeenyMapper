using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Async;
using WeenyMapper.Exceptions;
using WeenyMapper.QueryExecution;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicSelectBuilder<T> : DynamicCommandBuilderBase where T : new()
    {
        private readonly IObjectQueryExecutor _objectQueryExecutor;

        public DynamicSelectBuilder(IObjectQueryExecutor objectQueryExecutor)
        {
            _objectQueryExecutor = objectQueryExecutor;
        }

        public T Execute()
        {
            var result = ExecuteList();

            if (!result.Any())
            {
                throw new WeenyMapperException("No rows matched the given query");
            }

            return result.First();
        }

        public void ExecuteAsync(Action<T> callback)
        {
            TaskRunner.Run(Execute, callback);
        }

        public IList<T> ExecuteList()
        {
            var constraints = GetPropertyValues("Where");
            var propertiesToSelect = GetPropertyNames("Select");

            if (propertiesToSelect.Any())
            {
                return _objectQueryExecutor.Find<T>(typeof(T).Name, constraints, propertiesToSelect);
            }

            return _objectQueryExecutor.Find<T>(typeof(T).Name, constraints);
        }

        public void ExecuteListAsync(Action<IList<T>> callback)
        {
            TaskRunner.Run(ExecuteList, callback);
        }

        public TScalar ExecuteScalar<TScalar>()
        {
            var constraints = GetPropertyValues("Where");
            var propertiesToSelect = GetPropertyNames("Select");

            if (propertiesToSelect.Any())
            {
                return _objectQueryExecutor.FindScalar<T, TScalar>(typeof(T).Name, constraints, propertiesToSelect);
            }

            return _objectQueryExecutor.FindScalar<T, TScalar>(typeof(T).Name, constraints);
        }

        public void ExecuteScalarAsync<TScalar>(Action<TScalar> callback)
        {
            TaskRunner.Run(ExecuteScalar<TScalar>, callback);
        }

        protected override IEnumerable<MethodPatternDescription> MethodPatternDescriptions
        {
            get
            {
                return new[]
                    {
                        new MethodPatternDescription { HasParameter = true, MethodNamePrefix = "Where" },
                        new MethodPatternDescription { HasParameter = false, MethodNamePrefix = "Select" },
                    };
            }
        }
    }

    public class MethodPatternDescription
    {
        public bool HasParameter { get; set; }
        public string MethodNamePrefix { get; set; }
    }
}