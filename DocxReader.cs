using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocChange.Services.Change;

public class DocxReader
{
    public bool IsValidDocx(string filePath)
    {
        try
        {
            using var document = WordprocessingDocument.Open(filePath, false);

            return document.MainDocumentPart?.Document?.Body != null;
        }
        catch
        {
            return false;
        }
    }


    public string ReadAllText(string filePath)
    {
        using var document = WordprocessingDocument.Open(filePath, false);

        var result = new List<string>();

        // BODY
        var body = document.MainDocumentPart?.Document?.Body;

        if (body != null)
        {
            result.Add("=== BODY ===");
            result.Add(body.InnerText);
        }

        // HEADER
        if (document.MainDocumentPart != null)
        {
            foreach (var headerPart in document.MainDocumentPart.HeaderParts)
            {
                if (headerPart.Header != null)
                {
                    result.Add("=== HEADER ===");
                    result.Add(headerPart.Header.InnerText);
                }
            }
        }

        // FOOTER
        if (document.MainDocumentPart != null)
        {
            foreach (var footerPart in document.MainDocumentPart.FooterParts)
            {
                if (footerPart.Footer != null)
                {
                    result.Add("=== FOOTER ===");
                    result.Add(footerPart.Footer.InnerText);
                }
            }
        }

        return string.Join(
            Environment.NewLine,
            result);
    }
}