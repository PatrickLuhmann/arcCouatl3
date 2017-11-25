using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Couatl3_ViewModels
{
	public class AccountsViewModel : INotifyPropertyChanged
	{
		public AccountsViewModel()
		{
			// TODO: Remove this placeholder data.
			totalValue = "12345.67";
			accountsList = new List<Account>();
			accountsList.Add(new Account { AccountName = "Name1", AccountValue = "123.45" });
			accountsList.Add(new Account { AccountName = "Name1", AccountValue = "123.45" });
			accountsList.Add(new Account { AccountName = "Name1", AccountValue = "123.45" });
		}

		private string totalValue;
		public string TotalValue { get { return totalValue; } }

		private List<Account> accountsList;
		public List<Account> AccountsList { get { return accountsList; } }

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

	public class Account
	{
		public string AccountName { get; set; }
		public string AccountValue { get; set; }
	}
}
