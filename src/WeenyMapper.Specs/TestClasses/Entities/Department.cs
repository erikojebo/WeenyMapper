namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Department
    {
        public Department()
        {
            
        }

        public Department(string name) : this()
        {
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Department;

            if (other == null)
                return false;

            return Id == other.Id &&
                   Name == other.Name &&
                   CompanyId == other.CompanyId;
        }
    }
}