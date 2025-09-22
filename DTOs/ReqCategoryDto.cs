
namespace DTOs
{
    public class ReqCategoryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        // public ICollection<Request>? Requests { get; set; }
    }
    public class CreateCategoryDto
    {
        public string? Name { get; set; }

    }


}
// public List<ReqCategoryDto> ReqCategory = new List<ReqCategoryDto>
// {
//     new ReqCategoryDto { Id = 1, Name = "HelpDesk" },
//     new ReqCategoryDto { Id = 2, Name = "Network" },
//     new ReqCategoryDto { Id = 3, Name = "Programmer" },
//     new ReqCategoryDto { Id = 4, Name = "HR" },
//     new ReqCategoryDto { Id = 5, Name = "Facilities" }
// };