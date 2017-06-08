using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace Pokazowy
{
    public static class PDFParser
    {
        public static string PdfTextToString(this PdfReader reader)
        {
            string text = string.Empty;

            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                text += PdfTextExtractor.GetTextFromPage(reader, page);
            }

            reader.Close();
            return text;
        }
    }
}
