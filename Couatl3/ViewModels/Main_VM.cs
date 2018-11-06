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
					// TODO: Figure out why this statement is needed.
					// If I don't get the list of Securities here, the Transactions
					// collection of the Account will have null for the Security.
					List<Security> secList = db.Securities.ToList();
					List<Account> openAccts = db.Accounts
						.Where(a => a.Closed == false)
						.Include(a => a.Transactions)
						.Include(a => a.Positions)
						.ToList();
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
		public RelayCommand RelayAddSecurityCmd { get; set; }

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
			// TODO: Move this to a ViewModel.
			using (var db = new CouatlContext())
			{
				db.Database.Migrate();
			}

			RelayAddAccountCmd = new RelayCommand(AddAccount);
			RelayAddSecurityCmd = new RelayCommand(AddSecurity);

			//TODO: POC for add security
			PocSecSym = "";
			PocSecName = "";

			//TODO: POC for showing transactions
#if false
			using (var db = new CouatlContext())
			{
				Transaction newXact = new Transaction
				{
					Type = 1,
					Quantity = 123,
					Value = 6156.95M,
					Fee = 6.95M,
					Date = DateTime.Now
				};
				newXact.Security = db.Securities.Single(s => s.Symbol == "ACME");
				Account tgtAccont = db.Accounts.First();
				tgtAccont.Transactions.Add(newXact);
				//db.Transactions.Add(newXact);

				db.SaveChanges();
			}
#endif
		}
	}
}
