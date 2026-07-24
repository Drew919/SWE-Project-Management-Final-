using ArchonPM.Navigation;
using ArchonPM.Services;
using Microsoft.UI.Xaml;

namespace ArchonPM
{
    public partial class App : Application
    {
        private MainWindow? _window;

        public IProjectService ProjectService { get; } = new ProjectService();

        public NavigationContext Navigation { get; } = new();

        public MainWindow? MainWindow => _window;

        public static new App Current => (App)Application.Current;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
    }
}
