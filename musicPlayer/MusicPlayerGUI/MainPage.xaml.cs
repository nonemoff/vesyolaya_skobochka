using System;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using MusicPlayerLib;

namespace MusicPlayerGUI
{
    public partial class MainPage : ContentPage
    {
        // Экземпляр класса MusicPlayer
        MusicPlayer _musicPlayer = new MusicPlayer();

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLoadSongsClicked(object sender, EventArgs e)
        {
            try
            {
                // Считываем путь из текстового поля
                string path = PathEntry.Text;

                // Загружаем песни по указанному пути
                _musicPlayer.LoadSongs(path);

                // Получаем список треков из буфера
                List<Track> tracks = _musicPlayer.GetBuffer();

                // Отображаем полученные треки в ListView
                SongsListView.ItemsSource = tracks;
            }
            catch (Exception ex)
            {
                // При возникновении ошибки выводим всплывающее окно
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private async void OnAddTrackClicked(object sender, EventArgs e)
        {
            try
            {
                // Получаем нажатую кнопку и трек из её BindingContext
                Button addButton = sender as Button;
                if (addButton == null)
                    return;

                Track track = addButton.BindingContext as Track;
                if (track == null)
                    return;

                // Определяем индекс трека в локальном списке _tracks
                int index = _musicPlayer.GetBuffer().IndexOf(track);
                if (index < 0)
                {
                    await DisplayAlert("Ошибка", "Трек не найден в списке.", "OK");
                    return;
                }

                // Добавляем трек в очередь, передавая индекс в массиве
                _musicPlayer.AddTracksToQueueByIndices(new int[] { index });

                await DisplayAlert("Информация", $"Трек '{track.Title}' добавлен в очередь.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }
    }
}
