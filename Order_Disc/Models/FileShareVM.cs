using Order_Disc.Entities;

namespace Order_Disc.Models
{
    public class FileShareVM
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public int SharedWithUserId { get; set; }
        public virtual FileEntity File { get; set; }
        public int FileShareId { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public long SizeInBytes { get; set; }
        public DateTime UploadDate { get; set; }
        public string SharedByFirstName { get; set; } = null!;
        public string SharedByLastName { get; set; } = null!;
    }

}
