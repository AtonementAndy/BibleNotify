using BibleNotify.ViewModels;

namespace BibleNotify.Pages;

public partial class BiblePage : ContentPage, IQueryAttributable
{
	private readonly BibleViewModel _viewModel;
	private int? _pendingBookId;
	private int? _pendingChapter;

	public BiblePage(BibleViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadCommand.ExecuteAsync(null);
		if (_pendingBookId is { } bookId && _pendingChapter is { } chapter)
		{
			await _viewModel.NavigateToReferenceAsync(bookId, chapter);
			_pendingBookId = null;
			_pendingChapter = null;
		}
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		_pendingBookId = GetInteger(query, "bookId");
		_pendingChapter = GetInteger(query, "chapter");
	}

	private static int? GetInteger(IDictionary<string, object> query, string key) =>
		query.TryGetValue(key, out var value) && int.TryParse(value?.ToString(), out var parsed)
			? parsed
			: null;
}
