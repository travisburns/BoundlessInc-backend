using BoundlessEnterprises.Domain.Work;

namespace BoundlessEnterprises.Application.Work;

/// <summary>Classifies an uploaded file by its extension.</summary>
public static class RingFileKinds
{
    public static RingFileKind FromFileName(string fileName)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        return ext switch
        {
            "wav" or "mp3" or "aif" or "aiff" or "flac" or "ogg" or "m4a" => RingFileKind.Audio,
            "png" or "jpg" or "jpeg" or "gif" or "webp" or "svg" or "psd" or "ai" or "tif" or "tiff" => RingFileKind.Image,
            "mp4" or "mov" or "webm" or "avi" or "mkv" => RingFileKind.Video,
            "pdf" or "doc" or "docx" or "txt" or "md" or "rtf" or "xls" or "xlsx" or "ppt" or "pptx" => RingFileKind.Document,
            "zip" or "rar" or "7z" or "tar" or "gz" => RingFileKind.Archive,
            "logicx" or "als" or "flp" or "ptx" or "cpr" or "blend" or "unity" or "fig" or "sketch" => RingFileKind.Project,
            _ => RingFileKind.Other,
        };
    }
}
