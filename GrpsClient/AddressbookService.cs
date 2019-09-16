using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Addreesbook;
using Grpc.Core;

namespace GrpsClient
{
	public class AddressbookService : AddreesbookService.AddreesbookServiceClient
	{
		private readonly AddreesbookService.AddreesbookServiceClient _client;

		public AddressbookService(AddreesbookService.AddreesbookServiceClient client)
		{
			_client = client;
		}

		public async Task<Person> Get(int id)
		{
			var responce= await _client.GetAsync(new GetRequest()
			{
				Id = id
			});
			return responce.Person;
		}

		public async IAsyncEnumerable<Person> GetAllAsunc()
		{
			using (var call = _client.GetALLStream(new GetAllRequest()))
			{
				await foreach (var response in call.ResponseStream.ReadAllAsync())
				{
					yield return response.Person;
				}
			}
		}
	}
}