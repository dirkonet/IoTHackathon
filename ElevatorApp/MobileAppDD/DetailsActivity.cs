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
using Microsoft.WindowsAzure.MobileServices;
using Android.Graphics;

namespace MobileAppDD
{
    [Activity(Label = "@string/app_name")]
    public class DetailsActivity : Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DetailsActivity);
            var client = new MobileServiceClient(ToDoActivity.applicationURL);

            while (true)
            {
                var dataTable = client.GetTable<NewData>().Select(ab => ab).OrderByDescending(a => a.date).Take(1);

                List<NewData> all = await dataTable.ToListAsync();
                NewData last = all[0];

                var data = new string[]{

                    "Temp [°C]: " + last.temperature.ToString(),
                    "Rel. Luftf. [%]: "+ last.humidity.ToString(),
                    "Hell.: " + last.brightness.ToString(),
                    "Lautst.: "+last.sound.ToString()
                };
                // Create your application here
                var listView = FindViewById<ListView>(Resource.Id.test);

                IListAdapter adapter = new ArrayAdapter(this, Resource.Layout.TextViewItem, data);
                listView.SetAdapter(adapter);

                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}