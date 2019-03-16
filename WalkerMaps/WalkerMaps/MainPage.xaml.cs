using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace WalkerMaps
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BuildMap();
        }

        private void BuildMap()
        {

        }

        async protected override void OnAppearing()
        {
            var status = await Utils.CheckPermissions(Permission.Location);

            if (status == PermissionStatus.Granted)
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 50;
                if (locator.IsGeolocationAvailable && locator.IsGeolocationEnabled)
                {
                    try
                    {
                        customMap.IsShowingUser = true;
                        await SetLocation();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message.ToString());
                    }
                }
                else
                {
                    customMap.MyLocationEnabled = true;
                }
            }
            

            base.OnAppearing();
        }

        Position? UserPosition = null;
        private async Task SetLocation()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 50;
            if (locator.IsGeolocationAvailable && locator.IsGeolocationEnabled)
            {
                try
                {
                    var position = await locator.GetPositionAsync(new TimeSpan(10000));

                    UserPosition = new Position(position.Latitude, position.Longitude);
                    if (UserPosition.HasValue)
                    {
                        customMap.MoveToRegion(MapSpan.FromCenterAndRadius(UserPosition.Value, Distance.FromMeters(200)));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message.ToString());
                }
            }
        }
    }
}
