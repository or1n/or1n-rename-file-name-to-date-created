using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Xmp;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Globalization;

namespace Or1nRenameFileNameToDateCreated.Helpers;

/// <summary>
/// Provides fast, structured metadata scanning for files in a folder.
/// </summary>
public static class MetadataScanService
{
    /// <summary>
    /// Represents a scanned file and the metadata used to compute its chosen date.
    /// </summary>
    public sealed record FileMetadataResult(
        string FileName,
        string Extension,
        long SizeBytes,
        string SizeDisplay,
        FileCategory Category,
        DateTime SelectedDate,
        string SelectedDateSource,
        string DateParts,
        DateTime CreatedDate,
        DateTime ModifiedDate,
        DateTime? TakenDate,
        DateTime? MediaTaggedDate
    );

    /// <summary>
    /// Summary statistics for a scan.
    /// </summary>
    public sealed record ScanSummary(
        int TotalFiles,
        IReadOnlyDictionary<FileCategory, int> CategoryCounts,
        IReadOnlyDictionary<string, int> DateSourceCounts,
        IReadOnlyDictionary<string, int> ExtensionCounts
    );

    /// <summary>
    /// Result wrapper for a scan.
    /// </summary>
    public sealed record ScanResult(
        IReadOnlyList<FileMetadataResult> Files,
        ScanSummary Summary
    );

    /// <summary>
    /// Progress payload for metadata scans.
    /// </summary>
    public sealed record ScanProgress(
        int Index,
        int Total,
        string FileName,
        string Extension,
        double Percent,
        TimeSpan Elapsed,
        TimeSpan EstimatedRemaining
    );

    /// <summary>
    /// Supported high-level file categories.
    /// </summary>
    public enum FileCategory
    {
        Image,
        Video,
        Audio,
        Document,
        Archive,
        Executable,
        Binary,
        Other
    }

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".heic", ".heif", ".tif", ".tiff", ".bmp", ".gif", ".webp", ".raw", ".arw", ".cr2", ".nef"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mov", ".mkv", ".avi", ".wmv", ".m4v", ".webm", ".3gp"
    };

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".flac", ".m4a", ".aac", ".ogg", ".wma", ".opus"
    };

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".rtf", ".md", ".csv", ".json", ".xml"
    };

    private static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".7z", ".rar", ".tar", ".gz", ".bz2"
    };

    private static readonly HashSet<string> ExecutableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".msi", ".bat", ".cmd", ".com", ".ps1"
    };

    private static readonly HashSet<string> BinaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll", ".sys", ".bin", ".dat"
    };

    /// <summary>
    /// Scans a folder and returns structured metadata results with summary statistics.
    /// </summary>
    /// <param name="folderPath">Folder path to scan.</param>
    /// <param name="extensionFilter">Optional set of extensions to include (e.g., ".jpg", ".mp4").</param>
    /// <returns>A full scan result containing individual file data and summary counts.</returns>
    public static ScanResult ScanFolder(string folderPath, IReadOnlySet<string>? extensionFilter = null, Action<ScanProgress>? progressReporter = null)
    {
        var normalizedFilter = extensionFilter != null && extensionFilter.Count > 0
            ? new HashSet<string>(extensionFilter, StringComparer.OrdinalIgnoreCase)
            : null;

        var files = System.IO.Directory.EnumerateFiles(folderPath, "*", SearchOption.TopDirectoryOnly)
            .Where(path => normalizedFilter == null || normalizedFilter.Contains(Path.GetExtension(path)))
            .ToList();
        var results = new List<FileMetadataResult>(files.Count);
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < files.Count; i++)
        {
            var filePath = files[i];
            try
            {
                var info = new FileInfo(filePath);
                var extension = info.Extension.ToLowerInvariant();
                var category = GetCategory(extension);
                var created = info.CreationTime;
                var modified = info.LastWriteTime;

                DateTime? takenDate = null;
                DateTime? mediaTaggedDate = null;

                if (category == FileCategory.Image)
                {
                    takenDate = TryGetImageTakenDate(filePath);
                }
                else if (category == FileCategory.Video || category == FileCategory.Audio)
                {
                    mediaTaggedDate = TryGetMediaTaggedDate(filePath);
                }

                var (selectedDate, selectedSource) = ResolveBestDate(created, modified, takenDate, mediaTaggedDate);
                var dateParts = FormatDateParts(selectedDate);

                results.Add(new FileMetadataResult(
                    Path.GetFileName(filePath),
                    extension,
                    info.Length,
                    FormatFileSize(info.Length),
                    category,
                    selectedDate,
                    selectedSource,
                    dateParts,
                    created,
                    modified,
                    takenDate,
                    mediaTaggedDate
                ));
            }
            catch
            {
                // Ignore individual file errors to keep scan resilient
            }

            if (progressReporter != null)
            {
                var processed = i + 1;
                var percent = files.Count == 0 ? 100 : (processed * 100d / files.Count);
                var elapsed = stopwatch.Elapsed;
                var averageMs = processed > 0 ? elapsed.TotalMilliseconds / processed : 0;
                var remainingMs = Math.Max(0, (files.Count - processed) * averageMs);
                var eta = TimeSpan.FromMilliseconds(remainingMs);

                progressReporter(new ScanProgress(
                    processed,
                    files.Count,
                    Path.GetFileName(filePath),
                    Path.GetExtension(filePath),
                    percent,
                    elapsed,
                    eta
                ));
            }
        }

        var ordered = results
            .OrderBy(r => r.Category)
            .ThenBy(r => r.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var summary = BuildSummary(ordered);
        return new ScanResult(ordered, summary);
    }

    private static ScanSummary BuildSummary(IReadOnlyList<FileMetadataResult> results)
    {
        var categoryCounts = results
            .GroupBy(r => r.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        var dateSourceCounts = results
            .GroupBy(r => r.SelectedDateSource)
            .ToDictionary(g => g.Key, g => g.Count());

        var extensionCounts = results
            .GroupBy(r => r.Extension)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        return new ScanSummary(results.Count, categoryCounts, dateSourceCounts, extensionCounts);
    }

    private static FileCategory GetCategory(string extension)
    {
        if (ImageExtensions.Contains(extension)) return FileCategory.Image;
        if (VideoExtensions.Contains(extension)) return FileCategory.Video;
        if (AudioExtensions.Contains(extension)) return FileCategory.Audio;
        if (DocumentExtensions.Contains(extension)) return FileCategory.Document;
        if (ArchiveExtensions.Contains(extension)) return FileCategory.Archive;
        if (ExecutableExtensions.Contains(extension)) return FileCategory.Executable;
        if (BinaryExtensions.Contains(extension)) return FileCategory.Binary;
        return FileCategory.Other;
    }

    private static (DateTime selected, string source) ResolveBestDate(
        DateTime created,
        DateTime modified,
        DateTime? taken,
        DateTime? mediaTagged)
    {
        if (taken.HasValue)
        {
            return (taken.Value, "DateTaken");
        }

        if (mediaTagged.HasValue)
        {
            return (mediaTagged.Value, "MediaTaggedDate");
        }

        return (created, "DateCreated");
    }

    private static DateTime? TryGetImageTakenDate(string filePath)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);
            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (subIfd == null) return null;

            if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTimeOriginal))
            {
                return dateTimeOriginal;
            }

            if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var dateTimeDigitized))
            {
                return dateTimeDigitized;
            }

            var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (ifd0 != null && ifd0.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var dateTimeIfd0))
            {
                return dateTimeIfd0;
            }

            var xmpDirectory = directories.OfType<XmpDirectory>().FirstOrDefault();
            if (xmpDirectory != null)
            {
                var xmpDate = TryGetXmpDate(xmpDirectory);
                if (xmpDate.HasValue)
                {
                    return xmpDate.Value;
                }
            }

        }
        catch
        {
            // Ignore EXIF parsing failures
        }

        return null;
    }

    private static DateTime? TryGetXmpDate(XmpDirectory xmpDirectory)
    {
        try
        {
            var xmpProperties = xmpDirectory?.GetXmpProperties();
            if (xmpProperties == null) return null;

            string? value = null;
            if (xmpProperties.TryGetValue("xmp:CreateDate", out var createDate))
            {
                value = createDate;
            }
            else if (xmpProperties.TryGetValue("xmp:DateTimeOriginal", out var originalDate))
            {
                value = originalDate;
            }
            else if (xmpProperties.TryGetValue("photoshop:DateCreated", out var psDate))
            {
                value = psDate;
            }

            if (!string.IsNullOrWhiteSpace(value) && DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsed))
            {
                return parsed.ToLocalTime();
            }
        }
        catch
        {
            // Ignore XMP parsing failures
        }

        return null;
    }

    private static DateTime? TryGetMediaTaggedDate(string filePath)
    {
        try
        {
            var mediaFile = TagLib.File.Create(filePath);
            if (mediaFile.Tag != null && mediaFile.Tag.Year > 0)
            {
                return new DateTime((int)mediaFile.Tag.Year, 1, 1);
            }
        }
        catch
        {
            // Ignore tag parsing failures
        }

        return null;
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", len, sizes[order]);
    }

    private static string FormatDateParts(DateTime date)
    {
        return $"{date:yyyy-MM-dd HH:mm:ss.fff}";
    }
}
