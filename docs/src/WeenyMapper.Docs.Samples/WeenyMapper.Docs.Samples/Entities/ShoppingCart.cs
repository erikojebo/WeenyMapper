using System.Collections.Generic;

namespace WeenyMapper.Docs.Samples.Entities
{
    public class ShoppingCart
    {
        public ShoppingCart()
        {
            LineItems = new List<LineItem>();
        }

        public int CartId { get; set; }
        public int UserId { get; set; }

        public IList<LineItem> LineItems { get; set; }
    }
}