namespace BibleNotify.Controls;

public partial class FloatingTabBar : ContentView
{
    public static readonly BindableProperty SelectedTabProperty =
        BindableProperty.Create(nameof(SelectedTab), typeof(string), typeof(FloatingTabBar), "home");

    public string SelectedTab
    {
        get => (string)GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    public FloatingTabBar()
    {
        InitializeComponent();
    }

    private Task NavigateAsync(string route) =>
        Shell.Current.GoToAsync($"//main/{route}");

    private async void OnHomeTapped(object? sender, TappedEventArgs e) => await NavigateAsync("home");

    private async void OnPlansTapped(object? sender, TappedEventArgs e) => await NavigateAsync("plans");

    private async void OnBibleTapped(object? sender, TappedEventArgs e) => await NavigateAsync("bible");

    private async void OnNotesTapped(object? sender, TappedEventArgs e) => await NavigateAsync("notes");

    private async void OnSettingsTapped(object? sender, TappedEventArgs e) => await NavigateAsync("settings");
}
