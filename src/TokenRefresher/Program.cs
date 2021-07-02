using AivenEcommerce.V1.Modules.GitHub.Services;

using GitHubActionSharp;

using Octokit;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using TokenRefresher.Models;

namespace TokenRefresher
{
    enum Parameters
    {
        [Parameter("gh-token")]
        GitHubToken,

        [Parameter("meetup-apikey")]
        MeetupApiKey,

        [Parameter("meetup-apisecret")]
        MeetupApiSecret
    }

    class Program
    {

        public static HttpClient HttpClient { get; set; } = new HttpClient();
        const long REPOSITORY_ID = 382245667;

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

            await RefreshTokenMeetupAsync(gitHubService, actionContext);

            Console.WriteLine("Finish");
        }

        static async Task RefreshTokenMeetupAsync(IGitHubService gitHubService, GitHubActionContext actionContext)
        {
            Console.WriteLine("Refresh Token Meetup");

            string meetupApiKey = actionContext.GetParameter(Parameters.MeetupApiKey);
            string meetupApiSecret = actionContext.GetParameter(Parameters.MeetupApiSecret);

            string path = "refresh-tokens";
            string fileName = "meetup.json";

            var file = await gitHubService.GetFileContentAsync(REPOSITORY_ID, path, fileName);


            Token token = JsonSerializer.Deserialize<Token>(file.Content, _jsonOptions);

            Console.WriteLine("Get Token Successfully");

            HttpClient.BaseAddress = new Uri("https://secure.meetup.com");
            var request = new HttpRequestMessage(HttpMethod.Post, "/oauth2/access");

            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", meetupApiKey),
                new KeyValuePair<string, string>("client_secret", meetupApiSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", token.RefreshToken)
            };

            request.Content = new FormUrlEncodedContent(keyValues);
            var response = await HttpClient.SendAsync(request);


            token = await response.Content.ReadFromJsonAsync<Token>();

            Console.WriteLine("Refresh Token Successfully");


            await gitHubService.UpdateFileAsync(REPOSITORY_ID, path, fileName, JsonSerializer.Serialize(token, _jsonOptions));

            Console.WriteLine("Updated Token in meetup.json Successfully");


        }
    }
}
