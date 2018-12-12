using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Couatl3.Models
{
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

		static public void UpdateTransaction(Transaction xact)
		{
			using (var db = new CouatlContext())
			{
				db.Transactions.Attach(xact);
				db.Entry(xact).State = EntityState.Modified;
				db.SaveChanges();
			}
		}

		static public void DeleteTransaction(Transaction xact)
		{
			Account acct = xact.Account;

			using (var db = new CouatlContext())
			{
				db.Transactions.Remove(xact);
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

		static public List<Position> GetPositions()
		{
			List<Position> theList;
			using (var db = new CouatlContext())
			{
				theList = db.Positions
					.Include(p => p.Security)
					.ToList();
			}
			return theList;
		}

		static public void AddSecurity(Security sec)
		{
			using (var db = new CouatlContext())
			{
				db.Securities.Add(sec);
				db.SaveChanges();
			}
		}

		static public decimal GetNewestPrice(Security security)
		{
			return 0;
		}

		public enum TransactionType
		{
			Null = 0,
			Deposit,
			Withdrawal,
			Buy,
			Sell,
			Dividend,
			StockSplit,
			Invalid
		}
	}
}
