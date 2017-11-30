using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Couatl3_Model;

namespace Couatl3_ViewModels
{
	public class AccountsViewModel : INotifyPropertyChanged
	{
		public AccountsViewModel()
		{
			// TODO: Remove this placeholder data.
			totalValue = "12345.67";
			accountsList = new List<AccountVM>();
			accountsList.Add(new AccountVM { AccountName = "Name1", AccountValue = "123.00" });
			accountsList.Add(new AccountVM { AccountName = "Name2", AccountValue = "$123123123.45" });
			accountsList.Add(new AccountVM { AccountName = "Name3", AccountValue = "111111.11" });

			using (var db = new CouatlContext())
			{
				List<Account> openAccts;

				// Only show accounts that are open.
				openAccts = db.Accounts.Where(a => a.Closed == false).ToList();

				foreach (Account acct in openAccts)
				{
					AccountVM vmAcct = new AccountVM();
					vmAcct.AccountName = acct.Name;
					// TODO: This is obviously just temp code.
					vmAcct.AccountValue = "$99.99";

					AccountsList.Add(vmAcct);
				}
			}
		}

		private string totalValue;
		public string TotalValue { get { return totalValue; } }

		private List<AccountVM> accountsList;
		public List<AccountVM> AccountsList { get { return accountsList; } }

		// I copied this from a web page. I do not know what it does.
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

	}

	public class AccountVM
	{
		public string AccountName { get; set; }
		public string AccountValue { get; set; }
	}
}
