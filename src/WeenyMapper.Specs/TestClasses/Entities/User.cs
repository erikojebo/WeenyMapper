using System;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as User;

            if (other == null)
            {
                return false;
            }

            return Id == other.Id &&
                   Username == other.Username &&
                   Password == other.Password;
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Username: {1}, Password: {2}", Id, Username, Password);
        }
    }
}