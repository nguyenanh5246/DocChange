using DocChange.Models.Change;
using System.IO;

namespace DocChange.Services.Change;

public class ValidationService
{
    private readonly DocxReader _reader;

    public ValidationService()
    {
        _reader = new DocxReader();
    }


    public CheckResult CheckFiles(
        string inputFolder,
        List<ReplacementItem> replacements)
    {
        if (!Directory.Exists(inputFolder))
        {
            throw new DirectoryNotFoundException(
                "Input folder không tồn tại.");
        }


        var files =
            Directory.GetFiles(
                inputFolder,
                "*.docx",
                SearchOption.TopDirectoryOnly);


        var result =
            new CheckResult();


        result.TotalFiles =
            files.Length;


        foreach (var file in files)
        {
            var fileResult =
                CheckFile(
                    file,
                    replacements);


            result.FileResults.Add(
                fileResult);


            if (fileResult.IsValid)
            {
                result.ValidFiles++;
            }
            else
            {
                result.InvalidFiles++;
            }
        }


        return result;
    }


    private FileCheckResult CheckFile(
        string filePath,
        List<ReplacementItem> replacements)
    {
        var result =
            new FileCheckResult
            {
                FilePath = filePath,
                FileName =
                    Path.GetFileName(filePath),
                IsValid = true
            };


        // =============================================
        // Kiểm tra DOCX
        // =============================================

        if (!_reader.IsValidDocx(filePath))
        {
            result.IsValid = false;

            result.MissingValues.Add(
                "File DOCX không hợp lệ.");

            return result;
        }


        string text;


        try
        {
            text =
                _reader.ReadAllText(filePath);
        }
        catch
        {
            result.IsValid = false;

            result.MissingValues.Add(
                "Không thể đọc file.");

            return result;
        }


        // =============================================
        // Kiểm tra từng nội dung cần thay
        // =============================================

        foreach (var replacement in replacements)
        {
            if (string.IsNullOrEmpty(
                    replacement.OldValue))
            {
                continue;
            }


            if (!text.Contains(
                    replacement.OldValue,
                    StringComparison.Ordinal))
            {
                result.IsValid = false;

                result.MissingValues.Add(
                    replacement.OldValue);
            }
        }


        return result;
    }
}