using System.Net.Http;
using System;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using Handsify.Models;
using Newtonsoft.Json;
using static System.Collections.Specialized.BitVector32;
using System.Drawing;

namespace Handsify.utils
{
    public class APIClient
    {
        private readonly HttpClient _httpClient;
        private bool _disposed = false;
        private string sally = Environment.GetEnvironmentVariable("SALLY-ROOT-URL");

       // private UriBuilder url = new UriBuilder(Environment.GetEnvironmentVariable("SALLY-ROOT-URL"));
        public APIClient()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();

            //Console.WriteLine(("root: " + sally);



            clientHandler.UseDefaultCredentials = true;
  
            _httpClient = new HttpClient(clientHandler);
        }

        public async Task<LoggedInUserModel> ADAuthentication(string username, string password)
        {
            var url = sally + "/ADAuthentication";
            //Console.WriteLine(($"endpoint: {sally}/ADAuthentication");

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("username", username);
                request.Headers.Add("password", password);
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var user = System.Text.Json.JsonSerializer.Deserialize<LoggedInUserModel>(responseString);

                return user;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(($"Authentication post error: {ex.Message}");
                return new LoggedInUserModel();
            }
        }
    

        public async Task UpsertStation(HHStation station, List<Note> newNotes, List<int> archivedNotes, int floor, string pod)
        {
            ////Console.WriteLine(("upserting");
            ////Console.WriteLine(("target: "+  sally + "/SetStation");
            string target = sally + "/SetStation";

            var request = new
            {
                Station = station,
                Floor = floor,
                Pod = pod,
                NewNotes = newNotes,
                ArchivedNotes = archivedNotes
            };

            string json = System.Text.Json.JsonSerializer.Serialize(request);
            ////Console.WriteLine(("json content: " + json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(target, content);
                response.EnsureSuccessStatusCode();

                string responseString = await response.Content.ReadAsStringAsync();
                //return responseString;
                ////Console.WriteLine(("response: " + responseString);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(($"Error occurred: {ex.ToString()}");
            }
        }


        public async Task AddStation(HHStation station, int floor, string pod)
        {

            string target = sally + "/SetStation";

            var request = new
            {
                Station = station,
                Floor = floor,
                Pod = pod,
                NewNotes = new List<Note>(), // Match server expectations
                ArchivedNotes = new List<int>() // Match server expectations
            };

            string json = System.Text.Json.JsonSerializer.Serialize(request);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {

                HttpResponseMessage response = await _httpClient.PostAsync(target, content);
                response.EnsureSuccessStatusCode();

                string responseString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                //Console.WriteLine(($"Error occurred: {ex.ToString()}");
            }
        }

        public async Task<Pod> GetPod(string floor, string unit)
        {
            // Create the request message

            //Console.WriteLine((sally + "/get-pod");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, sally + "/get-pod");

            // Add the floor and unit to the headers
            requestMessage.Headers.Add("floor", floor);
            requestMessage.Headers.Add("unit", unit);

            try
            {
                // Send the request and get the response
                var response = await _httpClient.SendAsync(requestMessage);

               

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                var jsonResponse = await response.Content.ReadAsStringAsync();
                
                var pod = JsonConvert.DeserializeObject<Pod>(jsonResponse);


                return pod;
            }
            catch (Exception ex)
            {
                // Handle the exception if necessary
                //Console.WriteLine(("An error occurred: " + ex.Message );
                return null;
            }
        }
        public async Task<Pod> GetOperationalPod(string floor, string unit)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, sally + "/get-operational-pod");

            // Add the floor and unit to the headers
            requestMessage.Headers.Add("floor", floor);
            requestMessage.Headers.Add("unit", unit);

            try
            {
                // Send the request and get the response
                var response = await _httpClient.SendAsync(requestMessage);



                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                var jsonResponse = await response.Content.ReadAsStringAsync();
               
                var pod = JsonConvert.DeserializeObject<Pod>(jsonResponse);


                return pod;
            }
            catch (Exception ex)
            {
                // Handle the exception if necessary
                //Console.WriteLine(("An error occurred: " + ex.Message);
                return null;
            }
        }

        public async Task<string> ArchiveStation(string stationKey)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, sally + "/archive-station");

            requestMessage.Headers.Add("stationKey", stationKey);
            //Console.WriteLine(("in archive station apiclient");
            try
            {
                
                var response = await _httpClient.SendAsync(requestMessage);

                response.EnsureSuccessStatusCode();

                return "Successfully archived station " + stationKey;
            }
            catch (Exception ex)
            {
                // Handle the exception if necessary
                //Console.WriteLine(("An error occurred: " + ex.Message);
                return "an error occured when trying to archive the station " + stationKey;
            }
        }



    }

}

