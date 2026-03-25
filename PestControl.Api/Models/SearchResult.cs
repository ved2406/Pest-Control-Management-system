namespace PestControl.Api.Models
{
    public class SearchResult
    {
        public string Category { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public SearchResult(string category, int id, string title, string description)
        {
            Category = category;
            Id = id;
            Title = title;
            Description = description;
        }
    }
}
