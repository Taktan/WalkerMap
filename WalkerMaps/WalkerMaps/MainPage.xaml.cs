using Npgsql;
using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
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

        async private void LoadPinsFromDataBase()
        {
            await Task.Run(() =>
            {
                NpgsqlConnection connection = new NpgsqlConnection();
                string ConnectionString = "Server=walkermap.postgres.database.azure.com; Port=5432; User Id=sotyrdnik@walkermap; Password=BatyaVoronHohol1;Database = map_db";
                try
                {
                    connection = new NpgsqlConnection(ConnectionString);
                    connection.Open();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message.ToString());
                }

                NpgsqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM markers";
                try
                {
                    NpgsqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string response = reader[9].ToString();
                        response = response.Remove(0, response.IndexOf('(') + 1);
                        response = response.Remove(response.IndexOf(')'));
                        string[] coor = response.Split(' ');
                        double Lat_ = Convert.ToDouble(coor[1], new NumberFormatInfo());
                        double Lng_ = Convert.ToDouble(coor[0], new NumberFormatInfo());

                        var pin = new CustomPin
                        {
                            Type = PinType.Place,
                            Position = new Position(Lat_, Lng_),
                            Label = reader[0].ToString(),
                            Address = "Адрес",
                            Id = "Xamarin",
                            Url = ""
                        };

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            customMap.CustomPins.Add(pin);
                            customMap.Pins.Add(pin);
                        });
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
            });
        }
        
        async private void InsertPinToDataBase()
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

        private void Button_Released(object sender, EventArgs e)
        {

        }
    }
}
