using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WeenyMapper.Logging;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.SqlScripts
{
    public class TestDatabase
    {
        public static void Create(string connectionString, IDbCommandFactory commandFactory)
        {
            var sqlCommandTexts = ReadCreateDatabaseCommands();

            var commands = sqlCommandTexts.Select(commandFactory.CreateCommand);

            var sqlCommandExecutor = new DbCommandExecutor(new NullSqlCommandLogger(), commandFactory);
            sqlCommandExecutor.ExecuteNonQuery(commands, connectionString);
        }

        private static IEnumerable<string> ReadCreateDatabaseCommands()
        {
            var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var resourceName = resourceNames.FirstOrDefault(x => x.Contains("CreateTestDatabase.sql"));

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return SplitSqlScript(reader.ReadToEnd());
            }
        }

        private static IEnumerable<string> SplitSqlScript(string script)
        {
            return script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Trim())
                         .Where(x => !string.IsNullOrWhiteSpace(x))
                         .ToList();
        }
    }
}