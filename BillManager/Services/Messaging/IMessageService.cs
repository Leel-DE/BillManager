namespace BillManager.Services.Messaging;

public interface IMessageService
{
    void ShowInformation(string message, string? caption = null);

    void ShowError(string message, string? caption = null);
}

