using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
{
    [TestFixture]
    public class CommandParameterSpecs
    {
        private CommandParameter _parameter;

        [SetUp]
        public void SetUp()
        {
            _parameter = new CommandParameter("ColumnName", 1);            
        }

        [Test]
        public void Parameter_name_is_column_name_suffixed_by_Constraint()
        {
            Assert.AreEqual("ColumnNameConstraint", _parameter.Name);
        }

        [Test]
        public void Constraint_string_is_ColumnName_operator_ParameterName()
        {
            var constraintString = _parameter.ToConstraintString("<", x => string.Format("[{0}]", x));

            Assert.AreEqual("[ColumnName] < @ColumnNameConstraint", constraintString);
        }
        
        [Test]
        public void Parameter_with_non_zero_occurrence_count_gets_occurrence_count_plus_one_appended_to_parameter_name()
        {
            _parameter.ColumnNameOccurrenceIndex = 1;
            Assert.AreEqual("ColumnNameConstraint2", _parameter.Name);            
        }

        [Test]
        public void Constraint_string_includes_occurrence_count_when_non_zero()
        {
            _parameter.ColumnNameOccurrenceIndex = 1;
            var constraintString = _parameter.ToConstraintString("=", x => x);

            Assert.AreEqual("ColumnName = @ColumnNameConstraint2", constraintString);
        }
    }
}