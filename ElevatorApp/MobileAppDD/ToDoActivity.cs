using System;
using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using MobileAppDD;
using System.Collections.Generic;
using System.Data.SqlClient;
using Java.Net;
using Android.Content;
#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
#endif

namespace MobileAppDD
{
    [Activity(MainLauncher = true,
               Icon = "@drawable/thyssen3", Label = "@string/app_name",
               Theme = "@style/AppTheme")]
    public class ToDoActivity : Activity
    {
        // Client reference.
        private MobileServiceClient client;

#if OFFLINE_SYNC_ENABLED
        private IMobileServiceSyncTable<ToDoItem> todoTable;

        const string localDbFilename = "localstore.db";
#endif

        // URL of the mobile app backend.
        public const string applicationURL = @"https://mobileappdd.azurewebsites.net";

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Activity_To_Do);

            CurrentPlatform.Init();

            // Create the client instance, using the mobile app backend URL.
            client = new MobileServiceClient(applicationURL);
            
            var listView = FindViewById<ListView>(Resource.Id.test);

            var data = new string[]{
                    "Aufzug 1 - Nord",
                    "Aufzug 2 - Ost",
                    "Aufzug 3 - Süd",
                    "Aufzug 4 - West",
                    "Aufzug 5 - Mitte"
                };
            IListAdapter adapter = new ArrayAdapter(this, Resource.Layout.TextViewItem, data);

            listView.SetAdapter(adapter);

            listView.ItemClick += delegate { StartActivity(typeof(DetailsActivity)); };

            while (true)
            {
                var dataTable = client.GetTable<NewData>().Select(ab => ab).OrderByDescending(a => a.date).Take(1);

                List<NewData> all = await dataTable.ToListAsync();
                NewData last = all[0];

                if (last.humidity > 75)
                {
                    break;
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }    

            // Instantiate the builder and set notification elements:
            Notification.Builder builder = new Notification.Builder(this)
                .SetContentTitle("Wartungsmeldung Luftfeuchtigkeit")
                .SetContentText("Wartung in Aufzug 3 - Süd benötigt")
                .SetDefaults(NotificationDefaults.Sound)
                .SetSmallIcon(Resource.Drawable.thyssen3);

            // Build the notification:
            Notification notification = builder.Build();

            // Get the notification manager:
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);
        }

#if OFFLINE_SYNC_ENABLED
        private async Task InitLocalStoreAsync()
        {
            var store = new MobileServiceSQLiteStore(localDbFilename);
            store.DefineTable<ToDoItem>();

            // Uses the default conflict handler, which fails on conflict
            // To use a different conflict handler, pass a parameter to InitializeAsync.
            // For more details, see http://go.microsoft.com/fwlink/?LinkId=521416
            await client.SyncContext.InitializeAsync(store);
        }

        private async Task SyncAsync(bool pullData = false)
        {
            try {
                await client.SyncContext.PushAsync();

                if (pullData) {
                    await todoTable.PullAsync("allTodoItems", todoTable.CreateQuery()); // query ID is used for incremental sync
                }
            }
            catch (Java.Net.MalformedURLException) {
                CreateAndShowDialog(new Exception("There was an error creating the Mobile Service. Verify the URL"), "Error");
            }
            catch (Exception e) {
                CreateAndShowDialog(e, "Error");
            }
        }
#endif

        //Initializes the activity menu
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.activity_main, menu);
            return true;
        }


    }
}


