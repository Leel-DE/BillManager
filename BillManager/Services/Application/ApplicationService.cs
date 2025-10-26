namespace BillManager.Services.Application;

public sealed class ApplicationService : IApplicationService
{
    public void Shutdown()
    {
        System.Windows.Application.Current.Shutdown();
    }
}
