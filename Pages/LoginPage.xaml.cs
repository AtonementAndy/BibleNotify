using BibleNotify.ViewModels;

namespace BibleNotify.Pages;

public partial class LoginPage : ContentPage, IQueryAttributable
{
	private readonly LoginViewModel _viewModel;

	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query) =>
		_viewModel.ApplyQueryAttributes(query);
}
