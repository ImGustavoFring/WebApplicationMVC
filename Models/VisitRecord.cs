namespace WebApplicationMVC.Models
{
    public class VisitRecord
    {
        public int UserId { get; set; }
        public DateOnly Day { get; set; }
        public int Count { get; set; }
    }

}
