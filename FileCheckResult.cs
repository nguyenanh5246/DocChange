namespace DocChange.Models.Change;

public class FileCheckResult
{
    public string FilePath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public bool IsValid { get; set; }

    public List<string> MissingValues { get; set; } = new();
}