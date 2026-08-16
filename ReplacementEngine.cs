using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DocChange.Services.Change;

public class ReplacementEngine
{
    public int ReplaceInDocument(
        string filePath,
        string oldValue,
        string newValue)
    {
        int replacementCount = 0;

        using var document =
            WordprocessingDocument.Open(
                filePath,
                true);


        // =====================================================
        // BODY
        // =====================================================

        var body =
            document.MainDocumentPart?.Document?.Body;

        if (body != null)
        {
            foreach (var paragraph in
                     body.Descendants<Paragraph>())
            {
                replacementCount +=
                    ReplaceInParagraph(
                        paragraph,
                        oldValue,
                        newValue);
            }
        }


        // =====================================================
        // HEADER
        // =====================================================

        if (document.MainDocumentPart != null)
        {
            foreach (var headerPart in
                     document.MainDocumentPart.HeaderParts)
            {
                if (headerPart.Header == null)
                {
                    continue;
                }

                foreach (var paragraph in
                         headerPart.Header
                             .Descendants<Paragraph>())
                {
                    replacementCount +=
                        ReplaceInParagraph(
                            paragraph,
                            oldValue,
                            newValue);
                }
            }
        }


        // =====================================================
        // FOOTER
        // =====================================================

        if (document.MainDocumentPart != null)
        {
            foreach (var footerPart in
                     document.MainDocumentPart.FooterParts)
            {
                if (footerPart.Footer == null)
                {
                    continue;
                }

                foreach (var paragraph in
                         footerPart.Footer
                             .Descendants<Paragraph>())
                {
                    replacementCount +=
                        ReplaceInParagraph(
                            paragraph,
                            oldValue,
                            newValue);
                }
            }
        }


        // =====================================================
        // SAVE
        // =====================================================

        document.MainDocumentPart?
            .Document?
            .Save();

        return replacementCount;
    }


    private int ReplaceInParagraph(
        Paragraph paragraph,
        string oldValue,
        string newValue)
    {
        var textElements =
            paragraph
                .Descendants<Text>()
                .ToList();

        if (textElements.Count == 0)
        {
            return 0;
        }

        int replacementCount = 0;


        while (true)
        {
            bool replaced = false;


            for (int start = 0;
                 start < textElements.Count;
                 start++)
            {
                string combinedText =
                    string.Empty;

                int end = start;


                for (; end < textElements.Count; end++)
                {
                    combinedText +=
                        textElements[end].Text;

                    if (combinedText.Contains(oldValue))
                    {
                        break;
                    }
                }


                if (!combinedText.Contains(oldValue))
                {
                    continue;
                }


                int index =
                    combinedText.IndexOf(
                        oldValue,
                        StringComparison.Ordinal);


                int oldValueEnd =
                    index + oldValue.Length;


                // =============================================
                // Tìm Text bắt đầu
                // =============================================

                int currentPosition = 0;

                int firstTextIndex = -1;
                int lastTextIndex = -1;

                int startOffset = 0;
                int endOffset = 0;


                for (int i = start;
                     i <= end;
                     i++)
                {
                    int textStart =
                        currentPosition;

                    int textEnd =
                        currentPosition +
                        textElements[i].Text.Length;


                    if (firstTextIndex == -1 &&
                        index >= textStart &&
                        index < textEnd)
                    {
                        firstTextIndex = i;

                        startOffset =
                            index - textStart;
                    }


                    if (oldValueEnd > textStart &&
                        oldValueEnd <= textEnd)
                    {
                        lastTextIndex = i;

                        endOffset =
                            oldValueEnd - textStart;

                        break;
                    }


                    currentPosition =
                        textEnd;
                }


                if (firstTextIndex == -1 ||
                    lastTextIndex == -1)
                {
                    continue;
                }


                // =============================================
                // Cùng một Text
                // =============================================

                if (firstTextIndex ==
                    lastTextIndex)
                {
                    string text =
                        textElements[firstTextIndex].Text;


                    textElements[firstTextIndex].Text =
                        text.Remove(
                            startOffset,
                            oldValue.Length)
                        .Insert(
                            startOffset,
                            newValue);


                    replacementCount++;

                    replaced = true;

                    break;
                }


                // =============================================
                // Nhiều Text / nhiều Run
                // =============================================

                string firstText =
                    textElements[firstTextIndex].Text;

                string lastText =
                    textElements[lastTextIndex].Text;


                string before =
                    firstText[..startOffset];

                string after =
                    lastText[endOffset..];


                textElements[firstTextIndex].Text =
                    before +
                    newValue +
                    after;


                for (int i =
                         firstTextIndex + 1;
                     i <= lastTextIndex;
                     i++)
                {
                    textElements[i].Text =
                        string.Empty;
                }


                replacementCount++;

                replaced = true;

                break;
            }


            if (!replaced)
            {
                break;
            }
        }


        return replacementCount;
    }
}