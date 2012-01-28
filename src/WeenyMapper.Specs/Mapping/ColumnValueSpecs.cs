using NUnit.Framework;
using WeenyMapper.Mapping;

namespace WeenyMapper.Specs.Mapping
{
    [TestFixture]
    public class ColumnValueSpecs
    {
        [Test]
        public void Column_name_is_same_as_alias_for_unqualified_name()
        {
            var columnValue = new ColumnValue("ColumnName", 1);
            Assert.AreEqual("ColumnName", columnValue.ColumnName);
        }

        [Test]
        public void Column_name_is_second_word_in_table_qualified_name()
        {
            var columnValue = new ColumnValue("TableName ColumnName", 1);
            Assert.AreEqual("ColumnName", columnValue.ColumnName);
        }

        [Test]
        public void Has_table_qualified_alias_if_alias_consists_of_multiple_words()
        {
            var columnValue = new ColumnValue("TableName ColumnName", 1);
            Assert.IsTrue(columnValue.HasTableQualifiedAlias);
        }

        [Test]
        public void Does_not_have_table_qualified_alias_if_alias_consists_of_single_words()
        {
            var columnValue = new ColumnValue("ColumnName", 1);
            Assert.IsFalse(columnValue.HasTableQualifiedAlias);
        }
    }
}