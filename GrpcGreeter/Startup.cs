using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GrpcGreeter.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace GrpcGreeter
{
	public class Startup
	{
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddGrpc();

			services.AddSingleton<AddressBookStorage>();

			services.AddAuthorization(options =>
			{
				options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
				{
					policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
					policy.RequireClaim(ClaimTypes.Name);
				});
			});
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters =
						new TokenValidationParameters
						{
							ValidateAudience = false,
							ValidateIssuer = false,
							ValidateActor = false,
							ValidateLifetime = true,
							IssuerSigningKey = SecurityKey
						};
				});
		}

		private readonly SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());


		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGrpcService<GreeterService>();
				endpoints.MapGrpcService<AddressbookGrpc>();

				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
				});


				endpoints.MapGet("/generateJwtToken", context =>
				{
					return context.Response.WriteAsync(GenerateJwtToken(context.Request.Query["name"]));
				});
			});
		}

		private string GenerateJwtToken(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new InvalidOperationException("Name is not specified.");
			}

			var claims = new[] { new Claim(ClaimTypes.Name, name) };
			var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
			var token = new JwtSecurityToken("ExampleServer", "ExampleClients", claims, expires: DateTime.Now.AddSeconds(60), signingCredentials: credentials);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}

	}
}
