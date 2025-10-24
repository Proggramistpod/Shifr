using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Shifr.WorkFile
{
    internal static class WordHelper
    {
        public static string ReadWordText(string path)
        {
            using (var doc = WordprocessingDocument.Open(path, false))
            {
                return doc.MainDocumentPart.Document.Body.InnerText;
            }
        }

        public static void WriteWordText(string path, string newText)
        {
            using (var doc = WordprocessingDocument.Open(path, true))
            {
                var body = doc.MainDocumentPart.Document.Body;
                body.RemoveAllChildren();
                body.Append(new Paragraph(new Run(new Text(newText))));
                doc.MainDocumentPart.Document.Save();
            }
        }
    }
}
