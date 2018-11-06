using GalaSoft.MvvmLight;
using Couatl3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace Couatl3.ViewModels
{
    public class Main_VM : ViewModelBase
    {
		public ObservableCollection<Account_VM> Accounts
		{
			get
			{
				ObservableCollection<Account_VM> items = new ObservableCollection<Account_VM>();
				using (var db = new CouatlContext())
				{
					List<Account> openAccts = db.Accounts.Where(a => a.Closed == false).ToList();
					foreach (Account acct in openAccts)
					{
						items.Add(new Account_VM(acct));
					}
				}
				return items;
			}
		}

		private Account_VM selectedAccount;
		public Account_VM SelectedAccount
		{
			get
			{
				return selectedAccount;
			}
			set
			{
				selectedAccount = value;
				RaisePropertyChanged("SelectedAccount");
			}
		}

		public RelayCommand RelayAddAccountCmd { get; set; }

		private void AddAccount()
		{
			// TODO: Remove this test code.
			Account newAcct = new Account();
			newAcct.Institution = "Bank Of Middle Earth";
			newAcct.Name = "Elrond's Checking Account";
			using (var db = new CouatlContext())
			{
				db.Accounts.Add(newAcct);
				db.SaveChanges();
			}
			RaisePropertyChanged("Accounts");
		}

		public Main_VM()
		{
			// TODO: Move this to a ViewModel.
			using (var db = new CouatlContext())
			{
				db.Database.Migrate();
			}

			RelayAddAccountCmd = new RelayCommand(AddAccount);
		}
	}
}
