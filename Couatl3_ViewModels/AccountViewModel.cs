using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couatl3_Model;

namespace Couatl3_ViewModels
{
	public class AccountViewModel
	{
		private Account account;

		public string Name { get { return account.Name; } }
		public string Institution { get { return account.Institution; } }
		public decimal Value
		{
			get
			{
				decimal total = 0.0M;
#if true
				foreach (var p in account.Positions)
				{
					total += (p.Quantity * Blah.MostRecentValue(p.Security));
				}
				total += account.Cash;
#endif
				return total;
			}
		}

		// TODO: Does this make sense? If the View puts this into a ListView, what columns
		// will it be able to define? Quantity is there, but what about the symbol or the
		// name of the security? Can the binding go into the Security class?
		public ObservableCollection<Position> Positions { get; set; }

		public AccountViewModel(Account acct)
		{
			account = acct;
		}
	}
}
