using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couatl3.Models
{
	public class CouatlContext : DbContext
	{
		public DbSet<Account> Accounts { get; set; }
		public DbSet<Transaction> Transactions { get; set; }
		public DbSet<Price> Prices { get; set; }
		public DbSet<Security> Securities { get; set; }
		public DbSet<LotAssignment> LotAssignments { get; set; }
		public DbSet<Position> Positions { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// The path is relative to the main assembly (.exe).
			optionsBuilder.UseSqlite(@"Data Source=couatl3.db")
				.EnableSensitiveDataLogging();
		}
	}

	public class Account
	{
		public int AccountId { get; set; }

		public string Institution { get; set; }
		public string Name { get; set; }
		public decimal Cash { get; set; } = 0.0M;
		//public decimal Value { get; set; }
		public bool Closed { get; set; } = false;

		public List<Transaction> Transactions { get; set; } = new List<Transaction>();
		public List<Position> Positions { get; set; } = new List<Position>();
	}

	public class Transaction
	{
		public int TransactionId { get; set; }

		public int Type { get; set; }
		public decimal Quantity { get; set; } = 0.0M;
		public decimal Value { get; set; } = 0.0M;
		public decimal Fee { get; set; } = 0.0M;
		public DateTime Date { get; set; } = DateTime.Now;

		public int? SecurityId { get; set; }
		public Security Security { get; set; }

		public int AccountId { get; set; }
		public Account Account { get; set; }
	}

	public class Price
	{
		public int PriceId { get; set; }

		public decimal Amount { get; set; } = 0.0M;
		public DateTime Date { get; set; }
		public bool Closing { get; set; } = false;

		public int SecurityId { get; set; }
		public Security Security { get; set; }
	}

	public class Security
	{
		public int SecurityId { get; set; }

		public string Name { get; set; }
		public string Symbol { get; set; }
	}

	public class LotAssignment
	{
		public int LotAssignmentId { get; set; }

		public decimal Quantity { get; set; } = 0.0M;

		public int BuyTransactionId { get; set; }
		public Transaction BuyTransaction { get; set; }
		public int SellTransactionId { get; set; }
		public Transaction SellTransaction { get; set; }
	}

	public class Position
	{
		public int PositionId { get; set; }

		public decimal Quantity { get; set; } = 0.0M;

		public int SecurityId { get; set; }
		public Security Security { get; set; }

		public int AccountId { get; set; }
		public Account Account { get; set; }
	}
}
