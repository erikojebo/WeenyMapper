using NUnit.Framework;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
{
    [TestFixture]
    public class PageSpecs
    {
        private Page _page;

        [SetUp]
        public void SetUp()
        {
            _page = new Page(3, 9);
        }

        [Test]
        public void Low_row_limit_is_page_index_times_page_size_plus_one()
        {
            Assert.AreEqual(28, _page.LowRowLimit);
        }
        
        [Test]
        public void High_row_limit_is_page_index_plus_one_times_page_size()
        {
            Assert.AreEqual(36, _page.LowRowLimit);
        }
    }
}