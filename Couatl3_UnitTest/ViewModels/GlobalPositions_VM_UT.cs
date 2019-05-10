using Couatl3.Models;
using Couatl3.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Couatl3_UnitTest.ViewModels
{
	[TestClass]
	public class GlobalPositions_VM_UT
	{
		[TestInitialize]
		public void Init()
		{
			// For this VM we need to make sure there are no open accounts from
			// previous tests.
			List<Account> accounts = ModelService.GetAccounts(true);
			foreach (Account acct in accounts)
			{
				acct.Closed = true;
				ModelService.UpdateAccount(acct);
			}
		}

		[TestMethod]
		public void OneAccountWithNoPositions()
		{
			// ASSEMBLE
			ModelService_UT.AddAccount("Test Account Name", "Test Institution Name");

			// ACT
			GlobalPositions_VM testVM = new GlobalPositions_VM();
			List<Position> positions = testVM.Positions;

			// ASSERT
			Assert.IsNotNull(positions);
			Assert.AreEqual(0, positions.Count);
		}

		[TestMethod]
		public void OneAccountWithPositions()
		{
			// ASSEMBLE
			Account theAcct = ModelService_UT.AddAccount("Test Account Name", "Test Institution Name");
			Security theSec1 = ModelService_UT.AddSecurity("OAWP1", "One Account With Positions 1");
			decimal sec1Qty = 7.31M;
			Transaction sec1Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec1.SecurityId,
				Quantity = sec1Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct, sec1Xact);
			Security theSec2 = ModelService_UT.AddSecurity("OAWP2", "One Account With Positions 2");
			decimal sec2Qty = 5.01M;
			Transaction sec2Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec2.SecurityId,
				Quantity = sec2Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct, sec2Xact);
			
			// ACT
			GlobalPositions_VM testVM = new GlobalPositions_VM();
			List<Position> positions = testVM.Positions;

			// ASSERT
			Assert.IsNotNull(positions);
			Assert.AreEqual(2, positions.Count);
			Position actPos = positions.Find(p => p.SecurityId == theSec1.SecurityId);
			Assert.AreEqual(sec1Qty, actPos.Quantity);
			Assert.AreEqual(0, actPos.AccountId);
			Assert.AreEqual(null, actPos.Account);
			actPos = positions.Find(p => p.SecurityId == theSec2.SecurityId);
			Assert.AreEqual(sec2Qty, actPos.Quantity);
			Assert.AreEqual(0, actPos.AccountId);
			Assert.AreEqual(null, actPos.Account);
		}

		[TestMethod]
		public void TwoAccountsWithDifferentPositions()
		{
			// ASSEMBLE
			Account theAcct1 = ModelService_UT.AddAccount("Test Account Name 1", "Test Institution Name 1");
			Security theSec1_1 = ModelService_UT.AddSecurity("TAWDP1-1", "Two Accounts With Different Positions 1-1");
			decimal sec1_1Qty = 7.31M;
			Transaction sec1_1Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec1_1.SecurityId,
				Quantity = sec1_1Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct1, sec1_1Xact);
			Security theSec1_2 = ModelService_UT.AddSecurity("TAWDP1-2", "Two Accounts With Different Positions 1-2");
			decimal sec1_2Qty = 5.01M;
			Transaction sec1_2Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec1_2.SecurityId,
				Quantity = sec1_2Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct1, sec1_2Xact);

			Account theAcct2 = ModelService_UT.AddAccount("Test Account Name 2", "Test Institution Name 2");
			Security theSec2_1 = ModelService_UT.AddSecurity("TAWDP2-1", "Two Accounts With Different Positions 2-1");
			decimal sec2_1Qty = 6.14M;
			Transaction sec2_1Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec2_1.SecurityId,
				Quantity = sec2_1Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct2, sec2_1Xact);
			Security theSec2_2 = ModelService_UT.AddSecurity("TAWDP2-2", "Two Accounts With Different Positions 2-2");
			decimal sec2_2Qty = 2.08M;
			Transaction sec2_2Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec2_2.SecurityId,
				Quantity = sec2_2Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct2, sec2_2Xact);

			// ACT
			GlobalPositions_VM testVM = new GlobalPositions_VM();
			List<Position> positions = testVM.Positions;

			// ASSERT
			Assert.IsNotNull(positions);
			Assert.AreEqual(4, positions.Count);
			// from account 1
			Position actPos = positions.Find(p => p.SecurityId == theSec1_1.SecurityId);
			Assert.AreEqual(sec1_1Qty, actPos.Quantity);
			Assert.AreEqual(0, actPos.AccountId);
			Assert.AreEqual(null, actPos.Account);
			actPos = positions.Find(p => p.SecurityId == theSec1_2.SecurityId);
			Assert.AreEqual(sec1_2Qty, actPos.Quantity);
			Assert.AreEqual(0, actPos.AccountId);
			Assert.AreEqual(null, actPos.Account);
			// from account 2
			actPos = positions.Find(p => p.SecurityId == theSec2_1.SecurityId);
			Assert.AreEqual(sec2_1Qty, actPos.Quantity);
			Assert.AreEqual(0, actPos.AccountId);
			Assert.AreEqual(null, actPos.Account);
			actPos = positions.Find(p => p.SecurityId == theSec2_2.SecurityId);
			Assert.AreEqual(sec2_2Qty, actPos.Quantity);
			Assert.AreEqual(0, actPos.AccountId);
			Assert.AreEqual(null, actPos.Account);
		}

		[TestMethod]
		public void TwoAccountsWithOverlappingPositions()
		{
			// ASSEMBLE
			Account theAcct1 = ModelService_UT.AddAccount("Test Account Name 1", "Test Institution Name 1");
			Security theSec1_1 = ModelService_UT.AddSecurity("TAWDP1-1", "Two Accounts With Different Positions 1-1");
			decimal sec1_1Qty = 7.31M;
			Transaction sec1_1Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec1_1.SecurityId,
				Quantity = sec1_1Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct1, sec1_1Xact);
			Security theSec1_2 = ModelService_UT.AddSecurity("TAWDP1-2", "Two Accounts With Different Positions 1-2");
			decimal sec1_2Qty = 5.01M;
			Transaction sec1_2Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec1_2.SecurityId,
				Quantity = sec1_2Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct1, sec1_2Xact);

			Account theAcct2 = ModelService_UT.AddAccount("Test Account Name 2", "Test Institution Name 2");
			Security theSec2_1 = ModelService_UT.AddSecurity("TAWDP2-1", "Two Accounts With Different Positions 2-1");
			decimal sec2_1Qty = 6.14M;
			Transaction sec2_1Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec2_1.SecurityId,
				Quantity = sec2_1Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct2, sec2_1Xact);
			Security theSec2_2 = ModelService_UT.AddSecurity("TAWDP2-2", "Two Accounts With Different Positions 2-2");
			decimal sec2_2Qty = 2.08M;
			Transaction sec2_2Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec2_2.SecurityId,
				Quantity = sec2_2Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct2, sec2_2Xact);

			Security theSecShared = ModelService_UT.AddSecurity("TAWDP-S", "Two Accounts With Different Positions Shared");
			decimal secShared1Qty = 5.13M;
			Transaction secShared1Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSecShared.SecurityId,
				Quantity = secShared1Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct1, secShared1Xact);
			decimal secShared2Qty = 7.46M;
			Transaction secShared2Xact = new Transaction
			{
				Date = DateTime.Now,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSecShared.SecurityId,
				Quantity = secShared2Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct2, secShared2Xact);

			// ACT
			GlobalPositions_VM testVM = new GlobalPositions_VM();
			List<Position> positions = testVM.Positions;

			// ASSERT
			Assert.IsNotNull(positions);
			Assert.AreEqual(5, positions.Count);
			// from account 1
			Position actPos = positions.Find(p => p.SecurityId == theSec1_1.SecurityId);
			Assert.AreEqual(sec1_1Qty, actPos.Quantity);
			Assert.AreEqual(0, actPos.AccountId);
			Assert.AreEqual(null, actPos.Account);
			actPos = positions.Find(p => p.SecurityId == theSec1_2.SecurityId);
			Assert.AreEqual(sec1_2Qty, actPos.Quantity);
			Assert.AreEqual(0, actPos.AccountId);
			Assert.AreEqual(null, actPos.Account);
			// from account 2
			actPos = positions.Find(p => p.SecurityId == theSec2_1.SecurityId);
			Assert.AreEqual(sec2_1Qty, actPos.Quantity);
			Assert.AreEqual(0, actPos.AccountId);
			Assert.AreEqual(null, actPos.Account);
			actPos = positions.Find(p => p.SecurityId == theSec2_2.SecurityId);
			Assert.AreEqual(sec2_2Qty, actPos.Quantity);
			Assert.AreEqual(0, actPos.AccountId);
			Assert.AreEqual(null, actPos.Account);
			// shared position
			actPos = positions.Find(p => p.SecurityId == theSecShared.SecurityId);
			Assert.AreEqual(secShared1Qty + secShared2Qty, actPos.Quantity);
			Assert.AreEqual(0, actPos.AccountId);
			Assert.AreEqual(null, actPos.Account);
		}
	}
}
