using System;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace GrpcGreeter.Services
{
	public class GreeterService : Greeter.GreeterBase
	{
		private readonly ILogger<GreeterService> _logger;
		public GreeterService(ILogger<GreeterService> logger)
		{
			_logger = logger;
		}

		public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
		{
			return Task.FromResult(new HelloReply
			{
				Message = "Hello " + request.Name
			});
		}

		public override Task<HelloReply> SayHelloAgain(HelloRequest request, ServerCallContext context)
		{
			throw new RpcException(new Status(StatusCode.InvalidArgument, "Bad args"), new Metadata { { "testKey", "testValue" } }, "Test Message");
			throw new ArgumentNullException("Test Exception");
			return Task.FromResult(new HelloReply
			{
				Message = "Again " + request.Name
			});
		}

	}
}
