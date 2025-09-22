namespace Models;

public class FileEntity
{
    public int Id { get; set; }
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public bool isDeleted { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

