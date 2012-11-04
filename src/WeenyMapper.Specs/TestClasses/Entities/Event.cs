using System;

namespace WeenyMapper.Specs.TestClasses.Entities
{
    public class Event
    {
        public Guid AggregateId { get; set; }
        public DateTime PublishDate { get; set; }
        public string Data { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Event;

            if (other == null)
                return false;

            return other.AggregateId == this.AggregateId &&
                   other.PublishDate == this.PublishDate &&
                   other.Data == this.Data;
        }

        public override int GetHashCode()
        {
            return AggregateId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Event (Aggregate id: {0}, PublishDate: {1}, Data: {2}", AggregateId, PublishDate, Data);
        }
    }
}