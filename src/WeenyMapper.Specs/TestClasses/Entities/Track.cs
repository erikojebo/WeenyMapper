namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Track
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int AlbumId { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Track;

            if (other == null)
                return false;

            return Id == other.Id &&
                   Title == other.Title &&
                   AlbumId == other.AlbumId;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Track Id: {0}, Title: {1}, AlbumId: {2}", Id, Title, AlbumId);
        }
    }
}