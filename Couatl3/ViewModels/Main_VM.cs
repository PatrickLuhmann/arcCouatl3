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
		private ObservableCollection<Account_VM> accounts = new ObservableCollection<Account_VM>();
		public ObservableCollection<Account_VM> Accounts
		{
			get
			{
				List<Account> openAccts = ModelService.GetAccounts(true);
				accounts.Clear();
				foreach (Account acct in openAccts)
				{
					accounts.Add(new Account_VM(acct, this));
				}
				return accounts;
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
				// NOTE: this can be called with a null value,
				// such as during accounts.Clear().
				selectedAccount = value;
				if (selectedAccount != null)
					selectedAccount.SelectedTransaction = null;
				RaisePropertyChanged("SelectedAccount");
			}
		}

		public int NumXacts
		{
			get
			{
				List<Transaction> temp = ModelService.GetTransactions();
				return temp.Count;
			}
		}

		public void NotifyNumXacts()
		{
			RaisePropertyChanged("NumXacts");
		}

		public string PocSecSym { get; set; }
		public string PocSecName { get; set; }
		public List<Security> PocSecList
		{
			get
			{
				List<Security> list;
				using (var db = new CouatlContext())
				{
					list = db.Securities.ToList();
				}
				return list;
			}
			private set { }
		}

		public RelayCommand RelayAddAccountCmd { get; set; }
		public RelayCommand RelayDeleteAccountCmd { get; set; }
		public RelayCommand RelayAddSecurityCmd { get; set; }

		private void AddAccount()
		{
			Account newAcct = new Account();
			newAcct.Institution = "Bank Of Middle Earth";
			newAcct.Name = "Elrond's Checking Account";

			ModelService.AddAccount(newAcct);

			// Trigger a re-read of the database.
			RaisePropertyChanged("Accounts");
		}

		private void DeleteAccount()
		{
			if (selectedAccount != null)
			{
				// Remove it from the database.
				ModelService.DeleteAccount(selectedAccount.TheAccount);

				// Remove it from the VM list.
				// NOTE: This sets selectedAccount to null
				accounts.Remove(selectedAccount);

				// Trigger a re-read of the database.
				// NOTE: This sets selected Account to null
				RaisePropertyChanged("Accounts");

				// Update selected item.
				if (accounts.Count > 0)
					SelectedAccount = accounts[0];
				else
					SelectedAccount = null;
			}
			RaisePropertyChanged("NumXacts");
		}

		private void AddSecurity()
		{
			if (PocSecSym == "" || PocSecName == "")
				return;
			Security newSec = new Security();
			newSec.Symbol = PocSecSym;
			newSec.Name = PocSecName;
			using (var db = new CouatlContext())
			{
				db.Securities.Add(newSec);
				db.SaveChanges();
			}
			PocSecSym = "";
			PocSecName = "";
			RaisePropertyChanged("PocSecList");
		}

		public Main_VM()
		{
			ModelService.Initialize();

			RelayAddAccountCmd = new RelayCommand(AddAccount);
			RelayDeleteAccountCmd = new RelayCommand(DeleteAccount);
			RelayAddSecurityCmd = new RelayCommand(AddSecurity);

			//TODO: POC for add security
			PocSecSym = "";
			PocSecName = "";

			//TODO: POC for showing transactions
		}
	}
}
