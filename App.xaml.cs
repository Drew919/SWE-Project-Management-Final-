using ArchonPM.Services;
using Microsoft.UI.Xaml;
using System;

namespace ArchonPM
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>
        /// Shared in-memory project store for the current application session.
        /// </summary>
        public IProjectService ProjectService { get; } = new ProjectService();

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
