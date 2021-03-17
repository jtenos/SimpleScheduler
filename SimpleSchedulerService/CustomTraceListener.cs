using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SimpleSchedulerService
{
    public class CustomTraceListener
        : TraceListener
    {
        private readonly string _fullFileName;
        private readonly int _maxFileSize;
        private FileStream? _fileStream;
        private int _numBytes;
        private string? _currentLogFileFullName;

        public CustomTraceListener(string fullFileName, int maxFileSize = 10 * 1024 * 1024)
        {
            var fileInfo = new FileInfo(fullFileName);
            if (!fileInfo.Directory!.Exists)
            {
                throw new ApplicationException("Invalid directory for trace file");
            }
            _fullFileName = fullFileName;
            _maxFileSize = maxFileSize;
            InitializeFile();
        }

        private void InitializeFile()
        {
            if (_fileStream != null)
            {
                _fileStream.Dispose();
                _fileStream = null;

                using (var inputStream = File.OpenRead(_currentLogFileFullName!))
                {
                    using var outputFileStream = File.OpenWrite($"{_currentLogFileFullName}.gz");
                    using var gzipStream = new GZipStream(outputFileStream, CompressionLevel.Optimal);
                    byte[] buffer = new byte[1024 * 32];
                    int count;
                    while ((count = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        gzipStream.Write(buffer, 0, count);
                    }
                }
                File.Delete(_currentLogFileFullName!);
            }

            var fileInfo = new FileInfo(_fullFileName);
            _currentLogFileFullName = Path.Combine(fileInfo.DirectoryName!, $"{Path.GetFileNameWithoutExtension(_fullFileName)}_{DateTime.Now:yyyyMMddHHmmssfff}{fileInfo.Extension}");
            _fileStream = new FileStream(_currentLogFileFullName, FileMode.Create, FileAccess.Write, FileShare.Read);
            _numBytes = 0;
        }

        public override void Write(string? message)
        {
            if (message == null) { return; }
            RefreshFile();
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            _numBytes += bytes.Length;
            _fileStream!.Write(bytes, 0, bytes.Length);
            _fileStream.Flush();
        }

        public override void WriteLine(string? message)
        {
            if (message == null) { return; }
            RefreshFile();
            byte[] bytes = Encoding.UTF8.GetBytes($"{message}{Environment.NewLine}");
            _numBytes += bytes.Length;
            _fileStream!.Write(bytes, 0, bytes.Length);
            _fileStream.Flush();
        }

        private void RefreshFile()
        {
            lock (this)
            {
                if (_numBytes > _maxFileSize) { InitializeFile(); }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_fileStream != null) { _fileStream.Dispose(); _fileStream = null; }
        }
    }
}
