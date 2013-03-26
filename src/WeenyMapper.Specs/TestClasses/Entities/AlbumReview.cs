namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class AlbumReview
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        public Album Album { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as AlbumReview;

            if (other == null)
                return false;

            return Id == other.Id &&
                   Title == other.Title &&
                   Body == other.Body &&
                   Equals(Album, other.Album);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Title: {1}, Body: {2}, Album: {3}", Id, Title, Body, Album);
        }
    }
}