using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.DTOs;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PharmaTrack.WPF.ViewModels
{
    public class IngredientInteractionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<IngredientListDto> IngredientList { get; set; } = [];
        public ObservableCollection<DrugIngredient> SelectedIngredients { get; set; } = [];
    }
}
