using Microsoft.Maui.Controls;
using MusicPlayerGUI.ViewModels;

namespace MusicPlayerGUI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            // Устанавливаем BindingContext в экземпляр MainPageViewModel
            BindingContext = new MainPageViewModel();
        }
    }
}