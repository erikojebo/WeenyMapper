using NUnit.Framework;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
{
    [TestFixture]
    public class CommandParameterFactorySpecs
    {
        private CommandParameterFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new CommandParameterFactory();            
        }

        [Test]
        public void Creating_single_parameter_with_given_column_name_and_value_yields_parameter_with_same_column_name_and_value()
        {
            var parameter = _factory.Create("ColumnName", "value");
            var expectedParameter = new CommandParameter("ColumnName", "value") { ColumnNameOccurrenceIndex = 0 };

            Assert.AreEqual(expectedParameter, parameter);
        }

        [Test]
        public void ColumnNameOccurrence_is_incremented_for_each_parameter_created_with_a_given_name()
        {
            var parameter1 = _factory.Create("ColumnName1", 1);
            var parameter2 = _factory.Create("ColumnName1", 2);
            var parameter3 = _factory.Create("ColumnName2", 1);
            var parameter4 = _factory.Create("ColumnName1", 3);
            var parameter5 = _factory.Create("ColumnName3", 1);
            var parameter6 = _factory.Create("ColumnName2", 1);

            var expectedParameter1 = new CommandParameter("ColumnName1", 1) { ColumnNameOccurrenceIndex = 0 };
            var expectedParameter2 = new CommandParameter("ColumnName1", 2) { ColumnNameOccurrenceIndex = 1 };
            var expectedParameter3 = new CommandParameter("ColumnName2", 1) { ColumnNameOccurrenceIndex = 0 };
            var expectedParameter4 = new CommandParameter("ColumnName1", 3) { ColumnNameOccurrenceIndex = 2 };
            var expectedParameter5 = new CommandParameter("ColumnName3", 1) { ColumnNameOccurrenceIndex = 0 };
            var expectedParameter6 = new CommandParameter("ColumnName2", 1) { ColumnNameOccurrenceIndex = 1 };

            Assert.AreEqual(expectedParameter1, parameter1);
            Assert.AreEqual(expectedParameter2, parameter2);
            Assert.AreEqual(expectedParameter3, parameter3);
            Assert.AreEqual(expectedParameter4, parameter4);
            Assert.AreEqual(expectedParameter5, parameter5);
            Assert.AreEqual(expectedParameter6, parameter6);
        }
    }
}