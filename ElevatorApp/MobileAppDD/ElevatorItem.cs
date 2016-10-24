using System;
using Newtonsoft.Json;

namespace MobileAppDD
{
    public class ElevatorItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Text { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Complete { get; set; }
    }

    public class ElevatorItemWrapper : Java.Lang.Object
    {
        public ElevatorItemWrapper(ElevatorItem item)
        {
            ElevatorItem = item;
        }

        public ElevatorItem ElevatorItem { get; private set; }
    }
}