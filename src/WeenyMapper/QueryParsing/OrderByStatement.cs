using WeenyMapper.Conventions;

namespace WeenyMapper.QueryParsing
{
    public enum OrderByDirection
    {
        Ascending,
        Descending
    }

    public class OrderByStatement
    {
        public static OrderByStatement Create(string propertyName, OrderByDirection orderByDirection)
        {
            var statement = new OrderByStatement
                {
                    Direction = orderByDirection,
                    PropertyName = propertyName
                };

            return statement;
        }

        public string PropertyName { get; private set; }
        public OrderByDirection Direction { get; private set; }

        public OrderByStatement Translate(IConvention convention)
        {
            return new OrderByStatement
                {
                    PropertyName = convention.GetColumnName(PropertyName),
                    Direction = Direction
                };
        }
    }
}