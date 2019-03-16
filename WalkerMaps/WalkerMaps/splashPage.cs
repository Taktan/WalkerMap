using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace WalkerMaps
{
    class splashPage : ContentPage
    {
        
        Image splashImage; // переменная фона
        public splashPage() { // базовый конструктор
            NavigationPage.SetHasNavigationBar(this, false); // скрытие навигационной панели
            splashImage = new Image() { // добавляем изображение
                Source = "bg.jpg"
            };
            var sub = new AbsoluteLayout();
            AbsoluteLayout.SetLayoutFlags(splashImage,AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(splashImage, new Rectangle(0.5,0.5,AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            sub.Children.Add(splashImage);

            this.BackgroundColor = Color.FromHex("#51D4F5"); // установили фон страницы
            this.Content = sub; // Выносим созданый слой
        }
        protected override async void OnAppearing() // создается из класса ConentPage
        {
            base.OnAppearing();
            await splashImage.ScaleTo(1,3000);
            Application.Current.MainPage = new MainPage();
        }

    }
}
