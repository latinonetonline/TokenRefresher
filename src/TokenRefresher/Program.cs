using AivenEcommerce.V1.Modules.GitHub.Services;

using GitHubActionSharp;

using Octokit;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using TokenRefresher.Models.Google;
using TokenRefresher.Models.Meetup;

namespace TokenRefresher
{
    enum Parameters
    {
        [Parameter("gh-token")]
        GitHubToken,

        [Parameter("meetup-apikey")]
        MeetupApiKey,

        [Parameter("meetup-apisecret")]
        MeetupApiSecret,

        [Parameter("google-clientid")]
        GoogleClientId,

        [Parameter("google-clientsecret")]
        GoogleClientSecret
    }

    class Program
    {

        public static HttpClient HttpClient { get; set; } = new HttpClient();
        const long REPOSITORY_ID = 382245667;
        const string FOLDER_PATH = "refresh-tokens";

        static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        static async Task Main(string[] args)
        {
            GitHubActionContext actionContext = new(args);
            actionContext.LoadParameters();
            string githubToken = actionContext.GetParameter(Parameters.GitHubToken);

            GitHubClient githubClient = new(new Octokit.ProductHeaderValue(nameof(TokenRefresher)));

            Octokit.Credentials basicAuth = new(githubToken);

            githubClient.Credentials = basicAuth;

            IGitHubService gitHubService = new GitHubService(githubClient);


            try
            {
                await RefreshTokenMeetupAsync(gitHubService, actionContext);
            }
            finally
            {

            }

            try
            {
                await RefreshTokenGoogleAsync(gitHubService, actionContext);
            }
            finally
            {
            }

            

            Console.WriteLine("Finish");
        }

        static async Task RefreshTokenMeetupAsync(IGitHubService gitHubService, GitHubActionContext actionContext)
        {
            Console.WriteLine("Refresh Token Meetup");

            string meetupApiKey = actionContext.GetParameter(Parameters.MeetupApiKey);
            string meetupApiSecret = actionContext.GetParameter(Parameters.MeetupApiSecret);

            string fileName = "meetup.json";

            var file = await gitHubService.GetFileContentAsync(REPOSITORY_ID, FOLDER_PATH, fileName);


            MeetupToken token = JsonSerializer.Deserialize<MeetupToken>(file.Content, _jsonOptions);

            Console.WriteLine("Get Token Successfully");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://secure.meetup.com/oauth2/access");

            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", meetupApiKey),
                new KeyValuePair<string, string>("client_secret", meetupApiSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", token.RefreshToken)
            };

            request.Content = new FormUrlEncodedContent(keyValues);
            var response = await HttpClient.SendAsync(request);


            token = await response.Content.ReadFromJsonAsync<MeetupToken>();

            Console.WriteLine("Refresh Token Successfully");


            await gitHubService.UpdateFileAsync(REPOSITORY_ID, FOLDER_PATH, fileName, JsonSerializer.Serialize(token, _jsonOptions));

            Console.WriteLine("Updated Token in meetup.json Successfully");


        }

        static async Task RefreshTokenGoogleAsync(IGitHubService gitHubService, GitHubActionContext actionContext)
        {
            Console.WriteLine("Refresh Token Google");

            string googleClientId = actionContext.GetParameter(Parameters.GoogleClientId);
            string googleClientSecret = actionContext.GetParameter(Parameters.GoogleClientSecret);

            string fileName = "google.json";

            var file = await gitHubService.GetFileContentAsync(REPOSITORY_ID, FOLDER_PATH, fileName);


            GoogleToken token = JsonSerializer.Deserialize<GoogleToken>(file.Content, _jsonOptions);

            Console.WriteLine("Get Token Successfully");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");

            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", googleClientId),
                new KeyValuePair<string, string>("client_secret", googleClientSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", token.RefreshToken)
            };

            request.Content = new FormUrlEncodedContent(keyValues);
            var response = await HttpClient.SendAsync(request);


            GoogleToken newToken = await response.Content.ReadFromJsonAsync<GoogleToken>();
            newToken.RefreshToken = token.RefreshToken;

            Console.WriteLine("Refresh Token Successfully");


            await gitHubService.UpdateFileAsync(REPOSITORY_ID, FOLDER_PATH, fileName, JsonSerializer.Serialize(newToken, _jsonOptions));

            Console.WriteLine("Updated Token in google.json Successfully");


        }
    }
}
