using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using BloomHarvester.Parse.Model;
using Newtonsoft.Json;
using RestSharp;

namespace BloomHarvester.WebLibraryIntegration
{
	// Enhance: In the future, may be worth it to subclass the BloomDesktop one and take a dependency on it.
	// We could get the URLs and ApplicationID from the BlookDesktop base class. We would add our new Harvester-specific functions in our derived class.
	//
	// Right now, there's not enough common functionality to make it worthwhile. Maybe revisit this topic when the code becomes more stable.
	internal class ParseClient
	{
		// Constructors
		internal ParseClient(EnvironmentSetting environment)
		{
			string url;
			string environmentVariableName;

			switch (environment)
			{
				case EnvironmentSetting.Prod:
					url = "https://parse.bloomlibrary.org/";
					environmentVariableName = "BloomHarvesterParseAppIdProd";
					break;
				case EnvironmentSetting.Test:
					url = "https://bloom-parse-server-unittest.azurewebsites.net/parse";
					environmentVariableName = "BloomHarvesterParseAppIdTest";
					break;
				case EnvironmentSetting.Dev:
				default:
					//url = "https://dev-parse.bloomlibrary.org/";	// Theoretically, this link should work too and is preferred, but it doesn't work in the program for some reason even though it redirects correctly in the browser.
					url = "https://bloom-parse-server-develop.azurewebsites.net/parse";
					environmentVariableName = "BloomHarvesterParseAppIdDev";
					break;
				case EnvironmentSetting.Local:
					url = "http://localhost:1337/parse";
					environmentVariableName = null;
					break;
			}

			_client = new RestClient(url);

			if (environment != EnvironmentSetting.Local)
			{
				_applicationId = Environment.GetEnvironmentVariable(environmentVariableName);
			}
			else
			{
				_applicationId = "myAppId";
			}
			Debug.Assert(!String.IsNullOrWhiteSpace(_applicationId), "Parse Application ID is invalid. Retrieving books from Parse probably won't work. Consider checking your environment variables.");
		}

		// Fields and properties
		private RestClient _client;
		private string _applicationId;
		private string _sessionToken;	// Used to keep track of authentication

		// Careful! Very well might be null
		internal Logger.IMonitorLogger Logger { get; set; }


		// Methods
		/// <summary>
		/// Logs in, if necessary
		/// </summary>
		private void EnsureLogIn()
		{
			if (String.IsNullOrEmpty(_sessionToken))
			{
				LogIn();
			}
		}

		/// <summary>
		/// Sends a request to log in
		/// </summary>
		private void LogIn()
		{
			_sessionToken = String.Empty;

			var request = MakeRequest("login", Method.GET);
			request.AddParameter("username", Environment.GetEnvironmentVariable("BloomHarvesterParseUserName").ToLowerInvariant());
			request.AddParameter("password", Environment.GetEnvironmentVariable("BloomHarvesterParsePassword"));

			var response = _client.Execute(request);
			var dy = JsonConvert.DeserializeObject<dynamic>(response.Content);
			_sessionToken = dy.sessionToken; //there's also an "error" in there if it fails, but a null sessionToken tells us all we need to know
		}

		/// <summary>
		/// Makes a request of the specified type, and sets common headers and authentication tokens
		/// </summary>
		/// <param name="path">The Parse relative path, e.g. "classes/{className}"</param>
		/// <param name="requestType">e.g., GET, POST, etc.</param>
		/// <returns></returns>
		private RestRequest MakeRequest(string path, Method requestType)
		{
			var request = new RestRequest(path, requestType);
			SetCommonHeaders(request);
			return request;
		}

		private void SetCommonHeaders(RestRequest request)
		{
			request.AddHeader("X-Parse-Application-Id", _applicationId);
			if (!string.IsNullOrEmpty(_sessionToken))
			{
				request.AddHeader("X-Parse-Session-Token", _sessionToken);
			}
		}

		/// <summary>
		/// Creates an object in a Parse class (table)
		/// </summary>
		/// <param name="className">The name of the class (table). Do not prefix it with "classes/"</param>
		/// <param name="json">The JSON of the object to write</param>
		/// <exception>Throws an application exception if the request fails</exception>
		/// <returns>The response after executing the request</returns>
		internal IRestResponse CreateObject(string className, string json)
		{
			EnsureLogIn();
			var request = MakeRequest($"classes/{className}", Method.POST);
			request.AddParameter("application/json", json, ParameterType.RequestBody);

			var response = _client.Execute(request);
			CheckForResponseError(response, "Create failed.\nRequest.Json: {0}", json);

			return response;
		}

		/// <summary>
		/// Updates an object in a Parse class (table)
		/// </summary>
		/// <param name="className">The name of the class (table). Do not prefix it with "classes/"</param>
		/// <param name="json">The JSON of the object to update. It doesn't need to be the full object, just of the fields to update</param>
		/// <exception>Throws an application exception if the request fails</exception>
		/// <returns>The response after executing the request</returns>
		internal IRestResponse UpdateObject(string className, string updateJson)
		{
			EnsureLogIn();
			var request = MakeRequest($"classes/{className}", Method.PUT);
			request.AddParameter("application/json", updateJson, ParameterType.RequestBody);

			var response = _client.Execute(request);
			CheckForResponseError(response, "Update failed.\nRequest.Json: {0}", updateJson);

			return response;
		}

		/// <summary>
		/// Deletes an object in a Parse class (table)
		/// </summary>
		/// <param name="className">The name of the class (table). Do not prefix it with "classes/"</param>
		/// <param name="objectId">The objectId of the object to delte</param>
		/// <exception>Throws an application exception if the request fails</exception>
		/// <returns>The response after executing the request</returns>
		internal IRestResponse DeleteObject(string className, string objectId)
		{
			EnsureLogIn();
			var request = MakeRequest($"classes/{className}/{objectId}", Method.DELETE);

			var response = _client.Execute(request);
			CheckForResponseError(response, "Delete of object {0} failed.", objectId);

			return response;
		}

		private void CheckForResponseError(IRestResponse response, string exceptionInfoFormat, params object[] args)
		{
			bool isSuccess = ((int)response.StatusCode >= 200) && ((int)response.StatusCode <= 299);
			if (!isSuccess)
			{
				var message = new StringBuilder();

				message.AppendLine(String.Format(exceptionInfoFormat, args));
				message.AppendLine("Response.Code: " + response.StatusCode);
				message.AppendLine("Response.Uri: " + response.ResponseUri);
				message.AppendLine("Response.Description: " + response.StatusDescription);
				message.AppendLine("Response.Content: " + response.Content);
				throw new ApplicationException(message.ToString());
			}
		}

		/// <summary>
		/// Gets all rows from the Parse "books" class/table
		/// </summary>
		internal IEnumerable<Book> GetBooks()
		{
			var request = new RestRequest("classes/books", Method.GET);
			SetCommonHeaders(request);
			request.AddParameter("keys", "object_id,baseUrl");

			IEnumerable<Book> results = GetAllResults<Book>(request);
			return results;
		}

		/// <summary>
		/// // Gets all rows from the Parse "books" class/table
		/// </summary>
		internal IEnumerable<PublishedBook> GetPublishedBooksWithWarnings()
		{
			var request = new RestRequest("classes/publishedBooks", Method.GET);
			SetCommonHeaders(request);

			IEnumerable<PublishedBook> results = GetAllResults<PublishedBook>(request);
			return results;
		}

		/// <summary>
		/// Lazily gets all the results from a Parse database in chunks
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="request">The request should not include count, limit, skip, or order fields. This method will populate those in order to provide the functionality</param>
		/// <returns>Yields the results through an IEnumerable as needed</returns>
		private IEnumerable<T> GetAllResults<T>(IRestRequest request)
		{
			int numProcessed = 0;
			int totalCount = 0;
			do
			{
				request.AddParameter("count", "1");
				request.AddParameter("limit", "1000");   // The limit should probably be on the higher side. The fewer DB calls, the better, probably.
				request.AddParameter("skip", numProcessed.ToString());
				request.AddParameter("order", "updatedAt");

				Logger?.TrackEvent("ParseClient::GetAllResults Request Sent");
				var restResponse = _client.Execute(request);
				string responseJson = restResponse.Content;

				var response = JsonConvert.DeserializeObject<Parse.RestResponse<T>>(responseJson);
				totalCount = response.Count;

				var currentResultCount = response.Results.Length;
				if (currentResultCount <= 0)
				{
					break;
				}

				for (int i = 0; i < currentResultCount; ++i)
				{
					yield return response.Results[i];
				}

				numProcessed += currentResultCount;

				string message = $"GetAllResults Progress: {numProcessed} out of {totalCount}.";
				Console.Out.WriteLine(message);
				Logger?.LogVerbose(message);
			}
			while (numProcessed < totalCount);
		}

		/// <summary>
		/// Gets the Published Book that corresponds to the specified Book
		/// </summary>
		/// <param name="bookId">the objectId field of the desired Book</param>
		/// <returns>The objectId of the PublishedBook whose book pointer corresponds with the passed in bookId</returns>
		internal string GetPublishedBookByBookId(string bookId)
		{
			Logger?.TrackEvent("ParseClient::GetPublishedBookByBookId Start");
			var request = new RestRequest("classes/publishedBooks", Method.GET);
			SetCommonHeaders(request);
			request.AddParameter("limit", "1");
			request.AddParameter("where", "{ \"book\": { \"__type\":\"Pointer\",\"className\":\"books\",\"objectId\": \"" + bookId + "\"}}");

			var restResponse = _client.Execute(request);
			string responseJson = restResponse.Content;

			var response = JsonConvert.DeserializeObject<Parse.RestResponse<PublishedBook>>(responseJson);

			if (response.Results.Length <= 0)
			{
				return null;
			}
			var result = response.Results[0];

			string objectId = result.ObjectId;

			Logger?.TrackEvent("ParseClient::GetPublishedBookByBookId End - Success");
			return objectId;
		}
	}
}
