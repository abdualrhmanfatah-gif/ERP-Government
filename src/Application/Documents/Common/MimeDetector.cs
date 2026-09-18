namespace ERP_Government.Application.Documents.Common;

public static class MimeDetector
{
    private static readonly Dictionary<string, string> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // PDF
        [".pdf"] = "application/pdf",
        // Word
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        // Excel
        [".xls"] = "application/vnd.ms-excel",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".csv"] = "text/csv",
        // Images
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".bmp"] = "image/bmp",
        [".webp"] = "image/webp",
        [".svg"] = "image/svg+xml",
        // Text
        [".txt"] = "text/plain",
        [".html"] = "text/html",
        [".htm"] = "text/html",
        [".css"] = "text/css",
        [".js"] = "text/javascript",
        [".json"] = "application/json",
        [".xml"] = "application/xml",
        [".md"] = "text/plain",
        // Other
        [".zip"] = "application/zip",
        [".rar"] = "application/x-rar-compressed",
        [".7z"] = "application/x-7z-compressed",
    };

    public static string Detect(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return ExtensionMap.TryGetValue(ext, out var mime) ? mime : "application/octet-stream";
    }
}
