using System.Threading;
using System.Threading.Tasks;
using BillManager.Models;

namespace BillManager.Services.Storage;

public interface IBillStorageService
{
    Task<StoredBillResult> SaveAsync(Bill bill, string fileName, BillFileFormat format, CancellationToken cancellationToken = default);

    Task<StoredBillResult?> LoadLastBillAsync(CancellationToken cancellationToken = default);
}

