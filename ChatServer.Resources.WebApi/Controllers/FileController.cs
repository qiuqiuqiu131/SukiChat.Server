using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace ChatServer.Resources.WebApi.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public FilesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetBaseFolder()
        {
            var folder = _configuration.GetValue<string>("BaseFolder");
            if (string.IsNullOrWhiteSpace(folder))
                throw new InvalidOperationException("BaseFolder not configured");
            return folder;
        }

        private bool IsSafePath(string baseFolder, string fullPath)
        {
            var baseFull = Path.GetFullPath(baseFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var fileFull = Path.GetFullPath(fullPath);
            return fileFull.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase);
        }

        // Download with Range support (resume/pause). Uses PhysicalFile with enableRangeProcessing=true.
        [HttpGet("{*filePath}")]
        public IActionResult DownloadFile(string filePath)
        {
            Console.WriteLine($"{filePath}");

            var folder = GetBaseFolder();

            var combined = Path.Combine(folder, filePath ?? string.Empty);
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(combined);
            }
            catch
            {
                return BadRequest("Invalid file path.");
            }

            if (!IsSafePath(folder, fullPath))
                return BadRequest("Invalid file path.");

            if (!System.IO.File.Exists(fullPath))
                return NotFound($"File not found: {filePath}");

            var contentType = "application/octet-stream";
            // PhysicalFile with enableRangeProcessing true -> supports HTTP Range requests returning 206 Partial Content.
            return PhysicalFile(fullPath, contentType, Path.GetFileName(fullPath), enableRangeProcessing: true);
        }

        // Streamed upload (full overwrite) or partial write when client sends Content-Range header.
        // RestFileIOHelper uses PUT and sets Content-Length and writes raw bytes, so this matches.
        [HttpPut("{*filePath}")]
        public async Task<IActionResult> UploadFile(
            string filePath,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest("filePath required.");

            var folder = GetBaseFolder();

            var combined = Path.Combine(folder, filePath);
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(combined);
            }
            catch
            {
                return BadRequest("Invalid file path.");
            }

            if (!IsSafePath(folder, fullPath))
                return BadRequest("Invalid file path.");

            var destDir = Path.GetDirectoryName(fullPath) ?? folder;
            Directory.CreateDirectory(destDir);

            // If client sent Content-Range -> treat as partial upload (append/seek)
            // Content-Range: bytes <start>-<end>/<total>
            if (Request.Headers.TryGetValue("Content-Range", out var contentRangeValues))
            {
                var cr = contentRangeValues.ToString();
                // parse Content-Range
                var m = Regex.Match(cr, @"bytes\s+(\d+)-(\d+)/(\d+|\*)");
                if (!m.Success)
                    return BadRequest("Invalid Content-Range header.");

                if (!long.TryParse(m.Groups[1].Value, out var start))
                    return BadRequest("Invalid Content-Range start.");

                // We will open file and write at offset `start`
                try
                {
                    // Open or create and seek to start
                    using var fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                    fs.Seek(start, SeekOrigin.Begin);
                    // Copy request body to file (this will write the chunk)
                    await Request.Body.CopyToAsync(fs, 81920, cancellationToken);
                    await fs.FlushAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return BadRequest("Upload canceled.");
                }
                catch (Exception ex)
                {
                    // log ex if you have logging
                    return StatusCode(500, "Failed to write chunk: " + ex.Message);
                }

                return Ok(new { ok = true });
            }
            else
            {
                // No Content-Range -> full overwrite. Stream directly to disk.
                try
                {
                    // Create or overwrite
                    using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                    await Request.Body.CopyToAsync(fs, 81920, cancellationToken);
                    await fs.FlushAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return BadRequest("Upload canceled.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Failed to save file: " + ex.Message);
                }

                // Created with location header
                var relativeUrl = $"api/files/{filePath.Replace('\\', '/')}";
                return Created(relativeUrl, new { path = filePath });
            }
        }

        // Optional form upload (multipart) for browsers/tools that send files via form POST
        // Example: curl -F "file=@/path/to/file" "http://host/api/files?path=some/subdir"
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadForm([FromForm] UploadFormRequest request, CancellationToken cancellationToken = default)
        {
            if (request.File == null)
                return BadRequest("No file provided.");

            var folder = GetBaseFolder();
            var targetPath = string.IsNullOrWhiteSpace(request.Path) ? request.File.FileName : Path.Combine(request.Path, request.File.FileName);
            var combined = Path.Combine(folder, targetPath);
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(combined);
            }
            catch
            {
                return BadRequest("Invalid file path.");
            }

            if (!IsSafePath(folder, fullPath))
                return BadRequest("Invalid file path.");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? folder);

            using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await request.File.OpenReadStream().CopyToAsync(stream, 81920, cancellationToken);
            }

            return Created($"api/files/{targetPath.Replace('\\', '/')}", new { path = targetPath });
        }
    }
}

public class UploadFormRequest
{
    public IFormFile File { get; set; } = default!;
    public string? Path { get; set; }
}