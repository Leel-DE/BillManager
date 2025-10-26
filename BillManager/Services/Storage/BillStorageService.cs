using System;
using System.IO;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BillManager.Models;

namespace BillManager.Services.Storage;

public sealed class BillStorageService : IBillStorageService
{
    private const string LastBillFileName = "lastBill.json";

    private readonly string billsDirectory;
    private readonly string metadataPath;
    private readonly JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public BillStorageService()
    {
        var rootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BillManager");
        billsDirectory = Path.Combine(rootDirectory, "Bills");
        metadataPath = Path.Combine(rootDirectory, LastBillFileName);
        Directory.CreateDirectory(billsDirectory);
    }

    public async Task<StoredBillResult> SaveAsync(Bill bill, string fileName, BillFileFormat format, CancellationToken cancellationToken = default)
    {
        if (bill is null)
        {
            throw new ArgumentNullException(nameof(bill));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));
        }

        try
        {
            Directory.CreateDirectory(billsDirectory);
            var extension = GetExtension(format);
            var finalPath = Path.Combine(billsDirectory, $"{fileName}{extension}");

            if (format == BillFileFormat.Json)
            {
                var json = JsonSerializer.Serialize(bill, serializerOptions);
                await File.WriteAllTextAsync(finalPath, json, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var content = BuildPlainTextBill(bill);
                await File.WriteAllTextAsync(finalPath, content, cancellationToken).ConfigureAwait(false);
            }

            var info = new LastBillInfo
            {
                FilePath = finalPath,
                Format = format,
                Bill = bill
            };

            var metadataJson = JsonSerializer.Serialize(info, serializerOptions);
            await File.WriteAllTextAsync(metadataPath, metadataJson, cancellationToken).ConfigureAwait(false);

            return new StoredBillResult(bill, finalPath, format);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException or SecurityException)
        {
            throw new BillStorageException("Unable to save bill to disk. Please check file permissions and try again.", ex);
        }
    }

    public async Task<StoredBillResult?> LoadLastBillAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(metadataPath))
        {
            return null;
        }

        try
        {
            await using var stream = File.Open(metadataPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var info = await JsonSerializer.DeserializeAsync<LastBillInfo>(stream, serializerOptions, cancellationToken).ConfigureAwait(false);

            if (info?.Bill is null || string.IsNullOrWhiteSpace(info.FilePath))
            {
                return null;
            }

            return new StoredBillResult(info.Bill, info.FilePath, info.Format);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or SecurityException)
        {
            throw new BillStorageException("Unable to load the last saved bill.", ex);
        }
    }

    private static string GetExtension(BillFileFormat format) => format switch
    {
        BillFileFormat.Json => ".json",
        BillFileFormat.Text => ".txt",
        _ => ".txt"
    };

    private static string BuildPlainTextBill(Bill bill)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Description: {bill.Description}");
        builder.AppendLine($"Price: {bill.Price:F2}");
        builder.AppendLine($"Items: {bill.Items}");
        builder.AppendLine($"Created: {bill.CreatedAt:G}");
        return builder.ToString();
    }

    private sealed class LastBillInfo
    {
        public string? FilePath { get; set; }

        public BillFileFormat Format { get; set; }

        public Bill? Bill { get; set; }
    }
}
