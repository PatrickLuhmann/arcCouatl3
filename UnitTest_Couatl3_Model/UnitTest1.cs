using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Couatl3_Model;

namespace UnitTest_Couatl3_Model
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void CreateAccount()
		{
			using (var db = new CouatlContext())
			{
				// I don't know if I need to do this.
				// TODO: If this is necessary, put it in the "run before each test" method.
				db.Database.Migrate();

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
	}
}
