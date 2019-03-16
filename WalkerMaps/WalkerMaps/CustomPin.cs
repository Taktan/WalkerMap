using Xamarin.Forms.Maps;

namespace WalkerMaps
{
    public class CustomPin : Pin
    {
        public string Url { get; set; }
        public int ObjectType { get; set; }
        public double ObjectRating { get; set; }
    }
}
