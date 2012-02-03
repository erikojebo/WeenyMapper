using System.Collections.Generic;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IList<Employee> Employees { get; set; }
    }
}