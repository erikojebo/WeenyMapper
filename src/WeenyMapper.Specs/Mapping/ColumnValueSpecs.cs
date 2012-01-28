using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Mapping;
using WeenyMapper.Specs.TestClasses.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs.Mapping
{
    [TestFixture]
    public class ColumnValueSpecs
    {
        private ColumnValue _qualifiedColumnValue;
        private ColumnValue _unqualifiedColumnValue;

        [SetUp]
        public void SetUp()
        {
            _qualifiedColumnValue = new ColumnValue("Movie Title", 1);
            _unqualifiedColumnValue = new ColumnValue("Title", 1);
        }

        [Test]
        public void Column_name_is_same_as_alias_for_unqualified_name()
        {
            Assert.AreEqual("Title", _unqualifiedColumnValue.ColumnName);
        }

        [Test]
        public void Column_name_is_second_word_in_table_qualified_name()
        {
            Assert.AreEqual("Title", _qualifiedColumnValue.ColumnName);
        }

        [Test]
        public void Has_table_qualified_alias_if_alias_consists_of_multiple_words()
        {
            Assert.IsTrue(_qualifiedColumnValue.HasTableQualifiedAlias);
        }

        [Test]
        public void Does_not_have_table_qualified_alias_if_alias_consists_of_single_words()
        {
            Assert.IsFalse(_unqualifiedColumnValue.HasTableQualifiedAlias);
        }

        [Test]
        public void Is_for_type_when_qualified_with_type_name()
        {
            Assert.IsTrue(_qualifiedColumnValue.IsForType(typeof(Movie), new DefaultConvention()));
        }

        [Test]
        public void Is_not_for_type_when_qualified_with_different_type_name()
        {
            Assert.IsFalse(_qualifiedColumnValue.IsForType(typeof(User), new DefaultConvention()));
        }
        
        [Test]
        public void Is_for_type_when_qualified_with_table_name_from_type_name_and_convention()
        {
            var columnValue = new ColumnValue("MOVIE COLUMNNAME", 0);
            Assert.IsTrue(columnValue.IsForType(typeof(Movie), new UpperCaseConvention()));
        }

        [Test]
        public void Is_for_type_when_alias_is_unqualified()
        {
            Assert.IsTrue(_unqualifiedColumnValue.IsForType(typeof(Movie), new DefaultConvention()));
        }
    }
}