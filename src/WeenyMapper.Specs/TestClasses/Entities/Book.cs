namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Book
    {
        public string Isbn { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public int PageCount { get; set; }
        public bool IsPublicDomain { get; set; }

        public override int GetHashCode()
        {
            return Isbn.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Book;

            if (other == null)
            {
                return false;
            }

            return Isbn == other.Isbn &&
                   Title == other.Title &&
                   AuthorName == other.AuthorName &&
                   PageCount == other.PageCount;
        }

        public override string ToString()
        {
            return string.Format("Isbn: {0}, Title: {1}, AuthorName: {2}, PageCount: {3}", Isbn, Title, AuthorName, PageCount);
        }
    }
}