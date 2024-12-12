using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Serilog;

namespace MYMC.AutoUpdate;

public sealed class UpdateEngine
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger _logger;
    
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public UpdateEngine(IHttpClientFactory clientFactory, ILogger logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
        
        var assembly = Assembly.GetExecutingAssembly();
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        _currentVersion = Version.Parse(fileVersionInfo.FileVersion ?? "0.0.0.0");
        
        _logger.Information("Current version: {CurrentVersion}", _currentVersion);
    }
    
    private const string UpdateUrl = "https://software.mustafacanyucel.com/api/update";
    private const string BaseUrl = "https://software.mustafacanyucel.com";
    private const int BufferSize = 8192;
    private readonly Version _currentVersion;
    private UpdateInfo? _latestUpdateInfo;
    
    public event EventHandler<UpdateProgressEventArgs>? UpdateProgress;
    
    public async Task<bool> CheckForUpdates()
    {
        try
        {
            using var client = _clientFactory.CreateClient(App.UpdateHttpClientName);

            var stateSeed = GenerateSecureRandomNumber();
            var appIdentity = GetApplicationIdentity();
            
            var updateQueryRequest = new UpdateQueryRequest(appIdentity, stateSeed);
            var request = new HttpRequestMessage(HttpMethod.Post, UpdateUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(updateQueryRequest), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var updateQueryResponse = JsonSerializer.Deserialize<UpdateQueryResponse>(responseContent, JsonSerializerOptions);

            if (updateQueryResponse == null)
            {
                _logger.Warning("Failed to deserialize update query response");
                return false;
            }

            var actualState = TransformState(stateSeed);
            if (updateQueryResponse.State != actualState)
            {
                _logger.Warning("State mismatch in update query response");
                return false;
            }

            if (!TryParseVersion(updateQueryResponse.Version, out var latestVersion))
            {
                _logger.Warning("Failed to parse latest version: {LatestVersion}", updateQueryResponse.Version);
                return false;
            }

            if (latestVersion <= _currentVersion) return false;
            // Ensure download URL is absolute
            var downloadUrl = updateQueryResponse.DownloadUrl;
            if (downloadUrl.StartsWith("/"))
            {
                downloadUrl = BaseUrl + downloadUrl;
            }
                
            _latestUpdateInfo = new UpdateInfo
            {
                Version = latestVersion ?? throw new InvalidOperationException("Latest version is null"),
                DownloadUrl = downloadUrl,
                ReleaseNotes = updateQueryResponse.ReleaseNotes,
                Sha256Hash = updateQueryResponse.Sha256Hash
            };
            return true;

        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to check for updates");
            return false;
        }
    }
    
    public async Task DownloadAndInstallUpdate()
    {
        if (_latestUpdateInfo == null)
        {
            _logger.Warning("No update available to download and install");
            return;
        }

        try
        {
            using var client = _clientFactory.CreateClient(App.UpdateHttpClientName);
            using var response = await client.GetAsync(_latestUpdateInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var downloadedBytes = 0L;
            var buffer = new byte[BufferSize];
            
            var appName = Assembly.GetExecutingAssembly().GetName().Name ?? "App";
            var cleanAppName = string.Join("", appName.Split(Path.GetInvalidFileNameChars()));
            var tempFilePath = Path.Combine(
                Path.GetTempPath(), 
                $"{cleanAppName}_Setup_{_latestUpdateInfo.Version}.exe"
            );

            _logger.Information("Starting download to: {TempFile}", tempFilePath);

            // Make sure we dispose of all streams properly
            await using (var contentStream = await response.Content.ReadAsStreamAsync())
            await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                while (true)
                {
                    var bytesRead = await contentStream.ReadAsync(buffer);
                    if (bytesRead == 0) break;

                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    await fileStream.FlushAsync(); // Force write to disk
                    downloadedBytes += bytesRead;

                    if (totalBytes <= 0) continue;
                    var progress = (double)downloadedBytes / totalBytes * 100;
                    UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(
                        progress,
                        $"Downloading update ({downloadedBytes / 1024.0 / 1024.0:F1}MB / {totalBytes / 1024.0 / 1024.0:F1}MB)"));
                }
            } // This ensures streams are properly closed before we verify the file

            // Log file size after download
            var fileInfo = new FileInfo(tempFilePath);
            _logger.Information("Download complete. File size: {FileSize} bytes", fileInfo.Length);

            // Verify hash
            UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(100, "Verifying download..."));
            var fileHash = await CalculateFileHash(tempFilePath);

            _logger.Information("File hash: {FileHash}", fileHash);
            _logger.Information("Expected hash: {ExpectedHash}", _latestUpdateInfo.Sha256Hash);

            if (fileHash != _latestUpdateInfo.Sha256Hash)
            {
                File.Delete(tempFilePath);
                throw new Exception("Downloaded file hash does not match expected hash");
            }

            // Prepare for installation
            UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(100, "Preparing to install update..."));
            await InstallUpdate(tempFilePath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to download and install update");
            throw;
        }
    }
    
    private static bool TryParseVersion(string version, out Version? parsedVersion)
    {
        parsedVersion = null;
        try
        {
            parsedVersion = new Version(version);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    private static async Task<string> CalculateFileHash(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        var hashBytes = await SHA256.HashDataAsync(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
    
    private static async Task InstallUpdate(string updateFilePath)
    {
        var batchFilePath = Path.Combine(Path.GetTempPath(), "update.bat");

        var batchContents = $"""

                             @echo off
                             timeout /t 1 /nobreak > nul
                             start "" "{updateFilePath}"
                             del "%~f0"

                             """;

        await File.WriteAllTextAsync(batchFilePath, batchContents);

        // Start the batch file and exit the current process
        var startInfo = new ProcessStartInfo
        {
            FileName = batchFilePath,
            WindowStyle = ProcessWindowStyle.Hidden
        };
        Process.Start(startInfo);
        Environment.Exit(0);
    }
    
    private static string GetApplicationIdentity()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName();
        
        return new
        {
            Name = assemblyName.Name ?? "Unknown",
            Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ?? "0.0.0.0",
            Architecture = RuntimeInformation.ProcessArchitecture.ToString()
        }.ToString() ?? "Unknown Application";
    }
    
    private static string TransformState(int seed)
    {
        // Get bytes and ensure LittleEndian order
        var seedBytes = BitConverter.GetBytes(seed);
        if (BitConverter.IsLittleEndian == false)
        {
            Array.Reverse(seedBytes);
        }

        var hashBytes = SHA256.HashData(seedBytes);
        return Convert.ToBase64String(hashBytes.Take(8).ToArray());
    }
    
    private static int GenerateSecureRandomNumber()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        // Use only the positive range of integers
        return Math.Abs(BitConverter.ToInt32(bytes, 0));
    }
}