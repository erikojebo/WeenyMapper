using System.Collections.Generic;
using WeenyMapper.Extensions;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Company
    {
        public Company()
        {
            Employees = new List<Employee>();
            Departments = new List<Department>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public IList<Employee> Employees { get; set; }
        public IList<Department> Departments { get; set; }
 
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Company;

            if (other == null)
            {
                return false;
            }

            return Id == other.Id &&
                   Name == other.Name &&
                   Employees.ElementEquals(other.Employees) &&
                   Departments.ElementEquals(other.Departments);
        }

        public override string ToString()
        {
            return string.Format("Company Id: {0}, Name: {1}, Employee count: {2}", Id, Name, Employees.Count);
        }

        public void AddEmployee(Employee employee)
        {
            Employees.Add(employee);
            employee.Company = this;

            employee.RefreshReferencedIds();
        }

        public void AddDepartment(Department department)
        {
            Departments.Add(department);
            department.CompanyId = Id;
        }

        public void RemoveEmployee(Employee employee)
        {
            Employees.Remove(employee);
            employee.Company = null;
        }
    }
}