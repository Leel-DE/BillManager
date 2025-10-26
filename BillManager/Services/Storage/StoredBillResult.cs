using BillManager.Models;

namespace BillManager.Services.Storage;

public sealed class StoredBillResult
{
    public StoredBillResult(Bill bill, string filePath, BillFileFormat format)
    {
        Bill = bill;
        FilePath = filePath;
        Format = format;
    }

    public Bill Bill { get; }

    public string FilePath { get; }

    public BillFileFormat Format { get; }
}

