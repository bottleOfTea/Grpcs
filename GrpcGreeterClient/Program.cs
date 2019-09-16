using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Greet;
using Addreesbook;
using Grpc.Core;
using Grpc.Net.Client;

namespace GrpcGreeterClient
{
	class Program
	{
		private const string Address = "https://localhost:5001";
		static async Task Main(string[] args)
		{
			var channel = GrpcChannel.ForAddress(Address, new GrpcChannelOptions()
			{
				
			});
			var options = new CallOptions()
				.WithDeadline(DateTime.UtcNow.AddMinutes(1))
				.WithWaitForReady(true);
			var client = new Greeter.GreeterClient(channel);
			var reply = await client.SayHelloAsync(
				new HelloRequest {Name = "GreeterClient"}, options);

			try
			{
				var reply2 = await client.SayHelloAgainAsync(
					new HelloRequest { Name = "GreeterClient" }, options);
				Console.WriteLine("Greeting: " + reply2.Message);
				Console.WriteLine("Press any key to exit...");
			}
			catch (RpcException e)
			{
				Console.WriteLine(e);
				// ouch!
				// lets print the gRPC error message
				// which is "Length of `Name` cannot be more than 10 characters"
				Console.WriteLine(e.Status.Detail);
				// lets access the error code, which is `INVALID_ARGUMENT`
				Console.WriteLine(e.Status.StatusCode);
				// Want its int version for some reason?
				// you shouldn't actually do this, but if you need for debugging,
				// you can access `e.Status.StatusCode` which will give you `3`
				Console.WriteLine((int)e.Status.StatusCode);
				// Want to take specific action based on specific error?
				if (e.Status.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
				{
					// do your thing
				}
			}
		

			await AddressBookkOperations(channel);

			Console.ReadKey();
		}

		private static Person GeneratePerson(int number)
		{
			return new Person()
			{
				Id = number,
				Name = $"Test_{number}"
			};
		}

		private static void PrintPerson(Person person)
		{
			Console.WriteLine("Person id: " + person.Id + "Name: " + person.Name);
		}

		private static async Task CreatePerson(AddreesbookService.AddreesbookServiceClient client, Person person,
			string token)
		{
			Metadata? headers = null;
			if (token != null)
			{
				headers = new Metadata();
				headers.Add("Authorization", $"Bearer {token}");
			}

			var options = new CallOptions(headers)
				.WithDeadline(DateTime.UtcNow.AddSeconds(15))
				.WithWaitForReady(false);

			//var response = await client.AddAsync(person, headers);
			var response = await client.AddAsync(person, options);
		}
		private static async Task<string> GetToken()
		{
			Console.WriteLine($"Authenticating as {Environment.UserName}...");
			var httpClient = new HttpClient();
			var request = new HttpRequestMessage
			{
				RequestUri = new Uri($"{Address}/generateJwtToken?name={HttpUtility.UrlEncode(Environment.UserName)}"),
				Method = HttpMethod.Get,
				Version = new Version(2, 0)
			};
			var tokenResponse = await httpClient.SendAsync(request);
			tokenResponse.EnsureSuccessStatusCode();

			var token = await tokenResponse.Content.ReadAsStringAsync();
			Console.WriteLine("Successfully authenticated.");

			return token;
		}

		private static async Task AddressBookkOperations(GrpcChannel channel)
		{
			var client = new AddreesbookService.AddreesbookServiceClient(channel);

			var person1 = GeneratePerson(1);
			Console.WriteLine("Create person");
			try
			{
				await CreatePerson(client, person1, null);
			}
			catch (Grpc.Core.RpcException ex)
			{
				Console.WriteLine("Error Create Person." + Environment.NewLine + ex.ToString());
				await CreatePerson(client, person1, await GetToken());
			}

			var personResponse2 = await client.GetAsync(new GetRequest()
			{
				Id = person1.Id
			});
			Console.WriteLine("GetAsync request - Person id: " + personResponse2.Person.Id + "Name: " +
			                  personResponse2.Person.Name);

			using (var call = client.AddWithSummaryStream())
			{
				Console.WriteLine("Start server-side streaming RPC");
				foreach (var person in new[]
				{
					GeneratePerson(2),
					GeneratePerson(3),
					GeneratePerson(4)
				})
				{
					PrintPerson(person);
					await call.RequestStream.WriteAsync(person);
				}

				await call.RequestStream.CompleteAsync();
				var summary = await call;
				Console.WriteLine($"Stop server-side streaming RPC Count: {summary.Count} Time: {summary.ElapsedTime}");
			}

			using (var call = client.AddStream())
			{
				Console.WriteLine("Start bidirectional streaming RPC");

				var readTask = Task.Run(async () =>
				{
					await foreach (var response in call.ResponseStream.ReadAllAsync())
					{
						Console.WriteLine("bidirectional responce - Person id: " + response.Person.Id + "Name: " +
						                  response.Person.Name);
					}
				});

				foreach (var person in new[]
				{
					GeneratePerson(5),
					GeneratePerson(6),
					GeneratePerson(7)
				})
				{
					PrintPerson(person);
					await call.RequestStream.WriteAsync(person);
				}

				await call.RequestStream.CompleteAsync();
				await readTask;
				Console.WriteLine("Stop bidirectional  streaming RPC");
			}

			using (var call = client.GetALLStream(new GetAllRequest()))
			{
				Console.WriteLine("Start client-side streaming RPC");
				await foreach (var response in call.ResponseStream.ReadAllAsync())
				{
					PrintPerson(response.Person);
				}

				Console.WriteLine("Stop client-side streaming RPC");
			}
		}
	}
}