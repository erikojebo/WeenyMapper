using System;
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
        public static OrderByStatement CreateAscending(string propertyName)
        {
            var statement = new OrderByStatement
                {
                    Direction = OrderByDirection.Ascending,
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