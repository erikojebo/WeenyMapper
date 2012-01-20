using System;
using System.Data;

namespace WeenyMapper.Sql
{
    public class Page
    {
        public Page() {}

        public Page(int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int LowRowLimit
        {
            get { return PageIndex * PageSize + 1; }
        }

        public int HighRowLimit
        {
            get { return (PageIndex + 1) * PageSize; }
        }
    }
}