using System.Collections.Generic;
using Addreesbook;

namespace GrpcGreeter
{
	public class AddressBookStorage
	{
		private List<Person> Persons { get; } = new List<Person>();

		public void Add(Person person)
		{
			Persons.Add(person);
		}

		public Person Get(int id)
		{
			return Persons.Find(person => person.Id == id);
		}

		public List<Person> GetAll()
		{
			return Persons;
		}
	}
}