namespace DocChange.Models.Change;

public class ProcessResult
{
    public int TotalFiles { get; set; }

    public int ProcessedFiles { get; set; }

    public int SkippedFiles { get; set; }

    public int FailedFiles { get; set; }

    public List<string> SkippedFileNames { get; set; }
        = new();

    public List<string> FailedFileNames { get; set; }
        = new();
}