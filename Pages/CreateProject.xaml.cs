using ArchonPM.Objects;
using ArchonPM.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace ArchonPM
{
    public sealed partial class CreateProject : Page
    {
        private bool _isSubmitting;

        public CreateProject()
        {
            InitializeComponent();
        }

        private async void CreateProjectClick(object sender, RoutedEventArgs e)
        {
            if (_isSubmitting)
            {
                return;
            }

            string name = NameBox.Text?.Trim() ?? string.Empty;
            string owner = OwnerBox.Text?.Trim() ?? string.Empty;
            string description = DescriptionBox.Text?.Trim() ?? string.Empty;
            string? status = (StatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(owner) ||
                string.IsNullOrWhiteSpace(status))
            {
                var validationDialog = new ContentDialog
                {
                    Title = "Missing Information",
                    Content = "Please enter a project name, owner, and status before creating a project.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await validationDialog.ShowAsync();
                return;
            }

            _isSubmitting = true;
            if (sender is Button createButton)
            {
                createButton.IsEnabled = false;
            }

            try
            {
                Project newProject = new Project
                {
                    Name = name,
                    Owner = owner,
                    Description = description,
                    Status = status
                };

                App.Current.ProjectService.AddProject(newProject);

                var dialog = new ContentDialog
                {
                    Title = "Project Created",
                    Content = $"\"{newProject.Name}\" was created successfully.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
                Frame.Navigate(typeof(ViewProjects));
            }
            finally
            {
                _isSubmitting = false;
                if (sender is Button button)
                {
                    button.IsEnabled = true;
                }
            }
        }
    }
}
