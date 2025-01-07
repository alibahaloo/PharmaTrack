using PharmaTrack.Shared.DBModels;
using PharmaTrack.WPF.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Windows.Input;

namespace PharmaTrack.WPF.ViewModels
{
    public class InventoryViewModel
    {
        private readonly InventoryService _inventoryService;

        public ObservableCollection<Product> Products { get; set; }

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

        public ICommand LoadProductsCommand { get; }

        public InventoryViewModel(InventoryService inventoryService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            Products = new ObservableCollection<Product>();
            LoadProductsCommand = new AsyncRelayCommand(async _ => await LoadProductsAsync());
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                var products = await _inventoryService.GetProductsAsync();
                if (products != null)
                {
                    Products.Clear();
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }
                }
                StatusMessage = "Products loaded successfully.";
            }
            catch (UnauthorizedAccessException ex)
            {
                StatusMessage = $"Authorization error: {ex.Message}";
            }
            catch (HttpRequestException ex)
            {
                StatusMessage = $"Network error: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
