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
		public void UpdateAccount()
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
		public void Test_GetNewestPrice()
		{
			// ASSEMBLE

			// ACT

			// ASSERT
		}
	}
}
