using Digimezzo.Foundation.Core.Settings;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsStartupViewModel : BindableBase
    {
        private bool checkBoxStartupPageChecked;
        private bool checkBoxRememberLastPlayedTrackChecked;
        private bool isportable;

        public bool CheckBoxStartupPageChecked
        {
            get { return this.checkBoxStartupPageChecked; }
            set
            {
                SettingsClient.Set<bool>("Startup", "ShowLastSelectedPage", value);
                SetProperty<bool>(ref this.checkBoxStartupPageChecked, value);
            }
        }

        public bool CheckBoxRememberLastPlayedTrackChecked
        {
            get { return this.checkBoxRememberLastPlayedTrackChecked; }
            set
            {
                SettingsClient.Set<bool>("Startup", "RememberLastPlayedTrack", value);
                SetProperty<bool>(ref this.checkBoxRememberLastPlayedTrackChecked, value);
            }
        }

        public bool IsPortable
        {
            get { return this.isportable; }
            set { SetProperty<bool>(ref this.isportable, value); }
        }

        public SettingsStartupViewModel()
        {
            this.IsPortable = SettingsClient.Get<bool>("Configuration", "IsPortable");

            // CheckBoxes
            this.GetCheckBoxesAsync();
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.checkBoxStartupPageChecked = SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage");
                this.checkBoxRememberLastPlayedTrackChecked = SettingsClient.Get<bool>("Startup", "RememberLastPlayedTrack");
            });
        }
    }
}
