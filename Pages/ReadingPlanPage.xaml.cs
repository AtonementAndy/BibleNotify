using BibleNotify.ViewModels; // <-- ESSA LINHA ESTÁ FALTANDO NO SEU CÓDIGO
using Microsoft.Maui.Controls; // (Opcional, mas garante que ele ache o ContentPage)

namespace BibleNotify.Pages;

public partial class ReadingPlanPage : ContentPage
{
    private readonly ReadingPlanViewModel _viewModel;

    public ReadingPlanPage(ReadingPlanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}