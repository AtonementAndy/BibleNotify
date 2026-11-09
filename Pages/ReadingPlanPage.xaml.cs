using BibleNotify.ViewModels;

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
