using System;
using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace WalkerMaps
{
    public class CustomMap : Map
    {
        public List<CustomPin> CustomPins { get; set; }
        public bool MyLocationEnabled { get; set; } = false;

        public event EventHandler<Position> MapClicked;
        public void SendMapClick(object Sender, Position e)
        {
            MapClicked?.Invoke(this, e);
        }

        public event EventHandler CameraChanged;
        public void SendCameraChanged(object Sender, EventArgs e)
        {
            CameraChanged?.Invoke(this, e);
        }
    }
}
