using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Addreesbook;
using Greet;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace GrpcGreeter.Services
{
	public class AddressbookGrpc : AddreesbookService.AddreesbookServiceBase
	{
		private readonly AddressBookStorage _addressBookStorage;

		public AddressbookGrpc(AddressBookStorage addressBookStorage)
		{
			_addressBookStorage = addressBookStorage;
		}

		[Authorize]
		public override Task<AddRequest> Add(Person request, ServerCallContext context)
		{
			_addressBookStorage.Add(request);
			return Task.FromResult(new AddRequest
			{
				Person = request,
				Execution = OperationType.Ok
			});
		}

		public override Task<GetResponce> Get(GetRequest request, ServerCallContext context)
		{
			return Task.FromResult(new GetResponce
			{
				Person = _addressBookStorage.Get(request.Id),
			});
		}

		public override async Task AddStream(IAsyncStreamReader<Person> requestStream,
			IServerStreamWriter<AddRequest> responseStream, ServerCallContext context)
		{
			await foreach (var request in requestStream.ReadAllAsync())
			{
				_addressBookStorage.Add(request);
				await responseStream.WriteAsync(new AddRequest()
				{
					Person = request,
					Execution = OperationType.Ok
				});
			}
		}

		public override async Task<AddSummary> AddWithSummaryStream(IAsyncStreamReader<Person> requestStream,
			ServerCallContext context)
		{
			var count = 0;
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			await foreach (var request in requestStream.ReadAllAsync())
			{
				count++;
				_addressBookStorage.Add(request);
			}

			stopwatch.Stop();

			return new AddSummary()
			{
				Count = count,
				ElapsedTime = (int) (stopwatch.ElapsedMilliseconds / 1000)
			};
		}

		public override async Task GetALLStream(GetAllRequest request, IServerStreamWriter<GetResponce> responseStream,
			ServerCallContext context)
		{
			foreach (var person in _addressBookStorage.GetAll())
			{
				await responseStream.WriteAsync(new GetResponce()
				{
					Person = person
				});
			}
		}
	}
}