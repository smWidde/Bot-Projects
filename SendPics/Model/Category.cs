namespace SendPics
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public virtual System.Collections.Generic.List<Image> Images { get; set; }

    }
}
