using System;
using System.Windows;

namespace BillManager.Services.Messaging;

public sealed class MessageService : IMessageService
{
    private const string DefaultCaption = "Bill Manager";

    public void ShowInformation(string message, string? caption = null)
    {
        InvokeOnUi(() => MessageBox.Show(message, caption ?? DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Information));
    }

    public void ShowError(string message, string? caption = null)
    {
        InvokeOnUi(() => MessageBox.Show(message, caption ?? DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Error));
    }

    private static void InvokeOnUi(Action action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;

        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            dispatcher.Invoke(action);
        }
    }
}
