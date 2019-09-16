using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GrpsClient
{
	[Route("plain")]
	public class PlainController:Controller
	{

		private readonly AddressbookService _addressbookService;

		public PlainController(AddressbookService addressbookService)
		{
			_addressbookService = addressbookService;
		}

		[HttpGet]
		[Route("")]
		[Route("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var person = await _addressbookService.Get(id);
			return Ok(person);
		}
	}
}
