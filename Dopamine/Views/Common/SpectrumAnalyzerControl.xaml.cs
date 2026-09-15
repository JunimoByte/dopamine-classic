using CommonServiceLocator;
using Digimezzo.Foundation.Core.Settings;
using Dopamine.Core.Enums;
using Dopamine.Core.Helpers;
using Dopamine.Services.Playback;
using Dopamine.Services.Shell;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.Common
{
    public partial class SpectrumAnalyzerControl : UserControl
    {
        private IPlaybackService playbackService;
        private IShellService shellService;

        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }

        public SpectrumAnalyzerControl()
        {
            InitializeComponent();

            this.playbackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
            this.shellService = ServiceLocator.Current.GetInstance<IShellService>();

            this.Loaded += SpectrumAnalyzerControl_Loaded;
            this.Unloaded += SpectrumAnalyzerControl_Unloaded;
        }

        private void SpectrumAnalyzerControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.playbackService.PlaybackSuccess += PlaybackService_PlaybackSuccess;
            this.shellService.WindowStateChanged += ShellService_WindowStateChanged;
            SettingsClient.SettingChanged += SettingsClient_SettingChanged;
            this.TryRegisterSpectrumPlayers();
        }

        private void SpectrumAnalyzerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.playbackService.PlaybackSuccess -= PlaybackService_PlaybackSuccess;
            this.shellService.WindowStateChanged -= ShellService_WindowStateChanged;
            SettingsClient.SettingChanged -= SettingsClient_SettingChanged;
            this.UnregisterSpectrumPlayers();
        }

        private void PlaybackService_PlaybackSuccess(object sender, System.EventArgs e) => this.TryRegisterSpectrumPlayers();
        
        private void ShellService_WindowStateChanged(object sender, System.EventArgs e) => this.TryRegisterSpectrumPlayers();
        
        private void SettingsClient_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (SettingsClient.IsSettingChanged(e, "Playback", "ShowSpectrumAnalyzer"))
            {
                this.TryRegisterSpectrumPlayers();
            }
        }

        private void TryRegisterSpectrumPlayers()
        {
            this.UnregisterSpectrumPlayers();

            if (!this.playbackService.HasMediaFoundationSupport)
            {
                return;
            }

            if (!SettingsClient.Get<bool>("Playback", "ShowSpectrumAnalyzer"))
            {
                return;
            }

            if (this.shellService.WindowState == WindowState.Minimized)
            {
                return;
            }

            var player = this.playbackService.Player;
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() => 
            {
                this.SpectrumContainer.Visibility = Visibility.Visible;

                if (player != null)
                {
                    player.ClearSpectrumPlayers();
                    this.LeftSpectrumAnalyzer.RegisterSoundPlayer(player.GetWrapperSpectrumPlayer(SpectrumChannel.Left));
                    this.RightSpectrumAnalyzer.RegisterSoundPlayer(player.GetWrapperSpectrumPlayer(SpectrumChannel.Right));
                }
            }));
        }

        private void UnregisterSpectrumPlayers()
        {
            var player = this.playbackService.Player;
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() => 
            {
                this.SpectrumContainer.Visibility = Visibility.Collapsed;

                if (player != null)
                {
                    this.LeftSpectrumAnalyzer.UnregisterSoundPlayer();
                    this.RightSpectrumAnalyzer.UnregisterSoundPlayer();
                }
            }));
        }
    }
}
