using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using RegularExamMovieCatalog.Models;

namespace RegularExamMovieCatalog
{

    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string CreatedMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000";
        private const string Email = "ExamApril26@test.com";
        private const string Password = "ExamApril26@test.com";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI3MDVkYjMwNS1jMDQ4LTQzYTMtODI0Mi01MjEzNWI1ZDY1YzciLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjIxOjIwIiwiVXNlcklkIjoiZmE3NWI3MDQtZjY1ZS00MTFhLTYyNDktMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJFeGFtQXByaWwyNkB0ZXN0LmNvbSIsIlVzZXJOYW1lIjoiTWlyYVBvcnRhbFRlc3QyNiIsImV4cCI6MTc3NjUxNDg4MCwiaXNzIjoiTW92aWVDYXRhbG9nX0FwcF9Tb2Z0VW5pIiwiYXVkIjoiTW92aWVDYXRhbG9nX1dlYkFQSV9Tb2Z0VW5pIn0.GgLUZx8uEzq_uweVnCfDSbxKwW7-FnTegqyJ1eBhZa4";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;
            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(Email, Password);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            this.client = new RestClient(options);

        }

        public string GetJwtToken(string email, string pass)
        {
            RestClient tempClient = new RestClient(BaseUrl);
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, pass });
            RestResponse response = tempClient.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var readyResponse = JsonSerializer.Deserialize<JsonElement>(response.Content);
                string token = readyResponse.GetProperty("token").GetString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token is empty");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException("Response status code was not OK");
            }
            
        }
        [Order(1)]
        [Test]
        public void CreateNewMovieWithRequiredFields_ShouldReturnSuccess()
        {
            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            var body = new { Title = "The Devil Wears Prada", Description = "Interesting movie!" };
            request.AddJsonBody(body);

            RestResponse response = this.client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(readyResponse.Movie, Is.Not.Null);
            Assert.That(readyResponse.Movie.Id, Is.Not.Null);
            Assert.That(readyResponse.Movie.Id, Is.Not.Empty);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie created successfully!"));
            CreatedMovieId = readyResponse.Movie.Id;
        }
        [Order(2)]
        [Test]
        public void EditCreatedMovie_ShouldReturnSuccess()
        {
            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", CreatedMovieId);
            var body = new { Title = "The Devil Wears Prada 2", Description = "Very interesting movie!" };
            request.AddJsonBody(body);

            RestResponse response = this.client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnSuccess()
        {
            RestRequest request = new RestRequest("/api/Catalog/All", Method.Get);

            RestResponse response = this.client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(readyResponse, Is.Not.Empty);
            Assert.That(readyResponse.Count, Is.AtLeast(1));
        }

        [Order(4)]
        [Test]
        public void DeleteCreatedMovie_ShouldReturnSuccess()
        {
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", CreatedMovieId);

            RestResponse response = this.client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie deleted successfully!"));
        }
        [Order(5)]
        [Test]
        public void CreateNewMovieWithoutRequiredFields_ShouldReturnBadRequest()
        {
            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            var body = new { Title = "", Description = "" };
            request.AddJsonBody(body);

            RestResponse response = this.client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {
            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", -4);
            var body = new { Title = "The Devil Wears Prada 3", Description = "Great movie!" };
            request.AddJsonBody(body);

            RestResponse response = this.client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }
        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", -7);

            RestResponse response = this.client.Execute(request);
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }
        [OneTimeTearDown]
        public void TearDown()
        {
            this.client.Dispose();
        }
    }
}
