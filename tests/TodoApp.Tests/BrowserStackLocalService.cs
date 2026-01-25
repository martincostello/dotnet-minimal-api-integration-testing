// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace TodoApp;

internal sealed class BrowserStackLocalService(
    string accessKey,
    BrowserStackLocalOptions? options) : IDisposable
{
    private static readonly SemaphoreSlim _downloadLock = new(1, 1);
    private static string? _binaryPath;
    private bool _outputRedirected;
    private Process? _process;
    private bool _disposed;

    ~BrowserStackLocalService()
    {
        DisposeInternal();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var arguments = BrowserStackLocalOptions.BuildCommandLine(accessKey, options);

        // Ensure the binary for the BrowserStack Local proxy service is available
        if (_binaryPath is null)
        {
            // If tests are run in parallel, only download the file once
            await _downloadLock.WaitAsync(cancellationToken);

            try
            {
#pragma warning disable CA1508
                _binaryPath ??= await EnsureBinaryAsync(cancellationToken);
#pragma warning restore CA1508
            }
            finally
            {
                _downloadLock.Release();
            }
        }

        var startInfo = new ProcessStartInfo
        {
            CreateNoWindow = true,
            FileName = _binaryPath,
            RedirectStandardError = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        _process = Process.Start(startInfo);

        if (_process is null)
        {
            throw new InvalidOperationException("Failed to start BrowserStack Local service.");
        }

        try
        {
            var stdout = new StringBuilder();
            var tcs = new TaskCompletionSource();

            void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data is not null)
                {
                    stdout.AppendLine(e.Data);

                    if (e.Data.Contains("Press Ctrl-C to exit", StringComparison.OrdinalIgnoreCase))
                    {
                        tcs.SetResult();
                    }
                }
            }

            _process.OutputDataReceived += OnOutputDataReceived;
            _process.BeginOutputReadLine();

            _outputRedirected = true;

            if (_process.HasExited)
            {
                throw new InvalidOperationException(
                    $"Failed to start process. The process exited with code {_process.ExitCode}. This could be because {GetBinaryName()} requires updating; an updated version can be downloaded from {GetDownloadUri()}.");
            }

            // Give the BrowserStackLocal process time to initialize
            var timeout = TimeSpan.FromSeconds(15);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            try
            {
                await tcs.Task.WaitAsync(cts.Token);
            }
            catch (TaskCanceledException)
            {
                var exception = new InvalidOperationException(
                    $"Process failed to initialize within {timeout}. This could be because {GetBinaryName()} requires updating; an updated version can be downloaded from {GetDownloadUri()}. Alternatively, it could be caused by a firewall.");

                exception.Data["stdout"] = stdout.ToString();

                throw exception;
            }

            // Once started, we don't need to listen to stdout any more
            _process.OutputDataReceived -= OnOutputDataReceived;
            stdout.Clear();
        }
        catch (Exception)
        {
            if (_outputRedirected)
            {
                _process.CancelOutputRead();
            }

            if (!_process.HasExited)
            {
                try
                {
                    _process.Kill();
                }
                catch (InvalidOperationException)
                {
                }
            }

            _process.Dispose();
            _process = null;

            throw;
        }
    }

    public void Dispose()
    {
        DisposeInternal();
        GC.SuppressFinalize(this);
    }

    private static async Task<string> EnsureBinaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            string folderPath =
                OperatingSystem.IsWindows() ?
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) :
                Directory.CreateTempSubdirectory("_BrowserStackLocal").FullName;

            string localCachePath = Path.Combine(folderPath, "_BrowserStackLocal");
            string binaryPath = Path.Combine(localCachePath, GetBinaryName());

            string zippedBinaryPath = binaryPath + ".zip";
            string cachedETagFileName = zippedBinaryPath + ".ETag";

            Uri downloadUri = GetDownloadUri();

            using var client = new HttpClient();

            string currentETag;

            // Get the current ETag of the ZIP file containing the BrowserStack Local binary
            using (var request = new HttpRequestMessage(HttpMethod.Head, downloadUri))
            {
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();
                currentETag = response.Headers.ETag?.Tag ?? string.Empty;
            }

            // Is the local version of the tool up-to-date?
            bool needToDownload =
                !File.Exists(cachedETagFileName) ||
                !string.Equals(await File.ReadAllTextAsync(cachedETagFileName, Encoding.UTF8, cancellationToken), currentETag, StringComparison.Ordinal);

            if (needToDownload)
            {
                // Get the latest ZIP file and extract it
                using var source = await client.GetStreamAsync(downloadUri, cancellationToken);

                if (Directory.Exists(localCachePath))
                {
                    Directory.Delete(localCachePath, recursive: true);
                }

                Directory.CreateDirectory(localCachePath);

                await ZipFile.ExtractToDirectoryAsync(source, localCachePath, cancellationToken);
                await File.WriteAllTextAsync(cachedETagFileName, currentETag, Encoding.UTF8, cancellationToken);
            }

            return binaryPath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to download BrowserStack Local binary.", ex);
        }
    }

    private static string GetBinaryName()
        => OperatingSystem.IsWindows() ? "BrowserStackLocal.exe" : "BrowserStackLocal";

    private static Uri GetDownloadUri()
    {
        string fileName;

        if (OperatingSystem.IsWindows())
        {
            fileName = "BrowserStackLocal-win32.zip";
        }
        else if (OperatingSystem.IsMacOS())
        {
            fileName = "BrowserStackLocal-darwin-x64.zip";
        }
        else if (OperatingSystem.IsLinux())
        {
            fileName = Environment.Is64BitOperatingSystem ?
                "BrowserStackLocal-linux-x64.zip" :
                "BrowserStackLocal-linux-ia32.zip";
        }
        else
        {
            throw new PlatformNotSupportedException("The current platform is not supported.");
        }

        return new UriBuilder("https://www.browserstack.com")
        {
            Path = "browserstack-local/" + fileName
        }.Uri;
    }

    private void DisposeInternal()
    {
        if (!_disposed)
        {
            if (_process != null)
            {
                if (!_process.HasExited)
                {
                    try
                    {
                        if (_outputRedirected)
                        {
                            _process.CancelOutputRead();
                        }

                        try
                        {
                            _process.Kill();
                        }
                        catch (InvalidOperationException)
                        {
                        }

                        if (_process.WaitForExit(10_000))
                        {
                            // It seems to take a second or so to ensure the process
                            // is stopped so we can delete the extracted binary
                            Thread.Sleep(TimeSpan.FromSeconds(1));
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    catch (SystemException)
                    {
                    }
                }

                _process.Dispose();
            }

            _disposed = true;
        }
    }
}
