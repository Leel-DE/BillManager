using System;
using System.Windows;
using BillManager.Services.Application;
using BillManager.Services.Messaging;
using BillManager.Services.Storage;
using BillManager.Services.Validation;
using BillManager.ViewModels;

namespace BillManager;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var validationService = new BillValidationService();
            var storageService = new BillStorageService();
            var messageService = new MessageService();
            var applicationService = new ApplicationService();

            var mainViewModel = new MainViewModel(validationService, storageService, messageService, applicationService);

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The application failed to start: {ex.Message}", "Bill Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }
}
