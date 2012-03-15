using System;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public MovieGenre Genre { get; set; }

        // Make setter private to test conventions
        public int Rating { get; private set; }

        public void SetRating(int rating)
        {
            Rating = rating;
        }

        // Property without setter to test conventions
        public int TitleLength
        {
            get { return Title.Length; }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Movie;

            if (other == null)
            {
                return false;
            }

            return Id == other.Id &&
                   Title == other.Title &&
                   ReleaseDate == other.ReleaseDate &&
                   Rating == other.Rating;
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, title: {1}, release date: {2}, rating: {3}", Id, Title, ReleaseDate, Rating);
        }
    }

    public enum MovieGenre
    {
        Drama, Comedy, SciFi
    }
}