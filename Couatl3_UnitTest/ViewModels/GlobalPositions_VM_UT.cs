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
			List<GlobalPosition_VM> positions = testVM.Positions;

			// ASSERT
			Assert.IsNotNull(positions);
			Assert.AreEqual(0, positions.Count);
		}

		[TestMethod]
		public void OneAccountWithPositions()
		{
			// ASSEMBLE
			Account theAcct = ModelService_UT.AddAccount("Test Account Name", "Test Institution Name");

			// Add the transactions that create the positions.
			Security theSec1 = ModelService_UT.AddSecurity("OAWP1", "One Account With Positions 1");
			DateTime sec1Date = DateTime.Parse("2018-05-13");
			decimal sec1Qty = 7.31M;
			decimal sec1Value = 2083.49M;
			Transaction sec1Xact = new Transaction
			{
				Date = sec1Date,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec1.SecurityId,
				Quantity = sec1Qty,
				Value = sec1Value,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct, sec1Xact);
			Security theSec2 = ModelService_UT.AddSecurity("OAWP2", "One Account With Positions 2");
			DateTime sec2Date = DateTime.Parse("2018-05-13");
			decimal sec2Qty = 5.01M;
			decimal sec2Value = 61.48M;
			Transaction sec2Xact = new Transaction
			{
				Date = sec2Date,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec2.SecurityId,
				Quantity = sec2Qty,
				Value = sec2Value,
				Fee = 4.34M,
			};
			ModelService.AddTransaction(theAcct, sec2Xact);

			// Update the prices.
			decimal sec1Mrp = 2.43M;
			ModelService.AddPrice(theSec1.SecurityId, DateTime.Today, sec1Mrp, true);
			decimal sec2Mrp = 4.24M;
			ModelService.AddPrice(theSec2.SecurityId, DateTime.Today, sec2Mrp, true);

			// ACT
			GlobalPositions_VM testVM = new GlobalPositions_VM();
			List<GlobalPosition_VM> positions = testVM.Positions;

			// ASSERT
			GlobalPosition_VM actPos;
			Assert.IsNotNull(positions);
			Assert.AreEqual(2, positions.Count);
			actPos = positions.Find(p => p.ThePosition.SecurityId == theSec1.SecurityId);
			Assert.AreEqual(theSec1.Name, actPos.SecurityName);
			Assert.AreEqual(theSec1.Symbol, actPos.Symbol);
			Assert.AreEqual(sec1Qty, actPos.ThePosition.Quantity);
			Assert.AreEqual(sec1Qty * sec1Mrp, actPos.Value);
			Assert.AreEqual(0, actPos.ThePosition.AccountId);
			Assert.AreEqual(null, actPos.ThePosition.Account);
			actPos = positions.Find(p => p.ThePosition.SecurityId == theSec2.SecurityId);
			Assert.AreEqual(theSec2.Name, actPos.SecurityName);
			Assert.AreEqual(theSec2.Symbol, actPos.Symbol);
			Assert.AreEqual(sec2Qty, actPos.ThePosition.Quantity);
			Assert.AreEqual(sec2Qty * sec2Mrp, actPos.Value);
			Assert.AreEqual(0, actPos.ThePosition.AccountId);
			Assert.AreEqual(null, actPos.ThePosition.Account);

			// Check the lots.
			SecurityLot_VM actSecLot;
			actPos = positions.Find(p => p.ThePosition.SecurityId == theSec1.SecurityId);
			Assert.IsNotNull(actPos.SecLots);
			Assert.AreEqual(1, actPos.SecLots.Count);
			actSecLot = actPos.SecLots[0];
			Assert.AreEqual(sec1Date, actSecLot.Date);
			Assert.AreEqual(sec1Qty, actSecLot.Quantity);
			Assert.AreEqual(sec1Value, actSecLot.TotalCostBasis);
			Assert.AreEqual(sec1Value / sec1Qty, actSecLot.PerShareCostBasis);
			Assert.AreEqual(sec1Qty * sec1Mrp, actSecLot.Value);
			Assert.AreEqual((sec1Qty * sec1Mrp) - sec1Value, actSecLot.NetGain);
			Assert.AreEqual(((sec1Qty * sec1Mrp) - sec1Value) / sec1Value, actSecLot.PercentGain);

			actPos = positions.Find(p => p.ThePosition.SecurityId == theSec2.SecurityId);
			Assert.IsNotNull(actPos.SecLots);
			Assert.AreEqual(1, actPos.SecLots.Count);
			actSecLot = actPos.SecLots[0];
			Assert.AreEqual(sec2Date, actSecLot.Date);
			Assert.AreEqual(sec2Qty, actSecLot.Quantity);
			Assert.AreEqual(sec2Value, actSecLot.TotalCostBasis);
			Assert.AreEqual(sec2Value / sec2Qty, actSecLot.PerShareCostBasis);
			Assert.AreEqual(sec2Qty * sec2Mrp, actSecLot.Value);
			Assert.AreEqual((sec2Qty * sec2Mrp) - sec2Value, actSecLot.NetGain);
			Assert.AreEqual(((sec2Qty * sec2Mrp) - sec2Value) / sec2Value, actSecLot.PercentGain);
		}

		[TestMethod]
		public void TwoAccountsWithDifferentPositions()
		{
			// ASSEMBLE
			Account theAcct1 = ModelService_UT.AddAccount("Test Account Name 1", "Test Institution Name 1");

			// Add the transactions that create the positions.
			Security theSec1_1 = ModelService_UT.AddSecurity("TAWDP1-1", "Two Accounts With Different Positions 1-1");
			decimal sec1_1Qty = 7.31M;
			Transaction sec1_1Xact = new Transaction
			{
				Date = DateTime.Parse("2018-05-13"),
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
				Date = DateTime.Parse("2018-05-13"),
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
				Date = DateTime.Parse("2018-05-13"),
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
				Date = DateTime.Parse("2018-05-13"),
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec2_2.SecurityId,
				Quantity = sec2_2Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct2, sec2_2Xact);

			// Update the prices.
			decimal sec1_1Mrp = 3.10M;
			ModelService.AddPrice(theSec1_1.SecurityId, DateTime.Today, sec1_1Mrp, true);
			decimal sec1_2Mrp = 4.30M;
			ModelService.AddPrice(theSec1_2.SecurityId, DateTime.Today, sec1_2Mrp, true);
			decimal sec2_1Mrp = 4.38M;
			ModelService.AddPrice(theSec2_1.SecurityId, DateTime.Today, sec2_1Mrp, true);
			decimal sec2_2Mrp = 6.41M;
			ModelService.AddPrice(theSec2_2.SecurityId, DateTime.Today, sec2_2Mrp, true);

			// ACT
			GlobalPositions_VM testVM = new GlobalPositions_VM();
			List<GlobalPosition_VM> positions = testVM.Positions;

			// ASSERT
			Assert.IsNotNull(positions);
			Assert.AreEqual(4, positions.Count);
			// from account 1
			GlobalPosition_VM actPos = positions.Find(p => p.ThePosition.SecurityId == theSec1_1.SecurityId);
			Assert.AreEqual(theSec1_1.Name, actPos.SecurityName);
			Assert.AreEqual(theSec1_1.Symbol, actPos.Symbol);
			Assert.AreEqual(sec1_1Qty, actPos.ThePosition.Quantity);
			Assert.AreEqual(sec1_1Qty * sec1_1Mrp, actPos.Value);
			Assert.AreEqual(0, actPos.ThePosition.AccountId);
			Assert.AreEqual(null, actPos.ThePosition.Account);
			actPos = positions.Find(p => p.ThePosition.SecurityId == theSec1_2.SecurityId);
			Assert.AreEqual(theSec1_2.Name, actPos.SecurityName);
			Assert.AreEqual(theSec1_2.Symbol, actPos.Symbol);
			Assert.AreEqual(sec1_2Qty, actPos.ThePosition.Quantity);
			Assert.AreEqual(sec1_2Qty * sec1_2Mrp, actPos.Value);
			Assert.AreEqual(0, actPos.ThePosition.AccountId);
			Assert.AreEqual(null, actPos.ThePosition.Account);
			// from account 2
			actPos = positions.Find(p => p.ThePosition.SecurityId == theSec2_1.SecurityId);
			Assert.AreEqual(theSec2_1.Name, actPos.SecurityName);
			Assert.AreEqual(theSec2_1.Symbol, actPos.Symbol);
			Assert.AreEqual(sec2_1Qty, actPos.ThePosition.Quantity);
			Assert.AreEqual(sec2_1Qty * sec2_1Mrp, actPos.Value);
			Assert.AreEqual(0, actPos.ThePosition.AccountId);
			Assert.AreEqual(null, actPos.ThePosition.Account);
			actPos = positions.Find(p => p.ThePosition.SecurityId == theSec2_2.SecurityId);
			Assert.AreEqual(theSec2_2.Name, actPos.SecurityName);
			Assert.AreEqual(theSec2_2.Symbol, actPos.Symbol);
			Assert.AreEqual(sec2_2Qty, actPos.ThePosition.Quantity);
			Assert.AreEqual(sec2_2Qty * sec2_2Mrp, actPos.Value);
			Assert.AreEqual(0, actPos.ThePosition.AccountId);
			Assert.AreEqual(null, actPos.ThePosition.Account);
		}

		[TestMethod]
		public void TwoAccountsWithOverlappingPositions()
		{
			// ASSEMBLE
			Account theAcct1 = ModelService_UT.AddAccount("Test Account Name 1", "Test Institution Name 1");

			// Add the transactions that create the positions.
			Security theSec1_1 = ModelService_UT.AddSecurity("TAWDP1-1", "Two Accounts With Different Positions 1-1");
			decimal sec1_1Qty = 7.31M;
			Transaction sec1_1Xact = new Transaction
			{
				Date = DateTime.Parse("2018-05-13"),
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
				Date = DateTime.Parse("2018-05-13"),
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
				Date = DateTime.Parse("2018-05-13"),
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
				Date = DateTime.Parse("2018-05-13"),
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
				Date = DateTime.Parse("2018-05-13"),
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
				Date = DateTime.Parse("2018-05-13"),
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSecShared.SecurityId,
				Quantity = secShared2Qty,
				Value = 2083.49M,
				Fee = 21.12M,
			};
			ModelService.AddTransaction(theAcct2, secShared2Xact);

			// Update the prices.
			decimal sec1_1Mrp = 4.57M;
			ModelService.AddPrice(theSec1_1.SecurityId, DateTime.Today, sec1_1Mrp, true);
			decimal sec1_2Mrp = 22.40M;
			ModelService.AddPrice(theSec1_2.SecurityId, DateTime.Today, sec1_2Mrp, true);
			decimal sec2_1Mrp = 2.54M;
			ModelService.AddPrice(theSec2_1.SecurityId, DateTime.Today, sec2_1Mrp, true);
			decimal sec2_2Mrp = 9.00M;
			ModelService.AddPrice(theSec2_2.SecurityId, DateTime.Today, sec2_2Mrp, true);
			decimal secSharedMrp = 10.58M;
			ModelService.AddPrice(theSecShared.SecurityId, DateTime.Today, secSharedMrp, true);

			// ACT
			GlobalPositions_VM testVM = new GlobalPositions_VM();
			List<GlobalPosition_VM> positions = testVM.Positions;

			// ASSERT
			Assert.IsNotNull(positions);
			Assert.AreEqual(5, positions.Count);
			// from account 1
			GlobalPosition_VM actPos = positions.Find(p => p.ThePosition.SecurityId == theSec1_1.SecurityId);
			Assert.AreEqual(theSec1_1.Name, actPos.SecurityName);
			Assert.AreEqual(theSec1_1.Symbol, actPos.Symbol);
			Assert.AreEqual(sec1_1Qty, actPos.ThePosition.Quantity);
			Assert.AreEqual(sec1_1Qty * sec1_1Mrp, actPos.Value);
			Assert.AreEqual(0, actPos.ThePosition.AccountId);
			Assert.AreEqual(null, actPos.ThePosition.Account);
			actPos = positions.Find(p => p.ThePosition.SecurityId == theSec1_2.SecurityId);
			Assert.AreEqual(theSec1_2.Name, actPos.SecurityName);
			Assert.AreEqual(theSec1_2.Symbol, actPos.Symbol);
			Assert.AreEqual(sec1_2Qty, actPos.ThePosition.Quantity);
			Assert.AreEqual(sec1_2Qty * sec1_2Mrp, actPos.Value);
			Assert.AreEqual(0, actPos.ThePosition.AccountId);
			Assert.AreEqual(null, actPos.ThePosition.Account);
			// from account 2
			actPos = positions.Find(p => p.ThePosition.SecurityId == theSec2_1.SecurityId);
			Assert.AreEqual(theSec2_1.Name, actPos.SecurityName);
			Assert.AreEqual(theSec2_1.Symbol, actPos.Symbol);
			Assert.AreEqual(sec2_1Qty, actPos.ThePosition.Quantity);
			Assert.AreEqual(sec2_1Qty * sec2_1Mrp, actPos.Value);
			Assert.AreEqual(0, actPos.ThePosition.AccountId);
			Assert.AreEqual(null, actPos.ThePosition.Account);
			actPos = positions.Find(p => p.ThePosition.SecurityId == theSec2_2.SecurityId);
			Assert.AreEqual(theSec2_2.Name, actPos.SecurityName);
			Assert.AreEqual(theSec2_2.Symbol, actPos.Symbol);
			Assert.AreEqual(sec2_2Qty, actPos.ThePosition.Quantity);
			Assert.AreEqual(sec2_2Qty * sec2_2Mrp, actPos.Value);
			Assert.AreEqual(0, actPos.ThePosition.AccountId);
			Assert.AreEqual(null, actPos.ThePosition.Account);
			// shared position
			actPos = positions.Find(p => p.ThePosition.SecurityId == theSecShared.SecurityId);
			Assert.AreEqual(theSecShared.Name, actPos.SecurityName);
			Assert.AreEqual(theSecShared.Symbol, actPos.Symbol);
			Assert.AreEqual(secShared1Qty + secShared2Qty, actPos.ThePosition.Quantity);
			Assert.AreEqual((secShared1Qty + secShared2Qty) * secSharedMrp, actPos.Value);
			Assert.AreEqual(0, actPos.ThePosition.AccountId);
			Assert.AreEqual(null, actPos.ThePosition.Account);
		}
	}
}
