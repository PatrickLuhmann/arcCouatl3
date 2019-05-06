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

		static public string GetSecurityNameFromId(int id)
		{
			// $$INVALID$$ means id <= 0.
			// $$NONE$$ means id is not in the db.
			string name = "$$INVALID$$";
			if (id > 0)
				using (var db = new CouatlContext())
				{
					List<Security> sec = db.Securities.Where(s => s.SecurityId == id).ToList();
					if (sec.Count == 1)
						name = sec[0].Name;
					else
						name = "$$NONE$$";
				}

			return name;
		}

		/// <summary>
		/// Returns all of the Price records in the database.
		/// </summary>
		/// <returns>List<Price> containing all Prices in the database.</Price></returns>
		static public List<Price> GetPrices()
		{
			List<Price> prices;
			using (var db = new CouatlContext())
			{
				prices = db.Prices.ToList();
			}
			return prices;
		}

		static public void AddPrice(int securityId, DateTime date, decimal amount, bool closing)
		{
			Price thePrice = new Price();
			thePrice.Amount = amount;
			thePrice.Date = date;
			thePrice.Closing = closing;
			thePrice.SecurityId = securityId;

			using (var db = new CouatlContext())
			{
				bool addNewPrice = true;

				// Check for existing price.
				List<Price> prices;
				prices = db.Prices.Where(p => p.SecurityId == securityId && p.Date == date).ToList();

				// If there is more than one price for this date, then the database is in an invalid state.
				// TODO: Gracefully handle more than one price per date?
				if (prices.Count > 1)
					throw new IndexOutOfRangeException();
				else if (prices.Count == 1)
				{
					// Remove the existing price, if necessary.
					// A non-closing price will always be removed in favor of a more-recent price.
					// A closing price will only be removed if the new price is also a closing price
					// (presumanly this means that the earlier closing price was incorrect).
					if (!prices[0].Closing || closing)
					{
						db.Attach(prices[0]);
						db.Remove(prices[0]);
					}
					else
						addNewPrice = false;
				}

				if (addNewPrice)
					db.Prices.Add(thePrice);

				db.SaveChanges();
			}
		}

		static public void AddPrice(Transaction xact)
		{
			// For a Buy, the Fee is subtracted from the Value to get the security's price.
			// For a Sell, the fee is added to the Value to get the security's price.
			decimal theAmount;
			if (xact.Type == (int)ModelService.TransactionType.Buy)
				theAmount = (xact.Value - xact.Fee) / xact.Quantity;
			else if (xact.Type == (int)ModelService.TransactionType.Sell)
				theAmount = (xact.Value + xact.Fee) / xact.Quantity;
			else
				throw new ArgumentException();

			// Prices from transactions are by definition not closing prices.
			// TODO: This is not the case for mutual funds, right? Take this into account?
			AddPrice(xact.SecurityId, xact.Date, theAmount, false);
		}

		static public decimal GetNewestPrice(Security security)
		{
			return GetNewestPrice(security.SecurityId);
		}

		static public decimal GetNewestPrice(int id)
		{
			decimal price = 0;
			List<Price> Prices = null;

			// Get the prices for the security.
			using (var db = new CouatlContext())
			{
				if (db.Securities.Find(id) != null)
					Prices = db.Prices.Where(p => p.SecurityId == id).ToList();
			}

			// If the id is bad, then throw an exception.
			if (Prices == null)
				throw new ArgumentException();

			// If there are no prices yet for this security, return 0.
			// Otherwise return the most recent value.
			if (Prices.Count > 0)
			{
				// Sort by date.
				Prices.Sort((x, y) => DateTime.Compare(x.Date, y.Date));

				//TODO: Is it possible to have multiple prices for the same date?
				// If so, prefer closing to not-closing. However, it might be that
				// the Add-Price action will take care of this for us.

				// Grab the price value of the list item in the list.
				price = Prices.Last().Amount;
			}

			return price;
		}

		static public decimal GetPrice(int secId, DateTime date)
		{
			decimal price = 0;
			List<Price> prices;
			using (var db = new CouatlContext())
			{
				if (db.Securities.Find(secId) != null)
				{
					prices = db.Prices.Where(p => p.SecurityId == secId && p.Date == date).ToList();
					if (prices.Count > 0)
						price = prices[0].Amount;
				}
			}
			return price;
		}

		static public decimal GetPrice(Security security, DateTime date)
		{
			return GetPrice(security.SecurityId, date);
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

		#region Transaction
		// TODO: How to handle a transfer in/out of a security?
		// I don't like the idea of overloading Deposit. So either have a
		// SecurityIn/Out pair, or move to DepositCash/DepositSecurity.
		public enum TransactionType
		{
			Null = 0,
			Deposit,
			Withdrawal,
			Buy,
			Sell,
			Dividend,
			StockSplit,
			HOLD_TransferIn,
			HOLD_TransferOut,
			Fee,
			Invalid
		}

		public static void AddTransaction(Account theAcct, Transaction theXact)
		{
			// TODO: Data validation here? What to do if there is a problem?
			// TODO: Or, create a new Transaction object based on the input, and have the xtor do the data validation.

			using (var db = new CouatlContext())
			{
				Position thePos;
				// TODO: Update Positions here if this is a Buy/Sell/StockSplit?
				switch (theXact.Type)
				{
					case (int)TransactionType.Buy:
						thePos = db.Positions.FirstOrDefault(p => p.AccountId == theAcct.AccountId && p.SecurityId == theXact.SecurityId);
						if (thePos == null)
						{
							thePos = new Position
							{
								AccountId = theAcct.AccountId,
								SecurityId = theXact.SecurityId,
								Quantity = theXact.Quantity,
							};
							AddPosition(theAcct, thePos);
						}
						else
						{
							// Update the database.
							thePos.Quantity += theXact.Quantity;
							UpdatePosition(thePos);

							// Update the Account object.
							int idx = theAcct.Positions.FindIndex(p => p.PositionId == thePos.PositionId);
							theAcct.Positions.RemoveAt(idx);
							theAcct.Positions.Add(thePos);
						}

						ModelService.AddPrice(theXact);

						break;
					case (int)TransactionType.Sell:
						thePos = db.Positions.FirstOrDefault(p => p.AccountId == theAcct.AccountId && p.SecurityId == theXact.SecurityId);
						if (thePos == null)
						{
							// TODO: For now, assume the user meant to show a short sell.
							thePos = new Position
							{
								AccountId = theAcct.AccountId,
								SecurityId = theXact.SecurityId,
								Quantity = -1 * theXact.Quantity,
							};
							AddPosition(theAcct, thePos);
						}
						else
						{
							// Update the database.
							thePos.Quantity -= theXact.Quantity;
							UpdatePosition(thePos);

							// Update the Account object.
							int idx = theAcct.Positions.FindIndex(p => p.PositionId == thePos.PositionId);
							theAcct.Positions.RemoveAt(idx);
							theAcct.Positions.Add(thePos);
						}

						ModelService.AddPrice(theXact);

						break;
					case (int)TransactionType.Deposit:
					case (int)TransactionType.Withdrawal:
						// Set the invalid fields for this type to default values.
						theXact.Quantity = 0;
						theXact.SecurityId = -1;
						break;
					case (int)TransactionType.Fee:
						// Set the invalid fields for this type to default values.
						theXact.Quantity = 0;
						theXact.SecurityId = -1;
						theXact.Fee = 0; // It doesn't make sense to have a fee on a fee.
						break;
				}
			}

			// Give the transaction to its account.
			theAcct.Transactions.Add(theXact);
			UpdateAccount(theAcct);
		}

		// TODO: Make this private because the user should be doing delete/add; it is too difficult to change Type.
		static private void UpdateTransaction(Transaction xact)
		{
			using (var db = new CouatlContext())
			{
				db.Transactions.Attach(xact);
				db.Entry(xact).State = EntityState.Modified;
				db.SaveChanges();
			}
		}

		static public Account DeleteTransaction(Transaction theXact)
		{
			int theAcctId = theXact.Account.AccountId;

			// TODO: Data validation here? What to do if there is a problem?

			using (var db = new CouatlContext())
			{
				Position thePos;
				// TODO: Update Positions here if this is a Buy/Sell/StockSplit?
				switch (theXact.Type)
				{
					case (int)TransactionType.Buy:
						// Find the Position that this Buy affects.
						thePos = db.Positions.FirstOrDefault(p => p.AccountId == theAcctId && p.SecurityId == theXact.SecurityId);

						// Back out the shares that were added.
						thePos.Quantity -= theXact.Quantity;

						if (thePos.Quantity == 0)
							DeletePosition(thePos);
						else
							UpdatePosition(thePos);
						break;
					case (int)TransactionType.Sell:
						// Find the Position that this Sell affects.
						thePos = db.Positions.FirstOrDefault(p => p.AccountId == theAcctId && p.SecurityId == theXact.SecurityId);

						// Add back the shares that were removed.
						thePos.Quantity += theXact.Quantity;

						// TODO: Revisit the handling of short sells.
						// It is possible that this Sell transaction was the only one for this position.
						// In that case then we want to delete the entry in the Position table.
						if (thePos.Quantity == 0)
							DeletePosition(thePos);
						else
							UpdatePosition(thePos);
						break;
				}
			}

			// Delete the transaction from the database table.
			using (var db = new CouatlContext())
			{
				// TODO: Figure out if Attach is really needed before Remove; it seems it is not.
				//db.Transactions.Attach(theXact);
				db.Transactions.Remove(theXact);
				db.SaveChanges();
			}

			// Get an updated Account object and return it to the caller,
			// so that they have the freshest version from the database.
			Account tmpAcct;
			using (var db = new CouatlContext())
			{
				List<Security> secList = db.Securities.ToList();
				tmpAcct = db.Accounts
					.Include(a => a.Transactions)
					.Include(a => a.Positions)
					.Single(a => a.AccountId == theAcctId);
			}

			return tmpAcct;
		}

		static public List<Transaction> GetTransactions()
		{
			List<Transaction> theList;
			using (var db = new CouatlContext())
			{
				theList = db.Transactions
					.Include(t => t.Account).ThenInclude(a => a.Positions)
					.ToList();
			}
			return theList;
		}
#endregion

		#region Position
		static private void AddPosition(Account theAcct, Position thePos)
		{
			theAcct.Positions.Add(thePos);
			UpdateAccount(theAcct);
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

		static private void DeletePosition(Position thePos)
		{
			using (var db = new CouatlContext())
			{
				db.Positions.Attach(thePos);
				db.Remove(thePos);
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
#endregion
	}
}
