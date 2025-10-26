using System;

namespace BillManager.Services.Storage;

public class BillStorageException : Exception
{
    public BillStorageException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

