using WeenyMapper.QueryExecution;

namespace WeenyMapper.QueryBuilding
{
    public class StaticUpdateBuilder<T>
    {
        private readonly IObjectUpdateExecutor _objectUpdateExecutor;

        public StaticUpdateBuilder(IObjectUpdateExecutor objectUpdateExecutor)
        {
            _objectUpdateExecutor = objectUpdateExecutor;
        }

        public void Update(T instance)
        {
            _objectUpdateExecutor.Update(instance);
        }
    }
}