using Dopamine.Views.Common.Base;
using Dopamine.Core.Prism;
using Prism.Commands;
using Prism.Events;
using System.Windows;
using System.Windows.Input;

namespace Dopamine.Views.Common
{
    public partial class NowPlayingControl : TracksViewBase
    {
        private SubscriptionToken scrollSubscription;

        public NowPlayingControl()
        {
            InitializeComponent();

            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(() => this.ScrollToPlayingTrackAsync(this.ListBoxTracks));

            this.Loaded += NowPlayingControl_Loaded;
            this.Unloaded += NowPlayingControl_Unloaded;
        }
        
        private void NowPlayingControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.scrollSubscription == null)
            {
                this.scrollSubscription = this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Subscribe(async (_) => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));
            }
        }
        
        private void NowPlayingControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.scrollSubscription != null)
            {
                this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Unsubscribe(this.scrollSubscription);
                this.scrollSubscription = null;
            }
            if (this.DataContext is System.IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private async void ListBoxTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, e.OriginalSource as DependencyObject, false);
        }

        private void ListBoxTracks_KeyUp(object sender, KeyEventArgs e)
        {
            this.KeyUpHandlerAsync(sender, e);
        }

        private void ListBoxTracks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.ActionHandler(sender, e.OriginalSource as DependencyObject, false);
            }
        }
    }
}
