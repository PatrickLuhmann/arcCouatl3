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
		public void OneAccountWithNoPositionsDueToSales()
		{
			// ASSEMBLE
			Account theAcct = ModelService_UT.AddAccount("Test Account Name", "Test Institution Name");

			// Add the transaction that creates the position.
			Security theSec1 = ModelService_UT.AddSecurity("OAWNPDTS", "One Account No Positions Sales");
			DateTime sec1Date = DateTime.Parse("2018-05-28");
			decimal sec1Qty = 100.0M;
			decimal sec1Value = 2204.48M;
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

			// Add the transactions that remove the position.
			sec1Xact = new Transaction
			{
				Date = DateTime.Parse("2018-08-13"),
				Type = (int)ModelService.TransactionType.Sell,
				SecurityId = theSec1.SecurityId,
				Quantity = 40,
				Value = 1000.00M,
				Fee = 4.34M,
			};
			ModelService.AddTransaction(theAcct, sec1Xact);
			sec1Xact = new Transaction
			{
				Date = DateTime.Parse("2019-04-10"),
				Type = (int)ModelService.TransactionType.Sell,
				SecurityId = theSec1.SecurityId,
				Quantity = 60,
				Value = 1500.00M,
				Fee = 4.48M,
			};
			ModelService.AddTransaction(theAcct, sec1Xact);

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

			// For the second position, sell some after the buy.
			Security theSec2 = ModelService_UT.AddSecurity("OAWP2", "One Account With Positions 2");
			DateTime sec2BuyDate = DateTime.Parse("2018-05-13");
			decimal sec2BuyQty = 25.0M;
			decimal sec2BuyValue = 2500.00M;
			Transaction sec2BuyXact = new Transaction
			{
				Date = sec2BuyDate,
				Type = (int)ModelService.TransactionType.Buy,
				SecurityId = theSec2.SecurityId,
				Quantity = sec2BuyQty,
				Value = sec2BuyValue,
				Fee = 4.34M,
			};
			ModelService.AddTransaction(theAcct, sec2BuyXact);
			DateTime sec2SellDate = DateTime.Parse("2018-05-20");
			decimal sec2SellQty = 5.0M;
			decimal sec2SellValue = 5.30M;
			Transaction sec2SellXact = new Transaction
			{
				Date = sec2SellDate,
				Type = (int)ModelService.TransactionType.Sell,
				SecurityId = theSec2.SecurityId,
				Quantity = sec2SellQty,
				Value = sec2SellValue,
				Fee = 1.23M,
			};
			ModelService.AddTransaction(theAcct, sec2SellXact);

			// Update the prices.
			decimal sec1Mrp = 2.43M;
			ModelService.AddPrice(theSec1.SecurityId, DateTime.Today, sec1Mrp, true);
			decimal sec2Mrp = 4.24M;
			ModelService.AddPrice(theSec2.SecurityId, DateTime.Today, sec2Mrp, true);

			// ACT
			GlobalPositions_VM testVM = new GlobalPositions_VM();
			List<GlobalPosition_VM> positionVMs = testVM.Positions;

			// ASSERT
			GlobalPosition_VM actPosVM;
			Assert.IsNotNull(positionVMs);
			Assert.AreEqual(2, positionVMs.Count);
			actPosVM = positionVMs.Find(p => p.ThePosition.SecurityId == theSec1.SecurityId);
			Assert.AreEqual(theSec1.Name, actPosVM.SecurityName);
			Assert.AreEqual(theSec1.Symbol, actPosVM.Symbol);
			Assert.AreEqual(sec1Qty, actPosVM.ThePosition.Quantity);
			Assert.AreEqual(sec1Qty * sec1Mrp, actPosVM.Value);
			Assert.AreEqual(0, actPosVM.ThePosition.AccountId);
			Assert.AreEqual(null, actPosVM.ThePosition.Account);
			actPosVM = positionVMs.Find(p => p.ThePosition.SecurityId == theSec2.SecurityId);
			Assert.AreEqual(theSec2.Name, actPosVM.SecurityName);
			Assert.AreEqual(theSec2.Symbol, actPosVM.Symbol);
			Assert.AreEqual(sec2BuyQty - sec2SellQty, actPosVM.ThePosition.Quantity);
			Assert.AreEqual((sec2BuyQty - sec2SellQty) * sec2Mrp, actPosVM.Value);
			Assert.AreEqual(0, actPosVM.ThePosition.AccountId);
			Assert.AreEqual(null, actPosVM.ThePosition.Account);

			// Check the lots.
			SecurityLot_VM actSecLotVM;
			actPosVM = positionVMs.Find(p => p.ThePosition.SecurityId == theSec1.SecurityId);
			Assert.IsNotNull(actPosVM.SecLots);
			Assert.AreEqual(1, actPosVM.SecLots.Count);
			actSecLotVM = actPosVM.SecLots[0];
			Assert.AreEqual(sec1Date, actSecLotVM.Date);
			Assert.AreEqual(sec1Qty, actSecLotVM.Quantity);
			Assert.AreEqual(sec1Value, actSecLotVM.TotalCostBasis);
			Assert.AreEqual(sec1Value / sec1Qty, actSecLotVM.PerShareCostBasis);
			Assert.AreEqual(sec1Qty * sec1Mrp, actSecLotVM.Value);
			Assert.AreEqual((sec1Qty * sec1Mrp) - sec1Value, actSecLotVM.NetGain);
			Assert.AreEqual(((sec1Qty * sec1Mrp) - sec1Value) / sec1Value, actSecLotVM.PercentGain);

			actPosVM = positionVMs.Find(p => p.ThePosition.SecurityId == theSec2.SecurityId);
			Assert.IsNotNull(actPosVM.SecLots);
			Assert.AreEqual(1, actPosVM.SecLots.Count);
			actSecLotVM = actPosVM.SecLots[0];
			Assert.AreEqual(sec2BuyDate, actSecLotVM.Date);
			Assert.AreEqual(sec2BuyQty - sec2SellQty, actSecLotVM.Quantity);
			decimal expTCB = 2000.00M;
			Assert.AreEqual(expTCB, actSecLotVM.TotalCostBasis);
			Assert.AreEqual(sec2BuyValue / sec2BuyQty, actSecLotVM.PerShareCostBasis);
			decimal expValue = (sec2BuyQty - sec2SellQty) * sec2Mrp;
			Assert.AreEqual(expValue, actSecLotVM.Value);
			decimal expNetGain = expValue - expTCB;
			Assert.AreEqual(expNetGain, actSecLotVM.NetGain);
			decimal expPerGain = (expValue - expTCB) / expTCB;
			Assert.AreEqual(expPerGain, actSecLotVM.PercentGain);
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
