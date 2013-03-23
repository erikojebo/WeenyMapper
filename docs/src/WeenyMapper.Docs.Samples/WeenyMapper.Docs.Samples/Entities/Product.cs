namespace WeenyMapper.Docs.Samples.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        
        public bool IsDirty { get; set; }
    }
}