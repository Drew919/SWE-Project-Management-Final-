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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArchonPM
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateProject : Page
    {
        public CreateProject()
        {
            InitializeComponent();
        }

        private void CreateProjectClick(object sender, RoutedEventArgs e)
        {
            Project newProject = new Project();
            newProject.Name = NameBox.Text;
            newProject.Owner = OwnerBox.Text;
            newProject.Description = DescriptionBox.Text;
            newProject.Status = StatusBox.SelectedItem.ToString();

            var dialog = new ContentDialog
            {
                Title = "Project Created",
                Content = $"\"{newProject.Name}\" was created successfully.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            dialog.XamlRoot = this.XamlRoot; //required for ContentDialog to show up in WinUI 3
            dialog.ShowAsync(); // Show the dialog and wait for user to close it
            Frame.Navigate(typeof(Dashboard));
        }
    }
}
