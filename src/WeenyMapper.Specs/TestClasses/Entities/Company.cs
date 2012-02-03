using System.Collections.Generic;
using WeenyMapper.Extensions;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IList<Employee> Employees { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Company;

            return Id == other.Id &&
                   Name == other.Name &&
                   Employees.ElementEquals(other.Employees);
        }
    }
}