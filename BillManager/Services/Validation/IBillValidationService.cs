namespace BillManager.Services.Validation;

public interface IBillValidationService
{
    BillValidationResult Validate(string description, string priceText, string itemsText, string filename);
}

