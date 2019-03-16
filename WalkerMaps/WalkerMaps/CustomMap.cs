using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace WalkerMaps
{
    public class CustomMap : Map
    {
        public List<CustomPin> CustomPins { get; set; }
        public bool MyLocationEnabled { get; set; } = false;
    }
}
