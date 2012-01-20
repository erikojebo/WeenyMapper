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
    }
}