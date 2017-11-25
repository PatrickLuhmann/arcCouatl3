using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Couatl3_Model
{
	public class CouatlContext : DbContext
	{
	}

	public class Account
	{
		public int AccountId { get; set; }

		public string Institution { get; set; }
		public string Name { get; set; }
		public bool Closed { get; set; }
	}

	public class Transaction
	{
		public int TransactionId { get; set; }

		public int Type { get; set; }
		public int SecurityId { get; set; }
		//TODO: Make sure decimal actually works in EFCore. public decimal Quantity { get; set; }
	}

	public class Price
	{
		public int PriceId { get; set; }

	}

	public class Security
	{
		public int SecurityId { get; set; }

	}

	public class LotAssignment
	{
		public int LotAssignmentId { get; set; }

		
	}
}
