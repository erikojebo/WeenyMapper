namespace WeenyMapper.Docs.Samples.Entities
{
    public class LineItem
    {
        public int LineItemId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        
        public bool IsDirty { get; set; }

        public ShoppingCart ShoppingCart { get; set; }
    }
}