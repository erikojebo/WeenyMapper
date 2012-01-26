using System;
using System.Collections.Generic;

namespace WeenyMapper.Sql
{
    public class CommandParameterFactory : ICommandParameterFactory
    {
        private readonly IDictionary<string, int> _occurrenceCount = new Dictionary<string, int>();

        public string ParameterNamePrefix { get; set; }

        public CommandParameter Create(string columnName, object value)
        {
            if (!_occurrenceCount.ContainsKey(columnName))
            {
                _occurrenceCount[columnName] = 0;
            }
            else
            {
                _occurrenceCount[columnName] = _occurrenceCount[columnName] + 1;
            }

            var columnNameOccurrenceIndex = _occurrenceCount[columnName];
            return new CommandParameter(columnName, value)
                {
                    ColumnNameOccurrenceIndex = columnNameOccurrenceIndex,
                    ParameterNamePrefix = ParameterNamePrefix
                };
        }
    }
}