using System.IO;
using OrchardCore.Media.Indexing;

namespace OrchardCore.Shortcodes
{
    public class PdfMediaFileTextProvider2 : IMediaFileTextProvider
    {
        public bool CanHandle(string path)
        {
            return path.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase);
        }

        public string GetText(string path, Stream fileStream)
        {
            return "";
        }
    }
}
