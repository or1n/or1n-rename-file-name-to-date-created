// Global usings for common WinUI 3 and .NET types
global using Or1nRenameFileNameToDateCreated.Views;

global using Microsoft.UI.Xaml;
global using Microsoft.UI.Xaml.Controls;

// File & Folder Operations
global using System.IO;
global using Windows.Storage;
global using Windows.Storage.Pickers;

// These will be needed for Phase 3+ (Batch Rename Engine):
// - File metadata extraction (images, videos, all file types)
// - EXIF data reading for photos
// - Date formats and calculations

/*
 * TODO: Add the following NuGet packages for file metadata extraction in future versions:
 * 
 * For Image EXIF Data:
 * - MetadataExtractor (supports JPEG, TIFF, images)
 * - TagLibSharp (audio files)
 * 
 * For Video Metadata:
 * - TagLibSharp (supports MP4, MKV, WebM, WMA)
 * - MediaInfo (comprehensive media file support)
 * 
 * For All File Types:
 * - ShellFile (native Windows shell, built-in)
 * - PropertyStore (native Windows shell, built-in)
 * 
 * Example usage will be implemented in Phase 5 (v3.0):
 * - FileMetadataService class for extracting dates
 * - Support for date created, date modified, date taken (EXIF)
 * - Configurable date priority (which date to use if multiple available)
 * 
 * Installation:
 * dotnet add package MetadataExtractor
 * dotnet add package TagLibSharp
 * 
 * Note: Shell APIs are built-in to Windows and don't require NuGet packages
 */
