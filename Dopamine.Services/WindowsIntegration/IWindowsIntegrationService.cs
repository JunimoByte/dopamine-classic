using System;

namespace Dopamine.Services.WindowsIntegration
{
    public interface IWindowsIntegrationService
    {
        bool IsSystemUsingLightTheme { get; }
        bool IsStartedFromExplorer { get; }
        event EventHandler SystemUsesLightThemeChanged;
        void StartMonitoringSystemUsesLightTheme();
        void StopMonitoringSystemUsesLightTheme();
    }
}
