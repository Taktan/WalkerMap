using System;
using System.Collections.Generic;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Widget;
using WalkerMaps;
using WalkerMaps.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace WalkerMaps.Droid
{
    class CustomMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter
    {
        List<CustomPin> customPins;
        CustomMap formsMap;

        public CustomMapRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                NativeMap.InfoWindowClick -= OnInfoWindowClick;
            }

            if (e.NewElement != null)
            {
                formsMap = (CustomMap)e.NewElement;
                customPins = formsMap.CustomPins;
                bool MyLocationEnabled = formsMap.MyLocationEnabled;
                Control.GetMapAsync(this);
            }
        }

        protected override void OnMapReady(GoogleMap map)
        {
            base.OnMapReady(map);

            NativeMap.InfoWindowClick += OnInfoWindowClick;
            NativeMap.CameraChange += OnCameraChange;
            NativeMap.MapClick += OnMapClick;

            NativeMap.SetInfoWindowAdapter(this);
            NativeMap.MyLocationEnabled = MyLocationEnabled;
            NativeMap.UiSettings.MyLocationButtonEnabled = true;
        }

        private bool MyLocationEnabled
        {
            get
            {
                return NativeMap.MyLocationEnabled;
            }
            set
            {
                NativeMap.MyLocationEnabled = value;
            }
        }

        protected override MarkerOptions CreateMarker(Pin pin)
        {
            var marker = new MarkerOptions();
            var custom = GetCustomPinByPin(pin);
            if (custom.ObjectType == 0)
            {
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.locbre));
            }
            if (custom.ObjectType == 1)
            {
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.locurn));
            }
            if (custom.ObjectType == 2)
            {
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.loccha));
            }
            if (custom.ObjectType == 3)
            {
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.locwat));
            }
            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetTitle(pin.Label);
            marker.SetSnippet(pin.Address);

            
            return marker;
        }

        void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            var customPin = GetCustomPin(e.Marker);
            if (customPin == null)
            {
                throw new Exception("Custom pin not found");
            }

            if (!string.IsNullOrWhiteSpace(customPin.Url))
            {
                var url = Android.Net.Uri.Parse(customPin.Url);
                var intent = new Intent(Intent.ActionView, url);
                intent.AddFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
            }
        }

        public Android.Views.View GetInfoContents(Marker marker)
        {
            var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) as Android.Views.LayoutInflater;
            if (inflater != null)
            {
                Android.Views.View view;

                var customPin = GetCustomPin(marker);
                if (customPin == null)
                {
                    throw new Exception("Custom pin not found");
                }

                if (customPin.Id.ToString() == "object")
                {
                    view = inflater.Inflate(Resource.Layout.XamarinMapInfoWindow, null);
                }
                else
                {
                    view = inflater.Inflate(Resource.Layout.MapInfoWindow, null);
                }

                var infoTitle = view.FindViewById<TextView>(Resource.Id.InfoWindowTitle);
                var infoSubtitle = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitle);

                if (infoTitle != null)
                {
                    infoTitle.Text = marker.Title;
                }
                if (infoSubtitle != null)
                {
                    infoSubtitle.Text = marker.Snippet;
                }

                return view;
            }
            return null;
        }

        public Android.Views.View GetInfoWindow(Marker marker)
        {
            return null;
        }

        CustomPin GetCustomPin(Marker annotation)
        {
            var position = new Position(annotation.Position.Latitude, annotation.Position.Longitude);
            foreach (var pin in customPins)
            {
                if (pin.Position == position)
                {
                    return pin;
                }
            }
            return null;
        }
        CustomPin GetCustomPinByPin(Pin pin)
        {
            var position = new Position(pin.Position.Latitude, pin.Position.Longitude);
            foreach (var cust in customPins)
            {
                if (cust.Position == position)
                {
                    return cust;
                }
            }
            return null;
        }

        private void OnCameraChange(object sender, GoogleMap.CameraChangeEventArgs e)
        {
            if (formsMap != null && formsMap.VisibleRegion != null)
            {
                formsMap.SendCameraChanged(this, e);
            }
        }
        private void OnMapClick(object sender, GoogleMap.MapClickEventArgs e)
        {
            if (formsMap != null)
            {
                formsMap.SendMapClick(this, new Position(e.Point.Latitude, e.Point.Longitude));
            }
        }
    }
}