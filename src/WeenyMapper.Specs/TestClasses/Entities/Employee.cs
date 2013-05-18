using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Extensions;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Employee
    {
        public Employee()
        {
            Subordinates = new List<Employee>();
            Mentees = new List<Employee>();
            BirthDate = new DateTime(1970, 1, 1);
        }

        public Employee(string firstName, string lastName) : this()
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }

        public int? ManagerId { get; set; }
        public int CompanyId { get; set; }
        public int? MentorId { get; set; }

        public Company Company { get; set; }
        public Employee Manager { get; set; }
        public Employee Mentor { get; set; }
        public IList<Employee> Subordinates { get; set; }
        public IList<Employee> Mentees { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Employee;

            if (other == null)
            {
                return false;
            }

            return Id == other.Id &&
                   FirstName == other.FirstName &&
                   LastName == other.LastName &&
                   BirthDate == other.BirthDate &&
                   ManagerId == other.ManagerId &&
                   CompanyId == other.CompanyId &&
                   Company.NullSafeIdEquals(other.Company, x => x.Id) &&
                   Manager.NullSafeIdEquals(other.Manager, x => x.Id) &&
                   Mentor.NullSafeIdEquals(other.Mentor, x => x.Id) &&
                   Subordinates.Select(x => x.Id).ElementEquals(other.Subordinates.Select(x => x.Id)) &&
                   Mentees.Select(x => x.Id).ElementEquals(other.Mentees.Select(x => x.Id));
        }

        public void AddSubordinate(Employee subordinate)
        {
            Subordinates.Add(subordinate);
            subordinate.Manager = this;
            subordinate.RefreshReferencedIds();
        }

        public void AddMentee(Employee mentee)
        {
            Mentees.Add(mentee);
            mentee.Mentor = this;
            mentee.RefreshReferencedIds();
        }

        public void RefreshReferencedIds()
        {
            if (Company != null)
            {
                CompanyId = Company.Id;
            }
            if (Manager != null)
            {
                ManagerId = Manager.Id;
            }
            if (Mentor != null)
            {
                MentorId = Mentor.Id;
            }
        }

        public override string ToString()
        {
            return string.Format("Employee (Id: {0}, FirstName: {1}, LastName: {2}, BirthDate: {3}, ManagerId: {4}, CompanyId: {5})", Id, FirstName, LastName, BirthDate, ManagerId, CompanyId);
        }
    }
}