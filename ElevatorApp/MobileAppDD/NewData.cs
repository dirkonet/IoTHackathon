using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Newtonsoft.Json;

    namespace MobileAppDD
    {
        public class NewData
        {
            public String id { get; set; }

            public Int64 date { get; set; }
            
            public float temperature { get; set; }
            public float humidity { get; set; }

            public Int64 brightness { get; set; }
            public Int64 sound { get; set; }
        }

        public class NewDataWrapper : Java.Lang.Object
        {
            public NewDataWrapper(NewData item)
            {
                Data = item;
            }

            public NewData Data { get; private set; }
        }
    }

