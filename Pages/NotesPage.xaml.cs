using BibleNotify.ViewModels;

namespace BibleNotify.Pages;

public partial class NotesPage : ContentPage
{
	private readonly NotesViewModel _viewModel;

	public NotesPage(NotesViewModel viewModel)
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
