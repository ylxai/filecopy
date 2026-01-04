using System.Windows;
using System.Windows.Controls;
using FileCopyUtility.Models;
using FileCopyUtility.Services;

namespace FileCopyUtility.Windows;

public partial class ProfileWindow : Window
{
    private readonly ProfileService _profileService;
    private List<CopyProfile> _profiles = new();
    private CopyProfile? _currentProfile;

    public ProfileWindow()
    {
        InitializeComponent();
        _profileService = new ProfileService();
        LoadProfiles();
    }

    private async void LoadProfiles()
    {
        _profiles = await _profileService.GetAllProfilesAsync();
        ListProfiles.ItemsSource = _profiles;
    }

    private void ListProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListProfiles.SelectedItem is CopyProfile profile)
        {
            _currentProfile = profile;
            LoadProfileDetails(profile);
            PanelProfileDetails.Visibility = Visibility.Visible;
        }
    }

    private void LoadProfileDetails(CopyProfile profile)
    {
        TxtProfileName.Text = profile.Name;
        TxtDescription.Text = profile.Description;
    }

    private void BtnNewProfile_Click(object sender, RoutedEventArgs e)
    {
        _currentProfile = null;
        TxtProfileName.Clear();
        TxtDescription.Clear();
        PanelProfileDetails.Visibility = Visibility.Visible;
    }

    private async void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtProfileName.Text))
        {
            System.Windows.MessageBox.Show("Profile name is required.", "Validation Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        var profile = _currentProfile ?? new CopyProfile();
        profile.Name = TxtProfileName.Text.Trim();
        profile.Description = TxtDescription.Text.Trim();

        await _profileService.SaveProfileAsync(profile);
        LoadProfiles();

        System.Windows.MessageBox.Show($"Profile '{profile.Name}' saved successfully!", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private async void BtnDeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        if (_currentProfile == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete profile '{_currentProfile.Name}'?",
            "Delete Profile",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            await _profileService.DeleteProfileAsync(_currentProfile.Name);
            LoadProfiles();
            PanelProfileDetails.Visibility = Visibility.Collapsed;
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
