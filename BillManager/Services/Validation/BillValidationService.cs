using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using BillManager.Models;

namespace BillManager.Services.Validation;

public sealed class BillValidationService : IBillValidationService
{
    private static readonly Regex FileNameRegex = new("^[A-Za-z0-9_]{1,10}$", RegexOptions.Compiled);

    public BillValidationResult Validate(string description, string priceText, string itemsText, string filename)
    {
        var errors = new List<string>();

        var trimmedDescription = (description ?? string.Empty).Trim();
        var trimmedPriceText = (priceText ?? string.Empty).Trim();
        var trimmedItemsText = (itemsText ?? string.Empty).Trim();
        var trimmedFilename = (filename ?? string.Empty).Trim();

        if (trimmedDescription.Length < 3 || trimmedDescription.Length > 20)
        {
            errors.Add("Description must be between 3 and 20 characters.");
        }

        if (!TryParsePrice(trimmedPriceText, out var price) || price <= 0)
        {
            errors.Add("Price must be a valid number greater than 0.");
        }

        if (!int.TryParse(trimmedItemsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var items) || items < 0 || items > 5)
        {
            errors.Add("Items must be an integer between 0 and 5.");
        }

        if (!FileNameRegex.IsMatch(trimmedFilename))
        {
            errors.Add("Filename must be 1-10 characters long and contain only letters, numbers, or underscores.");
        }

        Bill? bill = null;

        if (errors.Count == 0)
        {
            bill = new Bill(trimmedDescription, decimal.Round(price, 2), items, DateTime.Now);
        }

        return new BillValidationResult(bill, errors.Count == 0 ? trimmedFilename : null, errors.AsReadOnly());
    }

    private static bool TryParsePrice(string input, out decimal price)
    {
        return decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out price) ||
               decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out price);
    }
}

