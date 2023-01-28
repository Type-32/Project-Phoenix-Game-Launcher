using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_Phoenix_Game_Launcher.Core;

namespace Project_Phoenix_Game_Launcher.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public RelayCommand LaunchViewCommand { get; set; }
        public RelayCommand UpdateViewCommand { get; set; }
        public LaunchViewModel LaunchViewModel { get; set; }
        public UpdateViewModel UpdateViewModel { get; set; }
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }
        public MainViewModel()
        {
            LaunchViewModel = new LaunchViewModel();
            UpdateViewModel = new UpdateViewModel();
            CurrentView = LaunchViewModel;

            LaunchViewCommand = new RelayCommand(o =>
            {
                CurrentView = LaunchViewModel;
            });
            UpdateViewCommand = new RelayCommand(o =>
            {
                CurrentView = UpdateViewModel;
            });
        }
    }
}
