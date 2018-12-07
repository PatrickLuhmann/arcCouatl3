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
			optionsBuilder.UseSqlite(@"Data Source=couatl3.db");
		}
	}
#if false

	public class ModelService
	{
		static public void Initialize()
		{
			using (var db = new CouatlContext())
			{
				db.Database.Migrate();
			}
		}

		/// <summary>
		/// Add an account to the database.
		/// </summary>
		/// <param name="acct">The account to add to the database.</param>
		static public void AddAccount(Account acct)
		{
			using (var db = new CouatlContext())
			{
				db.Accounts.Add(acct);
				db.SaveChanges();
			}
		}

		static public void UpdateAccount(Account acct)
		{
			using (var db = new CouatlContext())
			{
				db.Accounts.Attach(acct);
				db.Entry(acct).State = EntityState.Modified;
				db.SaveChanges();
			}
		}

		static public void DeleteAccount(Account acct)
		{
			using (var db = new CouatlContext())
			{
				db.Accounts.Attach(acct);
				db.Remove(acct);
				db.SaveChanges();
			}
		}

		static public List<Account> GetAccounts(bool openOnly)
		{
			// TODO: Figure out why this statement is needed.
			// If I don't get the list of Securities here, the Transactions
			// collection of the Account will have null for the Security.
			List<Account> theList;

			using (var db = new CouatlContext())
			{
				List<Security> secList = db.Securities.ToList();
				if (openOnly)
					theList = db.Accounts
						.Where(a => a.Closed == false)
						.Include(a => a.Transactions)
						.Include(a => a.Positions)
						.ToList();
				else
					theList = db.Accounts
						.Include(a => a.Transactions)
						.Include(a => a.Positions)
						.ToList();
			}
			return theList;
		}

		static public List<Transaction> GetTransactions()
		{
			List<Transaction> theList;
			using (var db = new CouatlContext())
			{
				theList = db.Transactions
					.Include(t => t.Account)
					.Include(t => t.Security)
					.ToList();
			}
			return theList;
		}

		static public decimal GetNewestPrice(Security security)
		{
			return 0;
		}
	}
#endif
	public class Account
	{
		public int AccountId { get; set; }

		public string Institution { get; set; }
		public string Name { get; set; }
		public decimal Cash { get; set; } = 0;
		//public decimal Value { get; set; }
		public bool Closed { get; set; } = false;

		public List<Transaction> Transactions { get; set; } = new List<Transaction>();
		public List<Position> Positions { get; set; } = new List<Position>();
	}

	public class Transaction
	{
		public int TransactionId { get; set; }

		public int Type { get; set; }
		public decimal Quantity { get; set; }
		public decimal Value { get; set; }
		public decimal Fee { get; set; }
		public DateTime Date { get; set; }

		public Security Security { get; set; }

		public int AccountId { get; set; }
		public Account Account { get; set; }
	}

	public class Price
	{
		public int PriceId { get; set; }

		public decimal Amount { get; set; }
		public DateTime Date { get; set; }
		public bool Closing { get; set; }

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

		public decimal Quantity { get; set; }

		public int BuyTransactionId { get; set; }
		public Transaction BuyTransaction { get; set; }
		public int SellTransactionId { get; set; }
		public Transaction SellTransaction { get; set; }
	}

	public class Position
	{
		public int PositionId { get; set; }

		public decimal Quantity { get; set; }

		public int SecurityId { get; set; }
		public Security Security { get; set; }
	}
}
