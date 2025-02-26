using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.DTOs;
using PharmaTrack.WPF.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace PharmaTrack.WPF.ViewModels
{
    public class DrugInteractionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<DrugListDto> DrugList { get; set; } = [];
        public ObservableCollection<DrugProduct> SelectedDrugs { get; set; } = [];
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
                        LoadDrugListAsync(SearchText);
                }
            }
        }

        // Flag to avoid triggering search when updating text from selection.
        private bool _suppressSearch = false;

        private DrugListDto? _selectedDrug;
        public DrugListDto? SelectedDrug
        {
            get => _selectedDrug;
            set
            {
                if (_selectedDrug != value)
                {
                    _selectedDrug = value;
                    OnPropertyChanged(nameof(SelectedDrug));
                    // When an item is selected, update the text accordingly.
                    _suppressSearch = true;
                    SearchText = _selectedDrug?.BrandName ?? string.Empty;
                    _suppressSearch = false;
                }
            }
        }

        // If you need the selected item's ID separately.
        public int SelectedItemId => SelectedDrug?.Id ?? 0;

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
                }
            }
        }

        private DrugInteractionResultDto? _drugInteractionResult;
        public DrugInteractionResultDto? DrugInteractionResult
        {
            get => _drugInteractionResult;
            set
            {
                _drugInteractionResult = value;
                OnPropertyChanged(nameof(DrugInteractionResult));
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

        private readonly DrugService _drugService;
        public ICommand AddSelectedDrug { get; }
        public ICommand RemoveDrugCommand { get; }
        public ICommand FindInteractions { get; }
        public DrugInteractionViewModel(DrugService drugService)
        {
            _drugService = drugService;
            AddSelectedDrug = new RelayCommand(ExecuteAddSelectedDrug);
            RemoveDrugCommand = new RelayCommand(ExecuteRemoveDrug);
            FindInteractions = new RelayCommand(ExecuteFindInteractions);
        }
        private void ExecuteRemoveDrug(object? parameter)
        {
            IsLoading = true;
            if (parameter is DrugProduct drug)
            {
                SelectedDrugs.Remove(drug);
            }
            IsLoading = false;
        }
        private async void ExecuteFindInteractions(object? parameter)
        {
            IsLoading = true;
            try
            {
                List<int> drugCodes = [.. SelectedDrugs.Select(s => s.DrugCode)];
                DrugInteractionResult = await _drugService.GetDrugInteractions(drugCodes);

                if (DrugInteractionResult?.Interactions.Count == 0)
                {
                    StatusMessage = "No interactions found for the selected drugs!";
                    StatusForeground = Brushes.Green;
                } else
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
        private async void ExecuteAddSelectedDrug(object? parameter)
        {
            IsLoading = true;
            try
            {
                if (SelectedDrug?.DrugCode == null) return;
                var drugProduct = await _drugService.GetDrugInfoByCodeAsync(SelectedDrug.DrugCode);

                if (drugProduct?.Product != null)
                {
                    var product = drugProduct.Product;
                    // Check if the product is already in the list by comparing a unique property (e.g., DrugCode)
                    if (!SelectedDrugs.Any(d => d.DrugCode == product.DrugCode) && SelectedDrugs.Count < 11)
                    {
                        SelectedDrugs.Add(product);
                        SelectedDrug = null;
                    }
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

        public async void LoadDrugListAsync(string? startWith = null)
        {
            if (string.IsNullOrEmpty(startWith))
            {
                // Optionally, clear the list and close the dropdown.
                DrugList.Clear();
                IsDropdownOpen = false;
                return;
            }
            IsLoading = true;
            try
            {
                var response = await _drugService.GetDrugList(startWith);
                DrugList.Clear();

                if (response != null)
                {
                    foreach (var item in response)
                    {
                        DrugList.Add(item);
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
