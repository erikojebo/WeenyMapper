using System;
using System.Collections.Generic;

namespace WeenyMapper.QueryParsing
{
    public class SelectQuery 
    {
        public SelectQuery()
        {
            ConstraintProperties = new List<string>();
        }

        public string ClassName { get; set; }
        public IList<string> ConstraintProperties { get; private set; }
    }
}