using CommunityToolkit.Mvvm.ComponentModel;

namespace TasteHub.ViewModels
{
    /// <summary>
    /// Base view model providing shared properties for all view models
    /// </summary>
    public partial class BaseViewModel : ObservableObject
    {
        /// <summary>Indicates whether the view model is currently loading data</summary>
        [ObservableProperty]
        private bool _isBusy;

        /// <summary>Title displayed on the page</summary>
        [ObservableProperty]
        private string _title = string.Empty;
    }
}