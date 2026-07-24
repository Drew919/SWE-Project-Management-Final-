using ArchonPM.Objects;
using ArchonPM.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArchonPM;

    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        // Open the dashboard (or ProjectsPage) when the app starts
        rootFrame.Navigate(typeof(Dashboard));
        }

    private void DashboardClick(object sender, RoutedEventArgs e)
        { 
        rootFrame.Navigate(typeof(Dashboard));
        }
        
    private void ProjectsClick(object sender, RoutedEventArgs e)
        {
        rootFrame.Navigate(typeof(ViewProjects));
    }

    private void NewProjectClick(object sender, RoutedEventArgs e)
    {
        rootFrame.Navigate(typeof(CreateProject));
    }

    
}
