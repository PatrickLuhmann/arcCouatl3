using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Couatl3_Model;
using System.Diagnostics;

namespace UnitTest_Couatl3_Model
{
	[TestClass]
	public class UnitTest1
	{
		[ClassInitialize]
		public static void CreateAndMigrateDatabase(TestContext context)
		{
			using (var db = new CouatlContext())
			{
				db.Database.Migrate();
			}
		}

		[TestMethod]
		public void CreateAccount()
		{
			using (var db = new CouatlContext())
			{
				// I don't know if I need to do this.
				// TODO: If this is necessary, put it in the "run before each test" method.
				//db.Database.Migrate();

				int numAccountsBefore = db.Accounts.Count();

				Account newAcct = new Account
				{
					Institution = "Bank Of Tenochtitlan",
					Name = "Standard Brokerage Account",
					Closed = false
				};
				db.Accounts.Add(newAcct);

				int numRecordsChanged = db.SaveChanges();
				int numAccountsAfter = db.Accounts.Count();

				Assert.AreEqual(1, numRecordsChanged);
				Assert.AreEqual(numAccountsAfter, numAccountsBefore + 1);
				Assert.AreNotEqual(0, newAcct.AccountId);
			}
		}

		[TestMethod]
		public void CreateSecurity()
		{
			using (var db = new CouatlContext())
			{
				int numSecBefore = db.Securities.Count();

				Security newSec = new Security
				{
					Name = "Acme Inc.",
					Symbol = "ACME"
				};
				db.Securities.Add(newSec);

				int numRecordsChanged = db.SaveChanges();
				int numSecAfter = db.Securities.Count();

				Assert.AreEqual(1, numRecordsChanged);
				Assert.AreEqual(numSecAfter, numSecBefore + 1);
				Assert.AreNotEqual(0, newSec.SecurityId);
			}
		}

		[TestMethod]
		public void AddTransaction()
		{
			using (var db = new CouatlContext())
			{
				//db.Database.Migrate();
				
				Account newAcct = new Account
				{
					Institution = "Bank Of Tenochtitlan",
					Name = "Standard Brokerage Account",
					Closed = false
				};

				Transaction newXact = new Transaction
				{
					Type = 1,
					SecurityId = 100,
					Quantity = 12.34M,
					Value = 56.78M,
					Fee = 9.01M,
					Date = DateTime.Now,
					Account = newAcct
				};
				db.Transactions.Add(newXact);
				var count = db.SaveChanges();
				Debug.WriteLine("{0} records saved to database", count);
				Debug.WriteLine("ID of new account: {0}", newAcct.AccountId);
				Debug.WriteLine("ID of new transaction: {0}", newXact.TransactionId);

				Assert.AreEqual(2, count);
				Assert.AreEqual(newAcct.AccountId, newXact.AccountId);
				// Note that the List<Transactions> did not need to be explicitly allocated
				// because the relationship was created from the Transaction side.
				Assert.AreEqual(1, newAcct.Transactions.Count);
				Assert.AreEqual(newXact, newAcct.Transactions[0]);

				// Add another transaction with the already-existing account.
				newXact = new Transaction
				{
					Type = 2,
					SecurityId = 200,
					Quantity = 12.34M,
					Value = 56.78M,
					Fee = 9.01M,
					Date = DateTime.Now,
					Account = newAcct
				};
				db.Transactions.Add(newXact);
				count = db.SaveChanges();
				Debug.WriteLine("{0} records saved to database", count);
				Debug.WriteLine("ID of new account: {0}", newAcct.AccountId);
				Debug.WriteLine("ID of new transaction: {0}", newXact.TransactionId);

				Assert.AreEqual(1, count);
				Assert.AreEqual(newAcct.AccountId, newXact.AccountId);
				Assert.AreEqual(2, newAcct.Transactions.Count);
				Assert.AreEqual(newXact, newAcct.Transactions[1]);
			}
		}

		[TestMethod]
		public void AddLotAssignment()
		{
			using (var db = new CouatlContext())
			{
				//db.Database.Migrate();

				Account newAcct = new Account
				{
					Institution = "Bank Of Tenochtitlan",
					Name = "Standard Brokerage Account",
					Closed = false
				};

				Transaction newXact1 = new Transaction
				{
					Type = 1,
					SecurityId = 100,
					Quantity = 12.34M,
					Value = 56.78M,
					Fee = 9.01M,
					Date = DateTime.Now,
					Account = newAcct
				};
				db.Transactions.Add(newXact1);

				Transaction newXact2 = new Transaction
				{
					Type = 2,
					SecurityId = 200,
					Quantity = 12.34M,
					Value = 56.78M,
					Fee = 9.01M,
					Date = DateTime.Now,
					Account = newAcct
				};
				db.Transactions.Add(newXact2);

				LotAssignment newLot = new LotAssignment
				{
					Quantity = 1234.5M,
					BuyTransaction = newXact1,
					SellTransaction = newXact2
				};
				db.LotAssignments.Add(newLot);
				var count = db.SaveChanges();
				Debug.WriteLine("{0} records saved to database", count);
				Debug.WriteLine("ID of new buy transaction: {0}", newXact1.TransactionId);
				Debug.WriteLine("ID of new sell transaction: {0}", newXact2.TransactionId);
				Debug.WriteLine("ID of new lot assignment: {0}", newLot.LotAssignmentId);

				Assert.AreEqual(4, count);
				Assert.AreEqual(newAcct.AccountId, newXact1.AccountId); // redundant check
				Assert.AreEqual(newAcct.AccountId, newXact2.AccountId); // redundant check
				Assert.AreEqual(newXact1, newLot.BuyTransaction);
				Assert.AreEqual(newXact2, newLot.SellTransaction);
			}
		}

		[TestMethod]
		public void AddPrice()
		{
			using (var db = new CouatlContext())
			{
				Security newSec = new Security
				{
					Name = "Acme Inc.",
					Symbol = "ACME"
				};

				Price newPrice = new Price
				{
					Amount = 12.34M,
					Date = DateTime.Now,
					Closing = true,
					Security = newSec
				};
				db.Prices.Add(newPrice);

				var count = db.SaveChanges();

				Assert.AreEqual(2, count);
				Assert.AreEqual(newSec, newPrice.Security);
				Assert.AreEqual(newSec.SecurityId, newPrice.SecurityId);
			}
		}
	}
}
