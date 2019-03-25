using Couatl3.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

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
		public void UpdateAccount_AddXact2()
		{
			// ASSEMBLE
			ModelService.Initialize();
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			Security theSec = GetSecurity(0);

			List<Account> beforeAccountList = ModelService.GetAccounts(false);
			List<Transaction> beforeXactList = ModelService.GetTransactions();

			// ACT
			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Deposit,
				Value = 67.89M,
				SecurityId = theSec.SecurityId,
			};
			ModelService.AddTransaction(theAcct, theXact);
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Deposit,
				Value = 2.39M,
				SecurityId = theSec.SecurityId,
			};
			ModelService.AddTransaction(theAcct, theXact);

			// ASSERT
			int[] theXactID = { theAcct.Transactions[0].TransactionId, theAcct.Transactions[1].TransactionId };
			List<Account> afterAccountList = ModelService.GetAccounts(false);
			List<Transaction> afterXactList = ModelService.GetTransactions();

			Assert.AreEqual(beforeAccountList.Count, afterAccountList.Count);
			Assert.AreEqual(beforeXactList.Count + 2, afterXactList.Count);
			Transaction actXact0 = afterXactList.Find(x => x.TransactionId == theXactID[0]);
			Assert.IsNotNull(actXact0);
			Assert.AreEqual((int)ModelService.TransactionType.Deposit, actXact0.Type);
			Assert.AreEqual(67.89M, actXact0.Value);
			Assert.AreEqual(theSec.SecurityId, actXact0.SecurityId);
			Transaction actXact1 = afterXactList.Find(x => x.TransactionId == theXactID[1]);
			Assert.IsNotNull(actXact1);
			Assert.AreEqual((int)ModelService.TransactionType.Deposit, actXact1.Type);
			Assert.AreEqual(2.39M, actXact1.Value);
			Assert.AreEqual(theSec.SecurityId, actXact1.SecurityId);
		}

		[TestMethod]
		public void AddTransaction_BuyNewPosition()
		{
			// ASSEMBLE
			ModelService.Initialize();
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			Security theSec = AddSecurity("ATBNP", "Add Transaction Buy New Position");

			List<Account> beforeAccountList = ModelService.GetAccounts(false);
			List<Transaction> beforeXactList = ModelService.GetTransactions();
			List<Position> beforePositionList = ModelService.GetPositions();

			// ACT
			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec.SecurityId,
				Quantity = 10,
				Value = 1545.40M,
				Fee = 6.95M,
			};
			ModelService.AddTransaction(theAcct, theXact);

			// ASSERT
			List<Transaction> afterXactList = ModelService.GetTransactions();
			Assert.AreEqual(beforeXactList.Count + 1, afterXactList.Count);
			Transaction actXact0 = afterXactList.Find(x => x.TransactionId == theXact.TransactionId);
			Assert.IsNotNull(actXact0);
			Assert.AreEqual((int)ModelService.TransactionType.Buy, actXact0.Type);
			Assert.AreEqual(10, actXact0.Quantity);
			Assert.AreEqual(1545.40M, actXact0.Value);
			Assert.AreEqual(6.95M, actXact0.Fee);
			Assert.AreEqual(theSec.SecurityId, actXact0.SecurityId);

			List<Position> afterPositionList = ModelService.GetPositions();
			Assert.AreEqual(beforePositionList.Count + 1, afterPositionList.Count);
			Position actPos = afterPositionList.Find(p => p.AccountId == theAcct.AccountId);
			Assert.AreEqual(10, actPos.Quantity);
			Assert.AreEqual(theSec.SecurityId, actPos.SecurityId);

			List<Account> afterAccountList = ModelService.GetAccounts(false);
			Assert.AreEqual(beforeAccountList.Count, afterAccountList.Count);
			Account actAcct = afterAccountList.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(1, actAcct.Transactions.Count);
			Assert.AreEqual(theXact.TransactionId, actAcct.Transactions[0].TransactionId);
			Assert.AreEqual(1, actAcct.Positions.Count);
			Assert.AreEqual(actPos.PositionId, actAcct.Positions[0].PositionId);
		}

		[TestMethod]
		public void AddTransaction_BuyExistingPosition()
		{
			// ASSEMBLE
			ModelService.Initialize();
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			Security theSec = AddSecurity("ATBEP", "Add Transaction Buy Existing Position");

			List<Account> beforeAccountList = ModelService.GetAccounts(false);
			List<Transaction> beforeXactList = ModelService.GetTransactions();
			List<Position> beforePositionList = ModelService.GetPositions();

			// First xact to establish the position.
			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec.SecurityId,
				Quantity = 10,
				Value = 1545.40M,
				Fee = 6.95M,
			};
			ModelService.AddTransaction(theAcct, theXact);

			// ACT
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec.SecurityId,
				Quantity = 25,
				Value = 3920.30M,
				Fee = 6.95M,
			};
			ModelService.AddTransaction(theAcct, theXact);

			// ASSERT
			List<Transaction> afterXactList = ModelService.GetTransactions();
			Assert.AreEqual(beforeXactList.Count + 2, afterXactList.Count);
			Transaction actXact = afterXactList.Find(x => x.TransactionId == theXact.TransactionId);
			Assert.IsNotNull(actXact);
			Assert.AreEqual((int)ModelService.TransactionType.Buy, actXact.Type);
			Assert.AreEqual(25, actXact.Quantity);
			Assert.AreEqual(3920.30M, actXact.Value);
			Assert.AreEqual(6.95M, actXact.Fee);
			Assert.AreEqual(theSec.SecurityId, actXact.SecurityId);

			List<Position> afterPositionList = ModelService.GetPositions();
			Assert.AreEqual(beforePositionList.Count + 1, afterPositionList.Count);
			Position actPos = afterPositionList.Find(p => p.AccountId == theAcct.AccountId);
			Assert.AreEqual(35, actPos.Quantity);
			Assert.AreEqual(theSec.SecurityId, actPos.SecurityId);

			List<Account> afterAccountList = ModelService.GetAccounts(false);
			Assert.AreEqual(beforeAccountList.Count, afterAccountList.Count);
			Account actAcct = afterAccountList.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(2, actAcct.Transactions.Count);
			Assert.AreEqual(theXact.TransactionId, actAcct.Transactions[1].TransactionId);
			Assert.AreEqual(1, actAcct.Positions.Count);
			Assert.AreEqual(actPos.PositionId, actAcct.Positions[0].PositionId);
		}

		[TestMethod]
		public void AddTransacton_Sell()
		{
			// ASSEMBLE
			ModelService.Initialize();
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			Security theSec = AddSecurity("ATS", "Add Transaction Sell");

			List<Account> beforeAccountList = ModelService.GetAccounts(false);
			List<Transaction> beforeXactList = ModelService.GetTransactions();
			List<Position> beforePositionList = ModelService.GetPositions();

			// First xact to establish the position.
			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec.SecurityId,
				Quantity = 10,
				Value = 1545.40M,
				Fee = 6.95M,
			};
			ModelService.AddTransaction(theAcct, theXact);

			// ACT
			int sec = theSec.SecurityId;
			decimal qty = 4;
			decimal val = 365.45M;
			decimal fee = 6.95M;
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Sell,
				SecurityId = sec,
				Quantity = qty,
				Value = val,
				Fee = fee,
			};
			ModelService.AddTransaction(theAcct, theXact);

			// ASSERT
			List<Transaction> afterXactList = ModelService.GetTransactions();
			Assert.AreEqual(beforeXactList.Count + 2, afterXactList.Count);
			Transaction actXact = afterXactList.Find(x => x.TransactionId == theXact.TransactionId);
			Assert.IsNotNull(actXact);
			Assert.AreEqual((int)ModelService.TransactionType.Sell, actXact.Type);
			Assert.AreEqual(qty, actXact.Quantity);
			Assert.AreEqual(val, actXact.Value);
			Assert.AreEqual(fee, actXact.Fee);
			Assert.AreEqual(theSec.SecurityId, actXact.SecurityId);

			List<Position> afterPositionList = ModelService.GetPositions();
			Assert.AreEqual(beforePositionList.Count + 1, afterPositionList.Count);
			Position actPos = afterPositionList.Find(p => p.AccountId == theAcct.AccountId);
			Assert.AreEqual(10 - qty, actPos.Quantity);
			Assert.AreEqual(theSec.SecurityId, actPos.SecurityId);

			List<Account> afterAccountList = ModelService.GetAccounts(false);
			Assert.AreEqual(beforeAccountList.Count, afterAccountList.Count);
			Account actAcct = afterAccountList.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(2, actAcct.Transactions.Count);
			Assert.AreEqual(theXact.TransactionId, actAcct.Transactions[1].TransactionId);
			Assert.AreEqual(1, actAcct.Positions.Count);
			Assert.AreEqual(actPos.PositionId, actAcct.Positions[0].PositionId);
		}

		/// <summary>
		/// Validates the addition of deposit and withdrawal transactions to the database.
		/// These transaction types do not modify any other table outside Transactions.
		/// </summary>
		[TestMethod]
		public void AddTransaction_DepositAndWithdrawal()
		{
			// ASSEMBLE
			ModelService.Initialize();
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			//Security theSec = AddSecurity("ATD", "Add Transaction Deposit");

			List<Account> beforeAccountList = ModelService.GetAccounts(false);
			List<Transaction> beforeXactList = ModelService.GetTransactions();
			List<Position> beforePositionList = ModelService.GetPositions();

			// ACT
			int sec = -1;
			decimal qty = 0.0M;
			decimal depVal = 2425.48M;
			decimal fee = 0.0M;
			Transaction depXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Deposit,
				SecurityId = sec,
				Quantity = qty,
				Value = depVal,
				Fee = fee,
			};
			ModelService.AddTransaction(theAcct, depXact);

			decimal withVal = 855046.453M;
			Transaction withXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Withdrawal,
				SecurityId = sec,
				Quantity = qty,
				Value = withVal,
				Fee = fee,
			};
			ModelService.AddTransaction(theAcct, withXact);

			// ASSERT
			// Check the (global) Transaction table.
			List<Transaction> afterXactList = ModelService.GetTransactions();
			Assert.AreEqual(beforeXactList.Count + 2, afterXactList.Count);
			Transaction actXact0 = afterXactList.Find(x => x.TransactionId == depXact.TransactionId);
			Assert.IsNotNull(actXact0);
			Assert.AreEqual((int)ModelService.TransactionType.Deposit, actXact0.Type);
			Assert.AreEqual(qty, actXact0.Quantity);
			Assert.AreEqual(depVal, actXact0.Value);
			Assert.AreEqual(fee, actXact0.Fee);
			Assert.AreEqual(sec, actXact0.SecurityId);
			actXact0 = afterXactList.Find(x => x.TransactionId == withXact.TransactionId);
			Assert.IsNotNull(actXact0);
			Assert.AreEqual((int)ModelService.TransactionType.Withdrawal, actXact0.Type);
			Assert.AreEqual(qty, actXact0.Quantity);
			Assert.AreEqual(withVal, actXact0.Value);
			Assert.AreEqual(fee, actXact0.Fee);
			Assert.AreEqual(sec, actXact0.SecurityId);

			// No change to Positions for this transaction type.
			List<Position> afterPositionList = ModelService.GetPositions();
			Assert.AreEqual(beforePositionList.Count, afterPositionList.Count);
			//Position actPos = afterPositionList.Find(p => p.AccountId == theAcct.AccountId);
			//Assert.AreEqual(10, actPos.Quantity);
			//Assert.AreEqual(theSec.SecurityId, actPos.SecurityId);

			// Check the Transactions list in the Account.
			List<Account> afterAccountList = ModelService.GetAccounts(false);
			Assert.AreEqual(beforeAccountList.Count, afterAccountList.Count);
			Account actAcct = afterAccountList.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(2, actAcct.Transactions.Count);
			Assert.AreEqual(depXact.TransactionId, actAcct.Transactions[0].TransactionId);
			Assert.AreEqual(withXact.TransactionId, actAcct.Transactions[1].TransactionId);
			//Assert.AreEqual(1, actAcct.Positions.Count);
			//Assert.AreEqual(actPos.PositionId, actAcct.Positions[0].PositionId);
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
				Type = (int)ModelService.TransactionType.Deposit,
				Value = 67.89M
			};
			ModelService.AddTransaction(theAcct, theXact);
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Deposit,
				Value = 2.39M
			};
			ModelService.AddTransaction(theAcct, theXact);

			int[] theXactID = { theAcct.Transactions[0].TransactionId, theAcct.Transactions[1].TransactionId };
			List<Account> beforeAccountList = ModelService.GetAccounts(false);
			List<Transaction> beforeXactList = ModelService.GetTransactions();

			// ACT
			theAcct = ModelService.DeleteTransaction(theAcct.Transactions[0]);

			// ASSERT
			Assert.AreEqual(1, theAcct.Transactions.Count);
			Assert.AreEqual(theXactID[1], theAcct.Transactions[0].TransactionId);

			List<Account> afterAccountList = ModelService.GetAccounts(false);
			Account actAcct = afterAccountList.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(1, actAcct.Transactions.Count);
			Assert.AreEqual(theXactID[1], actAcct.Transactions[0].TransactionId);

			List<Transaction> afterXactList = ModelService.GetTransactions();
			Assert.AreEqual(beforeXactList.Count - 1, afterXactList.Count);
			Assert.IsNull(afterXactList.Find(x => x.TransactionId == theXactID[0]));
			Assert.IsNotNull(afterXactList.Find(x => x.TransactionId == theXactID[1]));
		}

		[TestMethod]
		public void DeleteTransaction_BuyExistingPosition()
		{
			// ASSEMBLE
			ModelService.Initialize();
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			Security theSec = AddSecurity("DTBEP", "Delete Transaction Buy Existing Position");

			List<Account> beforeAccountList = ModelService.GetAccounts(false);
			List<Transaction> beforeXactList = ModelService.GetTransactions();
			List<Position> beforePositionList = ModelService.GetPositions();

			// Two xacts to establish the position.
			Transaction oneXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec.SecurityId,
				Quantity = 10,
				Value = 1545.40M,
				Fee = 6.95M,
			};
			ModelService.AddTransaction(theAcct, oneXact);
			Transaction twoXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec.SecurityId,
				Quantity = 25,
				Value = 3920.30M,
				Fee = 6.95M,
			};
			ModelService.AddTransaction(theAcct, twoXact);

			// ACT
			Transaction testXact = ModelService.GetTransactions().Find(t => t.TransactionId == twoXact.TransactionId);
			theAcct = ModelService.DeleteTransaction(testXact);

			// ASSERT
			Assert.AreEqual(1, theAcct.Transactions.Count);
			Assert.AreEqual(oneXact.TransactionId, theAcct.Transactions[0].TransactionId);

			List<Transaction> afterXactList = ModelService.GetTransactions();
			Assert.AreEqual(beforeXactList.Count + 1, afterXactList.Count);
			Assert.IsNull(afterXactList.Find(x => x.TransactionId == twoXact.TransactionId));
			// Check the first xact just to be complete.
			Transaction actXact = afterXactList.Find(x => x.TransactionId == oneXact.TransactionId);
			Assert.IsNotNull(actXact);
			Assert.AreEqual((int)ModelService.TransactionType.Buy, actXact.Type);
			Assert.AreEqual(10, actXact.Quantity);
			Assert.AreEqual(1545.40M, actXact.Value);
			Assert.AreEqual(6.95M, actXact.Fee);
			Assert.AreEqual(theSec.SecurityId, actXact.SecurityId);

			List<Position> afterPositionList = ModelService.GetPositions();
			Assert.AreEqual(beforePositionList.Count + 1, afterPositionList.Count);
			Position actPos = afterPositionList.Find(p => p.AccountId == theAcct.AccountId);
			Assert.AreEqual(10, actPos.Quantity);
			Assert.AreEqual(theSec.SecurityId, actPos.SecurityId);

			List<Account> afterAccountList = ModelService.GetAccounts(false);
			Assert.AreEqual(beforeAccountList.Count, afterAccountList.Count);
			Account actAcct = afterAccountList.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(1, actAcct.Transactions.Count);
			Assert.AreEqual(oneXact.TransactionId, actAcct.Transactions[0].TransactionId);
			Assert.AreEqual(1, actAcct.Positions.Count);
			Assert.AreEqual(actPos.PositionId, actAcct.Positions[0].PositionId);
		}

		[TestMethod]
		public void DeleteTransaction_AllBuysForPosition()
		{
			// ASSEMBLE
			ModelService.Initialize();
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			Security theSec = AddSecurity("DTABFP", "Delete Transaction All Buys For Position");

			// Two xacts to establish the position.
			decimal one_qty = 123;
			decimal one_val = 765365.45M;
			decimal one_fee = 6.95M;
			Transaction oneXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec.SecurityId,
				Quantity = one_qty,
				Value = one_val,
				Fee = one_fee,
			};
			ModelService.AddTransaction(theAcct, oneXact);
			int one_xact = oneXact.TransactionId;

			decimal two_qty = 123;
			decimal two_val = 765365.45M;
			decimal two_fee = 6.95M;
			Transaction twoXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec.SecurityId,
				Quantity = two_qty,
				Value = two_val,
				Fee = two_fee,
			};
			ModelService.AddTransaction(theAcct, twoXact);
			int two_xact = twoXact.TransactionId;

			List<Account> beforeAccountList = ModelService.GetAccounts(false);
			List<Transaction> beforeXactList = ModelService.GetTransactions();
			List<Position> beforePositionList = ModelService.GetPositions();

			// ACT
			Transaction testXact = ModelService.GetTransactions().Find(t => t.TransactionId == one_xact);
			theAcct = ModelService.DeleteTransaction(testXact);
			testXact = ModelService.GetTransactions().Find(t => t.TransactionId == two_xact);
			theAcct = ModelService.DeleteTransaction(testXact);

			// ASSERT
			// Verify both transactions are gone.
			// Local.
			Assert.AreEqual(0, theAcct.Transactions.Count);
			// Database.
			List<Transaction> afterXactList = ModelService.GetTransactions();
			Assert.AreEqual(beforeXactList.Count - 2, afterXactList.Count);
			Assert.IsNull(afterXactList.Find(x => x.TransactionId == one_xact));
			Assert.IsNull(afterXactList.Find(x => x.TransactionId == two_xact));

			// Verify that the position is gone.
			// Local.
			Assert.AreEqual(0, theAcct.Positions.Count);
			// Database.
			List<Position> afterPositionList = ModelService.GetPositions();
			Assert.AreEqual(beforePositionList.Count - 1, afterPositionList.Count);
			Assert.IsNull(afterPositionList.Find(p => p.AccountId == theAcct.AccountId));

			List<Account> afterAccountList = ModelService.GetAccounts(false);
			Assert.AreEqual(beforeAccountList.Count, afterAccountList.Count);
			Account actAcct = afterAccountList.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(0, actAcct.Transactions.Count);
			Assert.AreEqual(0, actAcct.Positions.Count);
		}

		[TestMethod]
		public void DeleteTransacton_Sell()
		{
			// ASSEMBLE
			ModelService.Initialize();
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			Security theSec = AddSecurity("DTS", "Delete Transaction Sell");

			List<Account> beforeAccountList = ModelService.GetAccounts(false);
			List<Transaction> beforeXactList = ModelService.GetTransactions();
			List<Position> beforePositionList = ModelService.GetPositions();

			// First xact to establish the position.
			decimal buy_qty = 123;
			decimal buy_val = 765365.45M;
			decimal buy_fee = 6.95M;
			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec.SecurityId,
				Quantity = buy_qty,
				Value = buy_val,
				Fee = buy_fee,
			};
			ModelService.AddTransaction(theAcct, theXact);
			int b_xact = theXact.TransactionId;

			// Second xact is to sell some of that position.
			decimal s_qty = 4;
			decimal s_val = 365.45M;
			decimal s_fee = 7.37M;
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Sell,
				SecurityId = theSec.SecurityId,
				Quantity = s_qty,
				Value = s_val,
				Fee = s_fee,
			};
			ModelService.AddTransaction(theAcct, theXact);
			int s_xact = theXact.TransactionId;

			// ACT
			// Now delete that sell transaction.
			theAcct = ModelService.DeleteTransaction(theXact);

			// ASSERT
			Assert.AreEqual(1, theAcct.Transactions.Count);
			Assert.AreEqual(b_xact, theAcct.Transactions[0].TransactionId);

			List<Transaction> afterXactList = ModelService.GetTransactions();
			Assert.AreEqual(beforeXactList.Count + 1, afterXactList.Count);
			// Verify the Buy is still there.
			Transaction actXact = afterXactList.Find(x => x.TransactionId == b_xact);
			Assert.IsNotNull(actXact);
			Assert.AreEqual((int)ModelService.TransactionType.Buy, actXact.Type);
			Assert.AreEqual(buy_qty, actXact.Quantity);
			Assert.AreEqual(buy_val, actXact.Value);
			Assert.AreEqual(buy_fee, actXact.Fee);
			Assert.AreEqual(theSec.SecurityId, actXact.SecurityId);
			// Verify that the Sell is not there.
			actXact = afterXactList.Find(x => x.TransactionId == s_xact);
			Assert.IsNull(actXact);

			List<Position> afterPositionList = ModelService.GetPositions();
			Assert.AreEqual(beforePositionList.Count + 1, afterPositionList.Count);
			Position actPos = afterPositionList.Find(p => p.AccountId == theAcct.AccountId);
			Assert.AreEqual(buy_qty, actPos.Quantity);
			Assert.AreEqual(theSec.SecurityId, actPos.SecurityId);

			List<Account> afterAccountList = ModelService.GetAccounts(false);
			Assert.AreEqual(beforeAccountList.Count, afterAccountList.Count);
			Account actAcct = afterAccountList.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(1, actAcct.Transactions.Count);
			Assert.AreEqual(b_xact, actAcct.Transactions[0].TransactionId);
			Assert.AreEqual(1, actAcct.Positions.Count);
			Assert.AreEqual(actPos.PositionId, actAcct.Positions[0].PositionId);
		}

		[TestMethod]
		public void DeleteTransacton_EntireShortSell()
		{
			// ASSEMBLE
			ModelService.Initialize();
			Account theAcct = AddAccount("Test Account Name", "Test Institution Name");
			Security theSec = AddSecurity("DTESS", "Delete Transaction Entire Short Sell");

			// First xact to establish the short position.
			decimal s_qty = 4;
			decimal s_val = 365.45M;
			decimal s_fee = 7.37M;
			Transaction theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Sell,
				SecurityId = theSec.SecurityId,
				Quantity = s_qty,
				Value = s_val,
				Fee = s_fee,
			};
			ModelService.AddTransaction(theAcct, theXact);
			int s_xact = theXact.TransactionId;

			List<Account> beforeAccountList = ModelService.GetAccounts(false);
			List<Transaction> beforeXactList = ModelService.GetTransactions();
			List<Position> beforePositionList = ModelService.GetPositions();

			// ACT
			// Now delete that sell transaction.
			theAcct = ModelService.DeleteTransaction(theXact);

			// ASSERT
			Assert.AreEqual(0, theAcct.Transactions.Count);

			// Verify that the Sell transaction is no longer there.
			List<Transaction> afterXactList = ModelService.GetTransactions();
			Assert.AreEqual(beforeXactList.Count - 1, afterXactList.Count);
			Transaction actXact = afterXactList.Find(x => x.TransactionId == s_xact);
			Assert.IsNull(actXact);

			// Verify that the short position is no longer there.
			List<Position> afterPositionList = ModelService.GetPositions();
			Assert.AreEqual(beforePositionList.Count - 1, afterPositionList.Count);
			Position actPos = afterPositionList.Find(p => p.AccountId == theAcct.AccountId);
			Assert.IsNull(actPos);

			List<Account> afterAccountList = ModelService.GetAccounts(false);
			Assert.AreEqual(beforeAccountList.Count, afterAccountList.Count);
			Account actAcct = afterAccountList.Find(a => a.AccountId == theAcct.AccountId);
			Assert.AreEqual(0, actAcct.Transactions.Count);
			Assert.AreEqual(0, actAcct.Positions.Count);
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
			ModelService.AddTransaction(theAcct, theXact);
			theXact = new Transaction
			{
				Date = DateTime.Now,
				Type = 1,
				Value = 2.39M
			};
			ModelService.AddTransaction(theAcct, theXact);

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

		Security GetSecurity(int id)
		{
			Security sec;
			if (id > 0)
				sec = ModelService.GetSecurities().Find(s => s.SecurityId == id);
			else
			{
				List<Security> temp = ModelService.GetSecurities();
				if (temp.Count > 0)
					sec = temp[0];
				else
					sec = AddSecurity("GSNSP", "Get Security No Securities Present");
			}
			return sec;
		}
		#endregion
	}
}
