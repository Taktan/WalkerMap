using Npgsql;
using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            BuildDesign();
            BuildMap();
        }

        private void BuildDesign()
        {
            PageUp.TranslationY = -1000;
        }

        private void BuildMap()
        {
            customMap.CustomPins = new List<CustomPin>();
            customMap.CameraChanged += CameraChanged;
            customMap.MapClicked += MapClicked;
        }

        private void CameraChanged(object Sender, EventArgs e)
        {
            if (customMap.VisibleRegion.Radius.Kilometers < 25)
            {
                Debug.WriteLine(customMap.CustomPins.Count);

                LoadPinsFromDataBase(customMap.VisibleRegion.Center.Latitude, customMap.VisibleRegion.Center.Longitude);
            }
        }

        async private void LoadPinsFromDataBase(double Lat, double Lng)
        {
            string lat = Lat.ToString("0,0.00", new CultureInfo("en-US", false));
            string lng = Lng.ToString("0,0.00", new CultureInfo("en-US", false));

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
                command.CommandText = string.Format("SELECT name, ST_AsEWKT(point), type, rating, ST_DistanceSphere(point, ST_GeomFromEWKT('SRID=4326;POINT({0} {1})')) AS dist FROM markers WHERE ST_DWithin(point, ST_GeomFromEWKT('SRID=4326;POINT({0} {1})'), 30000, true); ", lng, lat);
                try
                {
                    NpgsqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string response = reader[1].ToString();
                        response = response.Remove(0, response.IndexOf('(') + 1);
                        response = response.Remove(response.IndexOf(')'));
                        string[] coor = response.Split(' ');
                        double Lat_ = Convert.ToDouble(coor[1], new NumberFormatInfo());
                        double Lng_ = Convert.ToDouble(coor[0], new NumberFormatInfo());
                        int objectType = -1;
                        int.TryParse(reader[1].ToString(), out objectType);
                        double objectRating = 0;
                        double.TryParse(reader[3].ToString(), out objectRating);

                        var pin = new CustomPin
                        {
                            Type = PinType.Place,
                            Position = new Position(Lat_, Lng_),
                            Label = reader[0].ToString(),
                            Address = "Рейтинг: " + objectRating,
                            Id = "object",
                            ObjectType = objectType,
                            ObjectRating = objectRating,
                            Url = ""
                        };

                        bool IsPinSet = false;

                        foreach (Pin curpin in customMap.CustomPins)
                        {
                            if (curpin.Position == pin.Position)
                            {
                                IsPinSet = true;
                            }
                        }
                        if (!IsPinSet)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                customMap.CustomPins.Add(pin);
                                customMap.Pins.Add(pin);                              
                            });
                        }
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
            });
        }
        
        async private void InsertPinToDataBase(string name, int type, string desc)
        {
            await Task.Run(() =>
            {
                string lat = MapClickedPos.Value.Latitude.ToString("0,0.00", new CultureInfo("en-US", false));
                string lng = MapClickedPos.Value.Longitude.ToString("0,0.00", new CultureInfo("en-US", false));

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
                command.CommandText = string.Format("INSERT INTO markers (point, name, type, date_create, description) VALUES (ST_GeomFromEWKT('SRID=4326;POINT({1} {0})'),'{2}', {3},now(),'{4}');", lat, lng, name, type, desc);
                try
                {
                    NpgsqlDataReader reader = command.ExecuteReader();

                    reader.Read();

                    var pin = new CustomPin
                    {
                        Type = PinType.Place,
                        Position = new Position(MapClickedPos.Value.Latitude, MapClickedPos.Value.Longitude),
                        Label = reader[0].ToString(),
                        Address = "Адрес",
                        Id = "object",
                        ObjectType = type,
                        Url = ""
                    };

                    {
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

        bool PinnableState;
        private void ButtonAddPin_Clicked(object sender, EventArgs e)
        {
            PinnableState = !PinnableState;
        }

        Position? MapClickedPos = null;
        private void MapClicked(object Sender, Position e)
        {
            MapClickedPos = e;
            AddPinForm_ChangeState();
        }
        private void Button_CancelAddingPin_Clicked(object sender, EventArgs e)
        {
            AddPinForm_ChangeState();
        }

        private void Button_ConfirmAddingPin_Clicked(object sender, EventArgs e)
        {
            if (MapClickedPos.HasValue)
            {
                InsertPinToDataBase(PickerPinType.SelectedItem.ToString(), PickerPinType.SelectedIndex, EditorPinDesc.Text);
                AddPinForm_ChangeState();
            }
        }

        private void AddPinForm_ChangeState()
        {
            if (PinnableState)
            {
                PageUp.TranslateTo(0, 0, 500, Easing.SinIn);
                PinnableState = !PinnableState;
            }
            else
            {
                PageUp.TranslateTo(0, -MainAbsoluteLatout.Height, 500, Easing.SinIn);
            }
        }
    }
}
