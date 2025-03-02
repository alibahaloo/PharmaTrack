using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.DTOs;
using PharmaTrack.WPF.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace PharmaTrack.WPF.ViewModels
{
    public class IngredientInteractionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<string> IngredientList { get; set; } = [];
        public ObservableCollection<string> SelectedIngredients { get; set; } = [];
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    // Only call API if the user is typing; not when updating from selection.
                    if (!_suppressSearch)
                        LoadIngredientListAsync(SearchText);
                }
            }
        }

        // Flag to avoid triggering search when updating text from selection.
        private bool _suppressSearch = false;

        private string? _selectedIngredient;
        public string? SelectedIngredient
        {
            get => _selectedIngredient;
            set
            {
                if (_selectedIngredient != value)
                {
                    _selectedIngredient = value;
                    OnPropertyChanged(nameof(SelectedIngredient));
                    // When an item is selected, update the text accordingly.
                    _suppressSearch = true;
                    SearchText = value ?? string.Empty;
                    _suppressSearch = false;
                }
            }
        }

        // If you need the selected item's ID separately.
        private bool _isDropdownOpen;
        public bool IsDropdownOpen
        {
            get => _isDropdownOpen;
            set
            {
                if (_isDropdownOpen != value)
                {
                    _isDropdownOpen = value;
                    OnPropertyChanged(nameof(IsDropdownOpen));

                    // Use the property setter to notify the FocusBehavior
                    IsCombBoxFocused = value;
                }
            }
        }
        private bool _isComboBoxFocused;
        public bool IsCombBoxFocused
        {
            get => _isComboBoxFocused;
            set
            {
                if (_isComboBoxFocused != value)
                {
                    _isComboBoxFocused = value;
                    OnPropertyChanged(nameof(IsCombBoxFocused));
                }
            }
        }
        private string _statusMessage = default!;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading)); // Notify the UI
                }
            }
        }
        private Brush _statusForeground = default!;
        public Brush StatusForeground
        {
            get => _statusForeground;
            set { _statusForeground = value; OnPropertyChanged(nameof(StatusForeground)); }
        }

        private IngredientInteractionResultDto? _ingredientsInteractionResult;
        public IngredientInteractionResultDto? IngredientsInteractionResult
        {
            get => _ingredientsInteractionResult;
            set
            {
                _ingredientsInteractionResult = value;
                OnPropertyChanged(nameof(IngredientsInteractionResult));
            }
        }

        private readonly DrugService _drugService;
        public ICommand AddSelectedItem { get; }
        public ICommand RemoveItem { get; }
        public IngredientInteractionViewModel(DrugService drugService)
        {
            _drugService = drugService;
            AddSelectedItem = new RelayCommand(ExecuteAddSelectedItem);
            RemoveItem = new RelayCommand(ExecuteRemoveItem);
        }
        private async void ExecuteRemoveItem(object? parameter)
        {
            IsLoading = true;
            if (parameter is string ingredient)
            {
                SelectedIngredients.Remove(ingredient);
            }
            await ExecuteFindInteractions();
            IsLoading = false;
        }
        private async void ExecuteAddSelectedItem(object? parameter)
        {
            IsLoading = true;
            try
            {
                if (!string.IsNullOrEmpty(SelectedIngredient) && !SelectedIngredients.Contains(SelectedIngredient))
                {
                    SelectedIngredients.Add(SelectedIngredient);
                    SelectedIngredient = null;

                    await ExecuteFindInteractions();
                }
            }
            catch
            {

            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteFindInteractions()
        {
            IsLoading = true;
            try
            {
                IngredientsInteractionResult = await _drugService.GetIngredientInteractions(SelectedIngredients.ToList());

                if (IngredientsInteractionResult?.Interactions.Count == 0)
                {
                    StatusMessage = "No interactions found for the selected drugs!";
                    StatusForeground = Brushes.Green;
                }
                else
                {
                    StatusMessage = "Interactions found for the selected drugs!";
                    StatusForeground = Brushes.Blue;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                StatusForeground = Brushes.Red;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async void LoadIngredientListAsync(string startWith = "")
        {
            IsLoading = true;
            try
            {
                var response = await _drugService.GetIngredientLists(startWith);
                IngredientList.Clear();

                if (response != null)
                {
                    foreach (var item in response)
                    {
                        IngredientList.Add(item);
                    }
                    // Open the dropdown when there are results.
                    IsDropdownOpen = true;
                }
                else
                {
                    IsDropdownOpen = false;
                }
            }
            catch
            {

            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
