using System;
using System.Collections.Generic;
using Couatl3.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Couatl3_UnitTest
{
	[TestClass]
	public class ModelService_UT
	{
		[TestMethod]
		public void AddAccount()
		{
			// ASSEMBLE
			ModelService.Initialize();
			List<Account> beforeAccounts = ModelService.GetAccounts(false);

			Account theAcct = new Account
			{
				Name = "Test Account Name",
				Institution = "Test Institution Name",
			};

			// ACT
			ModelService.AddAccount(theAcct);

			// ASSERT
			Assert.IsTrue(theAcct.AccountId > 0);

			List<Account> afterAccounts = ModelService.GetAccounts(false);
			Assert.AreEqual(beforeAccounts.Count + 1, afterAccounts.Count);
			Assert.IsNull(beforeAccounts.Find(a => a.AccountId == theAcct.AccountId));
			Assert.IsNotNull(afterAccounts.Find(a => a.AccountId == theAcct.AccountId));
		}

		[TestMethod]
		public void UpdateAccount_ChangeNames()
		{
			// ASSEMBLE
			ModelService.Initialize();
			string newName = "Changed Name";
			string newInst = "Changed Institution";

			// Add a new account to the database.
			Account theAcct = new Account
			{
				Name = "Test Account Name",
				Institution = "Test Institution Name",
			};
			ModelService.AddAccount(theAcct);

			// ACT
			theAcct.Name = newName;
			theAcct.Institution = newInst;
			ModelService.UpdateAccount(theAcct);

			// ASSERT
			List<Account> afterAccounts = ModelService.GetAccounts(false);
			Account testAcct = afterAccounts.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(newName, testAcct.Name);
			Assert.AreEqual(newInst, testAcct.Institution);
		}

		[TestMethod]
		public void UpdateAccount_AddXact()
		{
			// ASSEMBLE
			ModelService.Initialize();

			// Add a new account to the database.
			Account theAcct = new Account
			{
				Name = "Test Account Name",
				Institution = "Test Institution Name",
			};
			ModelService.AddAccount(theAcct);
			List<Account> AccountListBefore = ModelService.GetAccounts(false);
			List<Transaction> XactListBefore = ModelService.GetTransactions();

			// ACT
			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Security = null,
				Value = 67.89M
			};
			theAcct.Transactions.Add(theXact);
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Security = null,
				Value = 2.39M
			};
			theAcct.Transactions.Add(theXact);

			ModelService.UpdateAccount(theAcct);

			// ASSERT
			int[] theXactID = { theAcct.Transactions[0].TransactionId, theAcct.Transactions[1].TransactionId };
			List<Account> AccountListAfter = ModelService.GetAccounts(false);
			List<Transaction> XactListAfter = ModelService.GetTransactions();

			Assert.AreEqual(AccountListBefore.Count, AccountListAfter.Count);
			Assert.AreEqual(XactListBefore.Count + 2, XactListAfter.Count);
			Assert.IsNotNull(XactListAfter.Find(x => x.TransactionId == theXactID[0]));
			Assert.IsNotNull(XactListAfter.Find(x => x.TransactionId == theXactID[1]));
		}

		[TestMethod]
		public void UpdateAccount_AddPosition()
		{
			// ASSEMBLE
			ModelService.Initialize();

			// Add a new account to the database.
			Account theAcct = new Account
			{
				Name = "Test Account Name",
				Institution = "Test Institution Name",
			};
			ModelService.AddAccount(theAcct);
			List<Account> AccountListBefore = ModelService.GetAccounts(false);
			List<Position> PositionListBefore = ModelService.GetPositions();

			// The position MUST have a security (foreign key constraint).
			Security theSec = new Security
			{
				Name = "Xylophones Inc.",
				Symbol = "XYZ"
			};
			ModelService.AddSecurity(theSec);

			// ACT
			Position thePos = new Position
			{
				Quantity = 100,
				Security = theSec,
			};
			theAcct.Positions.Add(thePos);
			ModelService.UpdateAccount(theAcct);

			// ASSERT
			List<Account> AccountListAfter = ModelService.GetAccounts(false);
			List<Position> PositionListAfter = ModelService.GetPositions();
			int[] thePosID = { theAcct.Positions[0].PositionId };

			Assert.AreEqual(AccountListBefore.Count, AccountListAfter.Count);
			Assert.AreEqual(PositionListBefore.Count + 1, PositionListAfter.Count);
			Assert.IsNotNull(PositionListAfter.Find(x => x.PositionId == thePosID[0]));
		}

		[TestMethod]
		public void UpdateTransaction()
		{
			// ASSEMBLE
			ModelService.Initialize();

			// Make a new account and a couple of transactions.
			Account theAcct = new Account { Name = "Delete Account", Institution = "Unit Test" };
			ModelService.AddAccount(theAcct);
			int theAcctID = theAcct.AccountId;

			//Security theSec = new Security { Name = "Awesome Inc.", Symbol = "XYZ" };

			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Security = null,
				Value = 67.89M
			};
			theAcct.Transactions.Add(theXact);
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Security = null,
				Value = 2.39M
			};
			theAcct.Transactions.Add(theXact);

			ModelService.UpdateAccount(theAcct);

			int[] theXactID = { theAcct.Transactions[0].TransactionId, theAcct.Transactions[1].TransactionId };
			List<Account> AccountListBefore = ModelService.GetAccounts(false);
			List<Transaction> XactListBefore = ModelService.GetTransactions();

			// ACT
			theXact.Type = 2;
			theXact.Value = 9.05M;
			ModelService.UpdateTransaction(theXact);

			// ASSERT
			List<Transaction> XactListAfter = ModelService.GetTransactions();
			Transaction valXact = XactListAfter.Find(x => x.TransactionId == theXactID[1]);
			Assert.AreEqual(2, valXact.Type);
			Assert.AreEqual(9.05M, valXact.Value);
			Assert.AreEqual(theAcct.AccountId, valXact.Account.AccountId);
			Assert.AreEqual(XactListBefore.Count, XactListAfter.Count);
			Assert.IsNotNull(XactListAfter.Find(x => x.TransactionId == theXactID[0]));
		}

		[TestMethod]
		public void DeleteTransaction()
		{
			// ASSEMBLE
			ModelService.Initialize();

			// Add a new account to the database.
			Account theAcct = new Account
			{
				Name = "Test Account Name",
				Institution = "Test Institution Name",
			};
			ModelService.AddAccount(theAcct);

			// Add a couple of transactions.
			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Security = null,
				Value = 67.89M
			};
			theAcct.Transactions.Add(theXact);
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Security = null,
				Value = 2.39M
			};
			theAcct.Transactions.Add(theXact);
			ModelService.UpdateAccount(theAcct);

			int[] theXactID = { theAcct.Transactions[0].TransactionId, theAcct.Transactions[1].TransactionId };
			List<Account> AccountListBefore = ModelService.GetAccounts(false);
			List<Transaction> XactListBefore = ModelService.GetTransactions();

			// ACT
			ModelService.DeleteTransaction(theAcct.Transactions[0]);

			// ASSERT
			List<Account> AccountListAfter = ModelService.GetAccounts(false);
			Account testAcct = AccountListAfter.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(1, testAcct.Transactions.Count);

			List<Transaction> XactListAfter = ModelService.GetTransactions();
			Assert.AreEqual(XactListBefore.Count - 1, XactListAfter.Count);
			Assert.IsNull(XactListAfter.Find(x => x.TransactionId == theXactID[0]));
			Assert.IsNotNull(XactListAfter.Find(x => x.TransactionId == theXactID[1]));
		}

		[TestMethod]
		public void DeleteAccount()
		{
			// ASSEMBLE
			ModelService.Initialize();

			// Make a new account and a couple of transactions.
			Account theAcct = new Account { Name = "Delete Account", Institution = "Unit Test" };
			ModelService.AddAccount(theAcct);
			int theAcctID = theAcct.AccountId;

			//Security theSec = new Security { Name = "Awesome Inc.", Symbol = "XYZ" };

			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Security = null,
				Value = 67.89M
			};
			theAcct.Transactions.Add(theXact);
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Security = null,
				Value = 2.39M
			};
			theAcct.Transactions.Add(theXact);

			ModelService.UpdateAccount(theAcct);

			int[] theXactID = { theAcct.Transactions[0].TransactionId, theAcct.Transactions[1].TransactionId };
			List<Account> AccountListBefore = ModelService.GetAccounts(false);
			List<Transaction> XactListBefore = ModelService.GetTransactions();

			// ACT
			ModelService.DeleteAccount(theAcct);

			// ASSERT
			List<Account> AccountListAfter = ModelService.GetAccounts(false);
			List<Transaction> XactListAfter = ModelService.GetTransactions();
			Assert.AreEqual(AccountListBefore.Count - 1, AccountListAfter.Count);
			Assert.IsNull(AccountListAfter.Find(a => a.AccountId == theAcctID));
			Assert.AreEqual(XactListBefore.Count - 2, XactListAfter.Count);
			Assert.IsNull(XactListAfter.Find(x => x.TransactionId == theXactID[0]));
			Assert.IsNull(XactListAfter.Find(x => x.TransactionId == theXactID[1]));
		}

		[TestMethod]
		public void UpdatePosition()
		{
			// ASSEMBLE
			ModelService.Initialize();

			// Add a new account to the database.
			Account theAcct = new Account
			{
				Name = "Test Account Name",
				Institution = "Test Institution Name",
			};
			ModelService.AddAccount(theAcct);

			// The position MUST have a security (foreign key constraint).
			Security theSec = new Security
			{
				Name = "Xylophones Inc.",
				Symbol = "XYZ"
			};
			ModelService.AddSecurity(theSec);

			Position thePos = new Position
			{
				Quantity = 100,
				Security = theSec,
			};
			theAcct.Positions.Add(thePos);
			ModelService.UpdateAccount(theAcct);

			List<Account> AccountListBefore = ModelService.GetAccounts(false);
			List<Position> PositionListBefore = ModelService.GetPositions();

			// ACT
			Position testPos = theAcct.Positions[0];
			testPos.Quantity += 123.45M;
			ModelService.UpdatePosition(testPos);

			// ASSERT
			List<Account> AccountListAfter = ModelService.GetAccounts(false);
			Assert.AreEqual(AccountListBefore.Count, AccountListAfter.Count);

			List<Position> PositionListAfter = ModelService.GetPositions();
			Assert.AreEqual(PositionListBefore.Count, PositionListAfter.Count);

			Account actAcct = AccountListAfter.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(1, actAcct.Positions.Count);
			Assert.AreEqual(thePos.PositionId, actAcct.Positions[0].PositionId);
			Assert.AreEqual(thePos.Quantity, actAcct.Positions[0].Quantity);
			Assert.AreEqual(223.45M, actAcct.Positions[0].Quantity);
			Assert.AreEqual(theSec.SecurityId, actAcct.Positions[0].Security.SecurityId);
		}

		[TestMethod]
		public void UpdateTransactionToAddNewPosition()
		{
			// ASSEMBLE
			ModelService.Initialize();

			// Add a new account to the database.
			Account theAcct = new Account
			{
				Name = "Test Account Name",
				Institution = "Test Institution Name",
			};
			ModelService.AddAccount(theAcct);

			// The position MUST have a security (foreign key constraint).
			Security theSec = new Security
			{
				Name = "Xylophones Inc.",
				Symbol = "XYZ"
			};
			ModelService.AddSecurity(theSec);

			// Add the base transaction.
			Transaction baseXact = new Transaction
			{
				Date = DateTime.Now,
			};
			theAcct.Transactions.Add(baseXact);
			ModelService.UpdateAccount(theAcct);

			List<Account> AccountListBefore = ModelService.GetAccounts(false);
			List<Transaction> TransactionListBefore = ModelService.GetTransactions();
			List<Position> PositionListBefore = ModelService.GetPositions();

			// ACT
			// Add a new Position corresponding to the new transaction.
			Position thePos = new Position
			{
				Quantity = 100,
				Security = theSec,
			};
			theAcct.Positions.Add(thePos);
			ModelService.UpdateAccount(theAcct);

			// Now update the existing transaction to make it a Buy.
			Transaction updateXact = theAcct.Transactions.Find(t => t.TransactionId == baseXact.TransactionId);
			updateXact.Type = 1;
			updateXact.Quantity = 100;
			updateXact.Fee = 6.95M;
			updateXact.Value = 1006.95M;
			Security newSec = ModelService.GetSecurities().Find(s => s.Symbol == theSec.Symbol);
			updateXact.Security = newSec;

			ModelService.UpdateTransaction(updateXact);

			// ASSERT
			List<Account> AccountListAfter = ModelService.GetAccounts(false);
			Assert.AreEqual(AccountListBefore.Count, AccountListAfter.Count);

			List<Position> PositionListAfter = ModelService.GetPositions();
			Assert.AreEqual(PositionListBefore.Count + 1, PositionListAfter.Count);

			Account actAcct = AccountListAfter.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(1, actAcct.Positions.Count);
			Assert.AreEqual(thePos.PositionId, actAcct.Positions[0].PositionId);
			Assert.AreEqual(100, actAcct.Positions[0].Quantity);
			Assert.AreEqual(theSec.SecurityId, actAcct.Positions[0].Security.SecurityId);

			List<Transaction> TransactionListAfter = ModelService.GetTransactions();
			Assert.AreEqual(TransactionListBefore.Count, TransactionListAfter.Count);
			Transaction actXact = TransactionListAfter.Find(t => t.TransactionId == baseXact.TransactionId);
			Assert.AreEqual(1, actXact.Type);
			Assert.AreEqual(100M, actXact.Quantity);
			Assert.AreEqual(6.95M, actXact.Fee);
			Assert.AreEqual(1006.95M, actXact.Value);
		}

		public void UpdateTransactionToAddToExistingPosition()
		{
			// ASSEMBLE
			ModelService.Initialize();

			// Add a new account to the database.
			Account theAcct = new Account
			{
				Name = "Test Account Name",
				Institution = "Test Institution Name",
			};
			ModelService.AddAccount(theAcct);

			// The position MUST have a security (foreign key constraint).
			Security theSec = new Security
			{
				Name = "Xylophones Inc.",
				Symbol = "XYZ"
			};
			ModelService.AddSecurity(theSec);

			// Add the first base transaction.
			Transaction baseXact = new Transaction
			{
				Date = DateTime.Now,
			};
			theAcct.Transactions.Add(baseXact);
			ModelService.UpdateAccount(theAcct);

			// Change it to a Buy, which causes a new Position to be added, then the Transaction is updated.
			// Add a new Position corresponding to the new transaction.
			Security newSec = ModelService.GetSecurities().Find(s => s.Symbol == theSec.Symbol);
			Position thePos = new Position
			{
				Quantity = 100,
				Security = newSec,
			};
			theAcct.Positions.Add(thePos);
			ModelService.UpdateAccount(theAcct);

			// Now update the existing transaction to make it a Buy.
			Transaction updateXact = theAcct.Transactions.Find(t => t.TransactionId == baseXact.TransactionId);
			updateXact.Type = 3;
			updateXact.Security = newSec;
			updateXact.Date = DateTime.Now;
			updateXact.Quantity = 100;
			updateXact.Fee = 6.95M;
			updateXact.Value = 1006.95M;
			ModelService.UpdateTransaction(updateXact);

			// Add the second base transaction.
			Transaction baseXact2 = new Transaction
			{
				Date = DateTime.Now,
			};
			theAcct.Transactions.Add(baseXact2);
			ModelService.UpdateAccount(theAcct);

			// TODO: Do I need all of these?
			List<Account> AccountListBefore = ModelService.GetAccounts(false);
			List<Transaction> TransactionListBefore = ModelService.GetTransactions();
			List<Position> PositionListBefore = ModelService.GetPositions();

			// ACT

			Transaction testXact = TransactionListBefore.Find(t => t.TransactionId == baseXact2.TransactionId);

			// Change the Type to Buy.
			testXact.Type = 3;
			
			// Set the Security.
			Security newSec2 = ModelService.GetSecurities().Find(s => s.Symbol == theSec.Symbol);
			testXact.Security = newSec2;
			
			// Get the existing Position.
			// NOTE: This is different from the app code that I am trying to simulate.
			Position newPos2 = PositionListBefore.Find(p => p.Security.SecurityId == newSec.SecurityId);
			newPos2.Quantity += 200;
			ModelService.UpdatePosition(newPos2);

			// Update the transaction.
			testXact.Date = DateTime.Now;
			testXact.Quantity = 200;
			testXact.Fee = 0.0M;
			testXact.Value = 2000.00M;
			ModelService.UpdateTransaction(testXact);

			// ASSERT
			List<Account> AccountListAfter = ModelService.GetAccounts(false);
			Assert.AreEqual(AccountListBefore.Count, AccountListAfter.Count);

			List<Position> PositionListAfter = ModelService.GetPositions();
			Assert.AreEqual(PositionListBefore.Count, PositionListAfter.Count);

			Account actAcct = AccountListAfter.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(1, actAcct.Positions.Count);
			Assert.AreEqual(thePos.PositionId, actAcct.Positions[0].PositionId);
			Assert.AreEqual(300, actAcct.Positions[0].Quantity);
			// Comparing .Security doesn't work, so I just compare the ID.
			Assert.AreEqual(theSec.SecurityId, actAcct.Positions[0].Security.SecurityId);

			List<Transaction> TransactionListAfter = ModelService.GetTransactions();
			Assert.AreEqual(TransactionListBefore.Count + 1, TransactionListAfter.Count);
			Transaction actXact = TransactionListAfter.Find(t => t.TransactionId == testXact.TransactionId);
			Assert.AreEqual(1, actXact.Type);
			Assert.AreEqual(200M, actXact.Quantity);
			Assert.AreEqual(0.0M, actXact.Fee);
			Assert.AreEqual(2000.00M, actXact.Value);
		}

		[TestMethod]
		public void Test_GetNewestPrice()
		{
			// ASSEMBLE

			// ACT

			// ASSERT
		}
	}
}
