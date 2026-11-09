using BibleNotify.ViewModels;

namespace BibleNotify.Pages;

public partial class SettingsPage : ContentPage, IQueryAttributable
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("section", out var section))
        {
            _viewModel.OpenSection(section?.ToString());
        }
    }
}
