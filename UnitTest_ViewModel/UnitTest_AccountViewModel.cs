using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Couatl3_ViewModels;
using Couatl3_Model;
using System.Collections.Generic;

namespace UnitTest_ViewModel
{
	[TestClass]
	public class UnitTest_AccountViewModel
	{
		[TestMethod]
		public void AccountName_Basic()
		{
			// ASSEMBLE
			Security acme = new Security
			{
				SecurityId = 1,
				Name = "ACME Inc.",
				Symbol = "ACME"
			};
			Account account = new Account();
			account.Institution = "Institution Name";
			account.Name = "Account Name";
			account.Cash = 123.45M;
			account.Closed = false;
			account.Transactions = new List<Transaction>();
			account.Positions = new List<Position>();
			account.Positions.Add(new Position { PositionId = 1, Quantity = 100, Security = acme });

			// Create a new AccountViewModel.
			// TODO: How does it know which account to use?
			AccountViewModel VM = new AccountViewModel(account);

			// TODO: What is the default account name? Does this even make sense?

			// TODO: Load a Model and then check account names?

			// ACT

			// ASSERT
			Assert.AreEqual(account.Name, VM.Name);
			Assert.AreEqual(account.Institution, VM.Institution);
			Assert.AreEqual(123.45M + 100 * 1.2M, VM.Value);
		}
	}
}
