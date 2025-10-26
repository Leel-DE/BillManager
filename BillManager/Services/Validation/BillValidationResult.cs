using System.Collections.Generic;
using BillManager.Models;

namespace BillManager.Services.Validation;

public sealed class BillValidationResult
{
    public BillValidationResult(Bill? bill, string? sanitizedFileName, IReadOnlyList<string> errors)
    {
        Bill = bill;
        SanitizedFileName = sanitizedFileName;
        Errors = errors;
    }

    public Bill? Bill { get; }

    public string? SanitizedFileName { get; }

    public IReadOnlyList<string> Errors { get; }

    public bool IsValid => Bill is not null && !string.IsNullOrWhiteSpace(SanitizedFileName) && Errors.Count == 0;
}

