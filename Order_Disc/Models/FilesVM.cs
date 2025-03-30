namespace Order_Disc.Models
{
    public class FilesVM
    {
        public int Id { get; set; }
        public int FileShareId { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string SharedByFirstName { get; set; } = null!;
        public string SharedByLastName { get; set; } = null!;
        public bool IsShared { get; set; }

        public long SizeInBytes { get; set; }

        public string SizeFormatted => FormatSize(SizeInBytes);

        public DateTime UploadDate { get; set; }

        private string FormatSize(long sizeInBytes)
        {
            if (sizeInBytes >= 1024 * 1024)
                return $"{sizeInBytes / (1024 * 1024):F2} MB";
            if (sizeInBytes >= 1024)
                return $"{sizeInBytes / 1024:F2} KB";
            return $"{sizeInBytes} B";
        }
    }
}
