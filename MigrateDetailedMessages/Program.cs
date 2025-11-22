using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Compression;
using System.Text;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
    })
    .Build()
    .RunAsync();

class Worker
    : BackgroundService
{
    private readonly string _connectionString;
    private readonly string _jobResultMessagesPath;
    public Worker(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Sched")!;
        // Use JobResultMessagesPath if configured, otherwise fall back to WorkerPath/__messages__ for backward compatibility
        string? jobResultMessagesPath = config["JobResultMessagesPath"];
        string? workerPath = config["WorkerPath"];
        if (!string.IsNullOrEmpty(jobResultMessagesPath))
        {
            _jobResultMessagesPath = jobResultMessagesPath;
        }
        else if (!string.IsNullOrEmpty(workerPath))
        {
            _jobResultMessagesPath = Path.Combine(workerPath, "__messages__");
        }
        else
        {
            throw new InvalidOperationException("Either JobResultMessagesPath or WorkerPath must be configured.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using SqlConnection conn = new(_connectionString);
        await conn.OpenAsync(stoppingToken);
        using SqlCommand comm = conn.CreateCommand();
        comm.CommandText = @"SELECT [JobID], [DetailedMessage] FROM [dbo].[Jobs] WHERE NULLIF(LTRIM(RTRIM([DetailedMessage])), '') IS NOT NULL;";
        using SqlDataReader rdr = await comm.ExecuteReaderAsync(stoppingToken);
        while (await rdr.ReadAsync(stoppingToken))
        {
            long jobID = rdr.GetInt64(0);
            string detailedMessage = rdr.GetString(1);
            DirectoryInfo messageDir = new(_jobResultMessagesPath);
            messageDir.Create();
            messageDir.Refresh();
            FileInfo messageGZipFile = new(Path.Combine(messageDir.FullName, $"{jobID}.txt.gz"));
            GZipTextFile(messageGZipFile, detailedMessage.Trim());
        }
    }

    private static void GZipTextFile(FileInfo messageGZipFile, string contents)
    {
        using MemoryStream inputStream = new(Encoding.UTF8.GetBytes(contents));
        using FileStream fileStream = messageGZipFile.OpenWrite();
        GZip.Compress(inputStream, fileStream);
    }
}

public static class GZip
{
    private const int BUFFER_SIZE = 0x4000;

    public static void Compress(Stream inputStream, Stream outputStream)
    {
        byte[] buffer = new byte[BUFFER_SIZE];
        using GZipStream gzip = new(outputStream, CompressionMode.Compress);
        int count;
        while ((count = inputStream.Read(buffer, 0, BUFFER_SIZE)) > 0)
        {
            gzip.Write(buffer, 0, count);
        }
    }
}
