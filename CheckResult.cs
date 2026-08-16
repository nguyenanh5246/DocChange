namespace DocChange.Models.Change;

public class CheckResult
{
    public int TotalFiles { get; set; }

    public int ValidFiles { get; set; }

    public int InvalidFiles { get; set; }

    public List<FileCheckResult> FileResults { get; set; } = new();
}