using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class RequestWithFileDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public FileEntityDto? File { get; set; }
    }

    public class FileEntityDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;

        public string Url { get; set; } = null!;
    }

}