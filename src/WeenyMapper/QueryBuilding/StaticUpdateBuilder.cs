using System;
using WeenyMapper.Conventions;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryBuilding
{
    public class StaticUpdateBuilder<T>
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;

        public StaticUpdateBuilder(IConvention convention, ISqlGenerator sqlGenerator)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
        }

        public string ConnectionString { get; set; }
    }
}