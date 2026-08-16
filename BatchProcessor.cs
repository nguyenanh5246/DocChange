using DocChange.Models.Change;
using System.IO;

namespace DocChange.Services.Change;

public class BatchProcessor
{
    private readonly ReplacementEngine _replacementEngine;

    public BatchProcessor()
    {
        _replacementEngine =
            new ReplacementEngine();
    }


    public ProcessResult ProcessFiles(
        string inputFolder,
        string outputFolder,
        List<ReplacementItem> replacements,
        CheckResult checkResult,
        bool overwriteExisting)
    {
        if (!Directory.Exists(inputFolder))
        {
            throw new DirectoryNotFoundException(
                "Input folder không tồn tại.");
        }


        Directory.CreateDirectory(
            outputFolder);


        var result =
            new ProcessResult();


        foreach (var fileResult in
                 checkResult.FileResults)
        {
            // =====================================
            // CHỈ XỬ LÝ FILE PASS
            // =====================================

            if (!fileResult.IsValid)
            {
                continue;
            }


            result.TotalFiles++;


            string inputFile =
                Path.Combine(
                    inputFolder,
                    fileResult.FileName);


            if (!File.Exists(inputFile))
            {
                result.FailedFiles++;

                result.FailedFileNames.Add(
                    fileResult.FileName);

                continue;
            }


            string outputFile =
                Path.Combine(
                    outputFolder,
                    fileResult.FileName);


            // =====================================
            // FILE ĐÃ TỒN TẠI
            // =====================================

            if (File.Exists(outputFile) &&
                !overwriteExisting)
            {
                result.SkippedFiles++;

                result.SkippedFileNames.Add(
                    fileResult.FileName);

                continue;
            }


            try
            {
                // =================================
                // COPY
                // =================================

                File.Copy(
                    inputFile,
                    outputFile,
                    overwriteExisting);


                // =================================
                // REPLACE
                // =================================

                foreach (var replacement in
                         replacements)
                {
                    _replacementEngine
                        .ReplaceInDocument(
                            outputFile,
                            replacement.OldValue,
                            replacement.NewValue);
                }


                result.ProcessedFiles++;
            }
            catch
            {
                result.FailedFiles++;

                result.FailedFileNames.Add(
                    fileResult.FileName);
            }
        }


        return result;
    }
}