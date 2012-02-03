using System;
using System.Collections.Generic;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }

        public int ManagerId { get; set; }
        public int CompanyId { get; set; }

        public Company Company { get; set; }
        public Employee Manager { get; set; }
        public IList<Employee> Subordinates { get; set; }
    }
}