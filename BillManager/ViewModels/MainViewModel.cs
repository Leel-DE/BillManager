using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BillManager.Commands;
using BillManager.Models;
using BillManager.Services.Application;
using BillManager.Services.Messaging;
using BillManager.Services.Storage;
using BillManager.Services.Validation;

namespace BillManager.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IBillValidationService validationService;
    private readonly IBillStorageService storageService;
    private readonly IMessageService messageService;
    private readonly IApplicationService applicationService;

    private string description = string.Empty;
    private string priceText = string.Empty;
    private string itemsText = string.Empty;
    private string filename = string.Empty;
    private BillFileFormat selectedFormat = BillFileFormat.Json;
    private string statusMessage = "Fill in the form to create a bill.";
    private bool isStatusError;
    private string lastBillDetails = "No bill has been loaded yet.";
    private string lastSavedFileSummary = "No bill has been saved yet.";

    public MainViewModel(
        IBillValidationService validationService,
        IBillStorageService storageService,
        IMessageService messageService,
        IApplicationService applicationService)
    {
        this.validationService = validationService;
        this.storageService = storageService;
        this.messageService = messageService;
        this.applicationService = applicationService;

        ValidationMessages.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasValidationMessages));

        SaveBillCommand = new AsyncRelayCommand(SaveBillAsync, canExecute: () => !IsBusy, exceptionHandler: ex => HandleUnexpectedError(ex, "An unexpected error occurred while saving the bill."));
        ViewLastBillCommand = new AsyncRelayCommand(ViewLastBillAsync, exceptionHandler: ex => HandleUnexpectedError(ex, "An unexpected error occurred while loading the last bill."));
        ExitCommand = new RelayCommand(() => applicationService.Shutdown());
        ResetFormCommand = new RelayCommand(ResetForm);
    }

    public AsyncRelayCommand SaveBillCommand { get; }

    public AsyncRelayCommand ViewLastBillCommand { get; }

    public RelayCommand ExitCommand { get; }

    public RelayCommand ResetFormCommand { get; }

    public ObservableCollection<string> ValidationMessages { get; } = new();

    public bool HasValidationMessages => ValidationMessages.Count > 0;

    public string Description
    {
        get => description;
        set => SetProperty(ref description, value);
    }

    public string PriceText
    {
        get => priceText;
        set => SetProperty(ref priceText, value);
    }

    public string ItemsText
    {
        get => itemsText;
        set => SetProperty(ref itemsText, value);
    }

    public string Filename
    {
        get => filename;
        set => SetProperty(ref filename, value);
    }

    public BillFileFormat SelectedFormat
    {
        get => selectedFormat;
        set => SetProperty(ref selectedFormat, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public bool IsStatusError
    {
        get => isStatusError;
        private set => SetProperty(ref isStatusError, value);
    }

    public string LastBillDetails
    {
        get => lastBillDetails;
        private set => SetProperty(ref lastBillDetails, value);
    }

    public string LastSavedFileSummary
    {
        get => lastSavedFileSummary;
        private set => SetProperty(ref lastSavedFileSummary, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetProperty(ref isBusy, value))
            {
                SaveBillCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private bool isBusy;

    private async Task SaveBillAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            ClearValidationMessages();

            var validationResult = validationService.Validate(Description, PriceText, ItemsText, Filename);

            if (!validationResult.IsValid || validationResult.Bill is null || string.IsNullOrWhiteSpace(validationResult.SanitizedFileName))
            {
                ApplyValidationMessages(validationResult.Errors);
                SetStatus("Please correct the highlighted validation errors.", true);
                return;
            }

            try
            {
                var storedBill = await storageService.SaveAsync(validationResult.Bill, validationResult.SanitizedFileName, SelectedFormat);
                LastSavedFileSummary = $"Saved as {Path.GetFileName(storedBill.FilePath)} ({storedBill.Format.ToString().ToUpperInvariant()})";
                LastBillDetails = RenderBillDetails(storedBill.Bill);

                var successMessage = $"Bill successfully saved as {Path.GetFileName(storedBill.FilePath)}";
                SetStatus(successMessage, false);
                messageService.ShowInformation(successMessage);
            }
            catch (BillStorageException ex)
            {
                messageService.ShowError(ex.Message);
                SetStatus(ex.Message, true);
            }
        }
        catch (Exception)
        {
            const string fallbackMessage = "An unexpected error occurred while saving the bill.";
            messageService.ShowError(fallbackMessage);
            SetStatus(fallbackMessage, true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ViewLastBillAsync()
    {
        try
        {
            ClearValidationMessages();

            var lastBill = await storageService.LoadLastBillAsync();

            if (lastBill is null)
            {
                const string noBillMessage = "No bill information is available yet.";
                LastSavedFileSummary = noBillMessage;
                LastBillDetails = "Save a bill to see details here.";
                SetStatus(noBillMessage, true);
                messageService.ShowInformation(noBillMessage);
                return;
            }

            LastSavedFileSummary = $"Last saved file: {Path.GetFileName(lastBill.FilePath)} ({lastBill.Format.ToString().ToUpperInvariant()})";
            LastBillDetails = RenderBillDetails(lastBill.Bill);

            const string successMessage = "Last bill loaded successfully.";
            SetStatus(successMessage, false);
            messageService.ShowInformation(successMessage);
        }
        catch (BillStorageException ex)
        {
            messageService.ShowError(ex.Message);
            SetStatus(ex.Message, true);
        }
        catch (Exception)
        {
            const string fallbackMessage = "An unexpected error occurred while loading the last bill.";
            messageService.ShowError(fallbackMessage);
            SetStatus(fallbackMessage, true);
        }
    }

    private void ResetForm()
    {
        Description = string.Empty;
        PriceText = string.Empty;
        ItemsText = string.Empty;
        Filename = string.Empty;
        SelectedFormat = BillFileFormat.Json;
        ClearValidationMessages();
        SetStatus("Form cleared. Enter new bill details.", false);
    }

    private void ClearValidationMessages()
    {
        ValidationMessages.Clear();
    }

    private void ApplyValidationMessages(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            ValidationMessages.Add(error);
        }
    }

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsStatusError = isError;
    }

    private void HandleUnexpectedError(Exception exception, string userFriendlyMessage)
    {
        messageService.ShowError(userFriendlyMessage);
        SetStatus(userFriendlyMessage, true);
        System.Diagnostics.Debug.WriteLine(exception);
    }

    private static string RenderBillDetails(Bill bill)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Description: {bill.Description}");
        builder.AppendLine($"Price: {bill.Price.ToString("C2", CultureInfo.CurrentCulture)}");
        builder.AppendLine($"Items: {bill.Items}");
        builder.AppendLine($"Created: {bill.CreatedAt.ToString("f", CultureInfo.CurrentCulture)}");
        return builder.ToString();
    }
}

