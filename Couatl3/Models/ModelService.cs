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

		#region Account
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
		#endregion

		#region Security
		static public void AddSecurity(Security sec)
		{
			using (var db = new CouatlContext())
			{
				db.Securities.Add(sec);
				db.SaveChanges();
			}
		}

		static public void DeleteSecurity(Security sec)
		{
			using (var db = new CouatlContext())
			{
				db.Securities.Attach(sec);
				db.Securities.Remove(sec);
				db.SaveChanges();
			}
		}

		static public string GetSymbolFromId(int id)
		{
			// $$INVALID$$ means id <= 0.
			// $$NONE$$ means id is not in the db.
			string sym = "$$INVALID$$";
			if (id > 0)
				using (var db = new CouatlContext())
				{
					List<Security> sec = db.Securities.Where(s => s.SecurityId == id).ToList();
					if (sec.Count == 1)
						sym = sec[0].Symbol;
					else
						sym = "$$NONE$$";
				}

			return sym;
		}

		static public decimal GetNewestPrice(Security security)
		{
			return 0;
		}

		static public decimal GetNewestPrice(int id)
		{
			return 0;
		}

		static public List<Security> GetSecurities()
		{
			List<Security> theList;
			using (var db = new CouatlContext())
			{
				theList = db.Securities
					.ToList();
			}
			return theList;
		}
		#endregion

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

		public static void AddTransaction(Account theAcct, Transaction theXact)
		{
			// TODO: Data validation here? What to do if there is a problem?

			using (var db = new CouatlContext())
			{
				// TODO: Update Positions here if this is a Buy/Sell/StockSplit?
				switch (theXact.Type)
				{
					case (int)TransactionType.Buy:
						Position thePos = db.Positions.FirstOrDefault(p => p.AccountId == theAcct.AccountId && p.SecurityId == theXact.SecurityId);
						if (thePos == null)
						{
							thePos = new Position
							{
								AccountId = theAcct.AccountId,
								SecurityId = theXact.SecurityId,
								Quantity = theXact.Quantity,
							};
							// TODO: Refactor this to the new style.
							theAcct.Positions.Add(thePos);
							UpdateAccount(theAcct);
						}
						else
						{
							thePos.Quantity += theXact.Quantity;
							UpdatePosition(thePos);
						}
						break;
				}
			}

			// Give the transaction to its account.
			theAcct.Transactions.Add(theXact);
			UpdateAccount(theAcct);
		}

		// TODO: Make this private because the user should be doing delete/add; it is too difficult to change Type.
		static public void UpdateTransaction(Transaction xact)
		{
			using (var db = new CouatlContext())
			{
				db.Transactions.Attach(xact);
				db.Entry(xact).State = EntityState.Modified;
				db.SaveChanges();
			}
		}

		static public void DeleteTransaction(Transaction theXact)
		{
			Account theAcct = theXact.Account;

			// TODO: Data validation here? What to do if there is a problem?

			using (var db = new CouatlContext())
			{
				// TODO: Update Positions here if this is a Buy/Sell/StockSplit?
				switch (theXact.Type)
				{
					case (int)TransactionType.Buy:
						// Find the Position that this Buy affects.
						Position thePos = db.Positions.FirstOrDefault(p => p.AccountId == theAcct.AccountId && p.SecurityId == theXact.SecurityId);

						// Back out the shares that were added.
						thePos.Quantity -= theXact.Quantity;
						UpdatePosition(thePos);
						break;
				}

				// Delete the transaction from the table.
				db.Transactions.Remove(theXact);
				db.SaveChanges();
			}
		}

		static public List<Transaction> GetTransactions()
		{
			List<Transaction> theList;
			using (var db = new CouatlContext())
			{
				theList = db.Transactions
					.Include(t => t.Account)
					.ToList();
			}
			return theList;
		}

		static private void UpdatePosition(Position pos)
		{
			using (var db = new CouatlContext())
			{
				db.Positions.Attach(pos);
				db.Entry(pos).State = EntityState.Modified;
				db.SaveChanges();
			}
		}

		static public List<Position> GetPositions()
		{
			List<Position> theList;
			using (var db = new CouatlContext())
			{
				theList = db.Positions
					.ToList();
			}
			return theList;
		}
	}
}
