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
		public void AddAccount_Basic()
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
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			Security theSec = AddSecurity("UAAX", "Update Account Add Xact");

			List<Account> beforeAccountList = ModelService.GetAccounts(false);
			List<Transaction> beforeXactList = ModelService.GetTransactions();

			// ACT
			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Value = 67.89M,
				SecurityId = theSec.SecurityId,
			};
			theAcct.Transactions.Add(theXact);
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Value = 2.39M,
				SecurityId = theSec.SecurityId,
			};
			theAcct.Transactions.Add(theXact);

			ModelService.UpdateAccount(theAcct);

			// ASSERT
			int[] theXactID = { theAcct.Transactions[0].TransactionId, theAcct.Transactions[1].TransactionId };
			List<Account> afterAccountList = ModelService.GetAccounts(false);
			List<Transaction> afterXactList = ModelService.GetTransactions();

			Assert.AreEqual(beforeAccountList.Count, afterAccountList.Count);
			Assert.AreEqual(beforeXactList.Count + 2, afterXactList.Count);
			Transaction actXact0 = afterXactList.Find(x => x.TransactionId == theXactID[0]);
			Assert.IsNotNull(actXact0);
			Assert.AreEqual(1, actXact0.Type);
			Assert.AreEqual(67.89M, actXact0.Value);
			Assert.AreEqual(theSec.SecurityId, actXact0.SecurityId);
			Transaction actXact1 = afterXactList.Find(x => x.TransactionId == theXactID[1]);
			Assert.IsNotNull(actXact1);
			Assert.AreEqual(1, actXact1.Type);
			Assert.AreEqual(2.39M, actXact1.Value);
			Assert.AreEqual(theSec.SecurityId, actXact1.SecurityId);
		}

		[TestMethod]
		public void UpdateTransaction_NoAffectOnPositions()
		{
			// ASSEMBLE
			ModelService.Initialize();

			// Make a new account and a couple of transactions.
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			int theAcctID = theAcct.AccountId;

			//Security theSec = new Security { Name = "Awesome Inc.", Symbol = "XYZ" };

			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Value = 67.89M
			};
			theAcct.Transactions.Add(theXact);
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Value = 2.39M
			};
			theAcct.Transactions.Add(theXact);

			ModelService.UpdateAccount(theAcct);

			int[] theXactID = { theAcct.Transactions[0].TransactionId, theAcct.Transactions[1].TransactionId };
			List<Account> AccountListBefore = ModelService.GetAccounts(false);
			List<Transaction> XactListBefore = ModelService.GetTransactions();

			// ACT
			theXact.Type = (int)ModelService.TransactionType.Withdrawal;
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

			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");

			// Add a couple of transactions.
			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Value = 67.89M
			};
			theAcct.Transactions.Add(theXact);
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
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
				Value = 67.89M
			};
			theAcct.Transactions.Add(theXact);
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
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
		public void UpdateAccount_AddPosition()
		{
			// ASSEMBLE
			ModelService.Initialize();
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			Security theSec = AddSecurity("UAAP", "Update Account Add Position");

			List<Account> AccountListBefore = ModelService.GetAccounts(false);
			List<Position> PositionListBefore = ModelService.GetPositions();

			// ACT
			Position thePos = new Position
			{
				Quantity = 100,
				SecurityId = theSec.SecurityId,
			};
			theAcct.Positions.Add(thePos);
			ModelService.UpdateAccount(theAcct);

			// ASSERT
			List<Account> AccountListAfter = ModelService.GetAccounts(false);
			List<Position> PositionListAfter = ModelService.GetPositions();
			int[] thePosID = { theAcct.Positions[0].PositionId };

			Assert.AreEqual(AccountListBefore.Count, AccountListAfter.Count);
			Assert.AreEqual(100, theAcct.Positions[0].Quantity);
			Assert.AreEqual(theSec.SecurityId, theAcct.Positions[0].SecurityId);

			Assert.AreEqual(PositionListBefore.Count + 1, PositionListAfter.Count);
			Position actPos = PositionListAfter.Find(x => x.PositionId == thePosID[0]);
			Assert.AreEqual(100, actPos.Quantity);
			Assert.AreEqual(theSec.SecurityId, actPos.SecurityId);
		}

		[TestMethod]
		public void UpdatePosition()
		{
			// ASSEMBLE
			ModelService.Initialize();

			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			Security theSec = AddSecurity("UP", "Update Position");

			Position thePos = new Position
			{
				Quantity = 100,
				SecurityId = theSec.SecurityId,
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
			Assert.AreEqual(theSec.SecurityId, actAcct.Positions[0].SecurityId);
		}

		[TestMethod]
		[Ignore]
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
				SecurityId = theSec.SecurityId,
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
			updateXact.SecurityId = newSec.SecurityId;

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
			Assert.AreEqual(theSec.SecurityId, actAcct.Positions[0].SecurityId);

			List<Transaction> TransactionListAfter = ModelService.GetTransactions();
			Assert.AreEqual(TransactionListBefore.Count, TransactionListAfter.Count);
			Transaction actXact = TransactionListAfter.Find(t => t.TransactionId == baseXact.TransactionId);
			Assert.AreEqual(1, actXact.Type);
			Assert.AreEqual(100M, actXact.Quantity);
			Assert.AreEqual(6.95M, actXact.Fee);
			Assert.AreEqual(1006.95M, actXact.Value);
		}

		[TestMethod]
		[Ignore]
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

			// The position MUST have a security (foreign key constraint), so
			// make one if none exist.
			List<Security> SecurityListBefore = ModelService.GetSecurities();
			Security theSec;
			if (SecurityListBefore.Count > 0)
				theSec = SecurityListBefore[0];
			else
			{
				theSec = new Security
				{
					Name = "Xylophones Inc.",
					Symbol = "XYZ"
				};
				ModelService.AddSecurity(theSec);
			}

			// Add the first base transaction.
			Transaction baseXact = new Transaction
			{
				Date = DateTime.Now,
			};
			theAcct.Transactions.Add(baseXact);
			ModelService.UpdateAccount(theAcct);

			// Change it to a Buy, which causes a new Position to be added, then the Transaction is updated.
			// Add a new Position corresponding to the new transaction.
			// NOTE: Something strange is going on with the Security objects, which is why we use newSec instead of theSec.
			Security newSec = ModelService.GetSecurities().Find(s => s.Symbol == theSec.Symbol);
			Position thePos = new Position
			{
				Quantity = 100,
#if false
				SecurityId = newSec.SecurityId,
#else
				SecurityId = theSec.SecurityId,
#endif
		};
			theAcct.Positions.Add(thePos);
			ModelService.UpdateAccount(theAcct);

			// Now update the existing transaction to make it a Buy.
			Transaction updateXact = theAcct.Transactions.Find(t => t.TransactionId == baseXact.TransactionId);
			updateXact.Type = 3;
#if false
			// This works for its UpdateTransaction(). However, it seems that if I do this here,
			// then I MUST use SecurityId when changing the second transaction to a Buy.
			updateXact.Security = newSec;
#elif false
			updateXact.SecurityId = newSec.SecurityId;
#elif true
			updateXact.SecurityId = theSec.SecurityId;
#else
			// Leave the security what it is.
#endif
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

			List<Account> AccountListBefore = ModelService.GetAccounts(false);
			List<Transaction> TransactionListBefore = ModelService.GetTransactions();
			List<Position> PositionListBefore = ModelService.GetPositions();

			// ACT

			Transaction testXact = TransactionListBefore.Find(t => t.TransactionId == baseXact2.TransactionId);

			// Change the Type to Buy.
			testXact.Type = 3;
			
			// Set the Security.
			Security newSec2 = ModelService.GetSecurities().Find(s => s.Symbol == theSec.Symbol);
#if false
			// This doesn't work, and I don't know why.
//			testXact.Security = newSec2;
#elif false
			// This works.
			testXact.SecurityId = newSec2.SecurityId;
#elif false
			// This doesn't work, and I don't know why.
			testXact.Security = newSec;
#else
			// This doesn't work, and I don't know why.
			testXact.SecurityId = theSec.SecurityId;
#endif

			// Get the existing Position.
			// NOTE: This is different from the app code that I am trying to simulate.
			Position newPos2 = PositionListBefore.Find(p => p.SecurityId == theSec.SecurityId && p.AccountId == theAcct.AccountId);
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
			Assert.AreEqual(theSec.SecurityId, actAcct.Positions[0].SecurityId);

			List<Transaction> TransactionListAfter = ModelService.GetTransactions();
			Assert.AreEqual(TransactionListBefore.Count, TransactionListAfter.Count);
			Transaction actXact = TransactionListAfter.Find(t => t.TransactionId == testXact.TransactionId);
			Assert.AreEqual(3, actXact.Type);
			Assert.AreEqual(200M, actXact.Quantity);
			Assert.AreEqual(0.0M, actXact.Fee);
			Assert.AreEqual(2000.00M, actXact.Value);
		}

		[TestMethod]
		public void AddSecurity_Basic()
		{
			// ASSEMBLE
			List<Security> beforeSecList = ModelService.GetSecurities();

			// ACT
			string sym = "XYZ";
			string name = "Xylophones Inc.";
			Security theSec = new Security { Symbol = sym, Name = name };
			ModelService.AddSecurity(theSec);

			// ASSERT
			List<Security> afterSecList = ModelService.GetSecurities();
			Assert.AreEqual(beforeSecList.Count + 1, afterSecList.Count);
			Assert.IsTrue(theSec.SecurityId > 0);
			Security actSec = afterSecList.Find(s => s.SecurityId == theSec.SecurityId);
			Assert.AreEqual(sym, actSec.Symbol);
			Assert.AreEqual(name, actSec.Name);
		}

		[TestMethod]
		public void DeleteSecurity_Basic()
		{
			// ASSEMBLE
			List<Security> beforeSecList = ModelService.GetSecurities();
			string sym = "DSB";
			string name = "Delete Security Basic";
			Security theSec = new Security { Symbol = sym, Name = name };
			ModelService.AddSecurity(theSec);

			// ACT
			ModelService.DeleteSecurity(theSec);
			Assert.IsTrue(theSec.SecurityId > 0);

			// ASSERT
			List<Security> afterSecList = ModelService.GetSecurities();
			Assert.AreEqual(beforeSecList.Count, afterSecList.Count);
			Security actSec = afterSecList.Find(s => s.SecurityId == theSec.SecurityId);
			Assert.IsNull(actSec);
		}

		[TestMethod]
		public void GetSymbolFromId_Basic()
		{
			// ASSEMBLE
			string sym = "XYZ";
			string name = "Xylophones Inc.";
			Security theSec = new Security { Symbol = sym, Name = name };
			ModelService.AddSecurity(theSec);

			// ACT
			string actSym = ModelService.GetSymbolFromId(theSec.SecurityId);

			// ASSERT
			Assert.AreEqual(sym, actSym);
		}

		[TestMethod]
		public void GetSymbolFromId_Invalid()
		{
			// ASSEMBLE
			string expSym = "$$INVALID$$";

			// ACT
			string actSym = ModelService.GetSymbolFromId(0);

			// ASSERT
			Assert.AreEqual(expSym, actSym);
		}

		[TestMethod]
		public void GetSymbolFromId_NotFound()
		{
			// ASSEMBLE
			// Add then delete a Security so we know the ID will not be found.
			string sym = "GONE";
			string name = "So Long Corp.";
			Security theSec = new Security { Symbol = sym, Name = name };
			ModelService.AddSecurity(theSec);
			ModelService.DeleteSecurity(theSec);
			string expSym = "$$NONE$$";

			// ACT
			string actSym = ModelService.GetSymbolFromId(theSec.SecurityId);

			// ASSERT
			Assert.AreEqual(expSym, actSym);
		}

		[TestMethod]
		[Ignore]
		public void Test_GetNewestPrice()
		{
			// ASSEMBLE

			// ACT

			// ASSERT
		}

		#region Helper Functions
		Account AddAccount(string name, string inst)
		{
			Account theAcct = new Account
			{
				Name = name,
				Institution = inst,
			};
			ModelService.AddAccount(theAcct);
			return theAcct;
		}

		Security AddSecurity(string symbol, string name)
		{
			Security theSec = new Security { Symbol = symbol, Name = name };
			ModelService.AddSecurity(theSec);
			return theSec;
		}
		#endregion
	}
}
