using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Net.Http.Headers;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace ShoeSizePrediction
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async Task InvokeRequestResponseService()
        {
            int gender = 0;
            if (genderentry.SelectedItem.ToString()=="Male")
            {
                gender = 1;
            }
            else
            {
                gender = 0;
            }
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "Shoe Size", "1"
                                            },
                                            {
                                                "Height", heightentry.Text
                                            },
                                            {
                                                "Weight", weightentry.Text
                                            },
                                            {
                                                "Gender", gender.ToString()
                                            },
                                }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                const string apiKey = "ZfkgdPV298CnSadow8gMciPugQXloWr4EEDd99ntazPtYngFnD4YSHaH3mYk0wuTiaFaG7a96HH17/Eownrdew=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/2bc6e03707c34a54ae0628b40a6fcb54/services/3ec7e081fc6f400892a517783f15ee89/execute?api-version=2.0&format=swagger");

                // WARNING: The 'await' statement below can result in a deadlock
                // if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false)
                // so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)

                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    var obj = JObject.Parse(result);
                    var value = obj["Results"]["output1"].Last["Scored Label Mean"]; //zero is early, one is late
                    await DisplayAlert("Your Shoe Size is", value.ToString(), "OK");
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp,
                    // which are useful for debugging the failure
                    string responseContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert(responseContent, response.Headers.ToString(), "OK");
                }
            }
        }

        private async void CalculateBut_Pressed(object sender, EventArgs e)
        {
            if (heightentry.Text!=null && weightentry.Text!=null && genderentry.SelectedItem!=null)
            {
                await InvokeRequestResponseService();
            }
            else
            {
                await DisplayAlert("Values are incorrect", "Please fill in the values", "OK");
            }
        }
    }
}
