using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Settings;
using Dopamine.Services.Playback;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Dopamine.Services.WindowsIntegration
{
    public class WindowsIntegrationService : IWindowsIntegrationService
    {
        private IPlaybackService playbackService;
        private bool isStartedFromExplorer;

        private bool isMonitoringSystemUsesLightTheme;
        private bool lastSystemUsesLightTheme;
    
        public WindowsIntegrationService(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;
            this.isStartedFromExplorer = Environment.GetCommandLineArgs().Length > 1;

            if(SettingsClient.Get<bool>("Playback", "PreventSleepWhilePlaying"))
            {
                this.EnableSleepPrevention();
            }

            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Playback", "PreventSleepWhilePlaying"))
                {
                    bool preventSleepWhilePlaying = (bool)e.Entry.Value;

                    if (preventSleepWhilePlaying)
                    {
                        this.EnableSleepPrevention();
                    }
                    else
                    {
                        this.DisableSleepPrevention();
                    }
                }
            };
            
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                if (this.isMonitoringSystemUsesLightTheme)
                {
                    bool currentTheme = this.IsSystemUsingLightTheme;
                    if (currentTheme != this.lastSystemUsesLightTheme)
                    {
                        this.lastSystemUsesLightTheme = currentTheme;
                        this.SystemUsesLightThemeChanged?.Invoke(this, new EventArgs());
                    }
                }
            }
        }

        private void EnableSleepPrevention()
        {
            this.playbackService.PlaybackFailed += (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackPaused += (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackStopped += (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackResumed += (_, __) => this.DisableSleep();
            this.playbackService.PlaybackSuccess += (_, __) => this.DisableSleep();

            if (this.playbackService.IsPlaying)
            {
                this.DisableSleep();
            }
        }

        private void DisableSleepPrevention()
        {
            this.playbackService.PlaybackFailed -= (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackPaused -= (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackStopped -= (_, __) => this.RestoreSleep();
            this.playbackService.PlaybackResumed -= (_, __) => this.DisableSleep();
            this.playbackService.PlaybackSuccess -= (_, __) => this.DisableSleep();

            this.RestoreSleep();
        }

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        public event EventHandler SystemUsesLightThemeChanged = delegate { };

        public bool IsSystemUsingLightTheme
        {
            get
            {
                int registrySystemUsesLightTheme = 0;

                try
                {
                    registrySystemUsesLightTheme = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", 0);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not get system uses light theme from registry. Exception: {0}", ex.Message);
                }

                return registrySystemUsesLightTheme == 1 ? true : false;
            }
        }

        public bool IsStartedFromExplorer
        {
            get
            {
                // Only returns true the first time we ask for the property
                bool returnValue = this.isStartedFromExplorer;
                this.isStartedFromExplorer = false;
                return returnValue;
            }
        }



        public void StartMonitoringSystemUsesLightTheme()
        {
            this.isMonitoringSystemUsesLightTheme = true;
            this.lastSystemUsesLightTheme = this.IsSystemUsingLightTheme;
        }

        public void StopMonitoringSystemUsesLightTheme()
        {
            this.isMonitoringSystemUsesLightTheme = false;
        }

        private void DisableSleep()
        {
            try
            {
                // To stop screen saver and monitor power off event
                // You can combine several flags and specify multiple behaviors with a single call
                // See: https://stackoverflow.com/questions/38282770/stop-screensaver-programmatically
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            }
            catch (Exception ex)
            {
                LogClient.Error("Failed to disable sleep. Exception: {0}", ex.Message);
            }
           
        }

        private void RestoreSleep()
        {
            try
            {
                // To reset or allow those events again you have to call this API with only ES_CONTINUOUS
                // See: https://stackoverflow.com/questions/38282770/stop-screensaver-programmatically
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
            catch (Exception ex)
            {
                LogClient.Error("Failed to restore sleep. Exception: {0}", ex.Message);
            }
        }
    }
}
