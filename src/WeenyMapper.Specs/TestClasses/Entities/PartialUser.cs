using System;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class PartialUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as PartialUser;

            if (other == null)
            {
                return false;
            }

            return Id == other.Id &&
                   Username == other.Username;
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Username: {1}", Id, Username);
        }
    }
}