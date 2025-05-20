using PharmaTrack.Core.DTOs;
using PharmaTrack.Core.DBModels;
using PharmaTrack.WPF.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Windows.Input;
using System.Windows.Media;

namespace PharmaTrack.WPF.ViewModels
{
    public class IngredientListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private readonly DrugService _drugService;
        public ObservableCollection<DrugIngredient> Ingredients { get; set; } = [];

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

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }
        private int _totalPages = 1;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged(nameof(TotalPages));
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

        private int? _activeIngredientCode;
        public int? ActiveIngredientCode
        {
            get => _activeIngredientCode;
            set { _activeIngredientCode = value; OnPropertyChanged(nameof(ActiveIngredientCode)); }
        }
        private string? _ingredient;
        public string? Ingredient
        {
            get => _ingredient;
            set { _ingredient = value; OnPropertyChanged(nameof(Ingredient)); }
        }
        private int? _drugCode;
        public int? DrugCode
        {
            get => _drugCode;
            set { _drugCode = value; OnPropertyChanged(nameof(DrugCode)); }
        }

        public ICommand LoadIngredientsCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ResetFiltersCommand { get; }

        public IngredientListViewModel(DrugService drugService)
        {
            _drugService = drugService;
            LoadIngredientsCommand = new AsyncRelayCommand(async _ => await LoadIngredientsAsync());
            NextPageCommand = new AsyncRelayCommand(async _ => await ChangePageAsync(1));
            PreviousPageCommand = new AsyncRelayCommand(async _ => await ChangePageAsync(-1));

            ApplyFiltersCommand = new AsyncRelayCommand(async _ => await LoadIngredientsAsync());
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());
        }

        private async void ResetFilters()
        {
            DrugCode = null;
            ActiveIngredientCode = null;
            Ingredient = null;
            await LoadIngredientsAsync();
        }

        public async Task LoadIngredientsAsync()
        {
            IsLoading = true;
            try
            {
                // Create a request with filters from ViewModel properties
                var request = new DrugIngredientRequest
                {
                    DrugCode = DrugCode,
                    ActiveIngredientCode = ActiveIngredientCode,
                    Ingredient = Ingredient,
                };

                var response = await _drugService.GetIngredientsAsync(request, CurrentPage);
                if (response != null)
                {
                    Ingredients.Clear();
                    foreach (var drug in response.Data)
                    {
                        Ingredients.Add(drug);
                    }

                    CurrentPage = response.CurrentPage;
                    TotalPages = response.TotalPageCount;
                }
                StatusMessage = "Items loaded successfully.";
                StatusForeground = Brushes.Green;
            }
            catch (UnauthorizedAccessException ex)
            {
                StatusMessage = $"Authorization error: {ex.Message}";
                StatusForeground = Brushes.Red;
            }
            catch (HttpRequestException ex)
            {
                StatusMessage = $"Network error: {ex.Message}";
                StatusForeground = Brushes.Red;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ChangePageAsync(int direction)
        {
            if ((direction == -1 && CurrentPage > 1) || (direction == 1 && CurrentPage < TotalPages))
            {
                CurrentPage += direction;
                await LoadIngredientsAsync();
            }
        }
    }
}
