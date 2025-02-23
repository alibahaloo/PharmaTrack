using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.DTOs;
using PharmaTrack.WPF.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;

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

        private readonly DrugService _drugService;
        public DrugInteractionViewModel(DrugService drugService)
        {
            _drugService = drugService;
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
    }
}
