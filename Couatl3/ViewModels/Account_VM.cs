using Couatl3.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Couatl3.ViewModels
{
	public class Account_VM : ViewModelBase
	{
		public Account TheAccount { get; set; }
		public ObservableCollection<Position_VM> MyPositions { get; private set; }
		public ObservableCollection<Transaction_VM> MyTransactions { get; set; }

		private Transaction_VM selectedTransaction;
		public Transaction_VM SelectedTransaction
		{
			get
			{
				return selectedTransaction;
			}
			set
			{
				selectedTransaction = value;
				RaisePropertyChanged("SelectedTransaction");
			}
		}

		public RelayCommand UpdateAccountNamesCmd { get; set; }
		public RelayCommand AddTransactionCmd { get; set; }
		public RelayCommand DeleteTransactionCmd { get; set; }
		public RelayCommand UpdateTransactionCmd { get; set; }

		private void UpdateAccountNames()
		{
			ModelService.UpdateAccount(TheAccount);
		}

		private void AddTransaction()
		{
			// Create the object and update the database.
			Transaction t = new Transaction();
			TheAccount.Transactions.Add(t);
			ModelService.UpdateAccount(TheAccount);

			// Create the VM object and update the app/UI.
			Transaction_VM tvm = new Transaction_VM();
			tvm.TheTransaction = t;
			MyTransactions.Add(tvm);
			MyParent.NotifyNumXacts();
		}

		public void DeleteTransaction()
		{
			if (SelectedTransaction != null)
			{
				ModelService.DeleteTransaction(SelectedTransaction.TheTransaction);
				MyTransactions.Remove(SelectedTransaction);
				SelectedTransaction = null;
				MyParent.NotifyNumXacts();
			}
		}

		public void UpdateTransaction()
		{
			if (SelectedTransaction != null)
			{
				ModelService.UpdateTransaction(selectedTransaction.TheTransaction);
			}
		}

		private Main_VM MyParent;
		public Account_VM(Account acct, Main_VM parent)
		{
			TheAccount = acct;
			MyParent = parent;

			UpdateAccountNamesCmd = new RelayCommand(UpdateAccountNames);
			AddTransactionCmd = new RelayCommand(AddTransaction);
			DeleteTransactionCmd = new RelayCommand(DeleteTransaction);
			UpdateTransactionCmd = new RelayCommand(UpdateTransaction);

			// Populate MyPositions.
			MyPositions = new ObservableCollection<Position_VM>();
			foreach (var p in TheAccount.Positions)
			{
				Position_VM pvm = new Position_VM();
				pvm.ThePosition = p;
				MyPositions.Add(pvm);
			}

			// Populate MyTransactions.
			MyTransactions = new ObservableCollection<Transaction_VM>();
			foreach (var t in TheAccount.Transactions)
			{
				Transaction_VM pvm = new Transaction_VM();
				pvm.TheTransaction = t;
				MyTransactions.Add(pvm);
			}
		}
	}

	public class Position_VM
	{
		private Position thePosition;
		public Position ThePosition
		{
			get { return thePosition; }
			set
			{
				thePosition = value;
				Value = thePosition.Quantity * ModelService.GetNewestPrice(thePosition.Security);
			}
		}
		public decimal Value { get; set; }
	}

	public class ComboBoxXactType
	{
		public ModelService.TransactionType XactEnum { get; set; }
		public string XactTypeString { get; set; }
	}

	public class Transaction_VM : ViewModelBase
	{
		private Transaction theTransaction;
		public Transaction TheTransaction
		{
			get { return theTransaction; }
			set
			{
				theTransaction = value;

				type = (ModelService.TransactionType)theTransaction.Type;
				
				//TODO: Implement method to calculate cash balance per transaction.
				CashBalance = 3.4M;
			}
		}

		public List<ComboBoxXactType> XactTypeList { get; set; }

		private ModelService.TransactionType type;
		public ModelService.TransactionType Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
				theTransaction.Type = (int)type;
				RaisePropertyChanged("Type");
				RaisePropertyChanged("TheTransaction");
			}
		}

		public decimal CashBalance { get; set; } = 1.2M;

		public Transaction_VM()
		{
			XactTypeList = new List<ComboBoxXactType>()
			{
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.Deposit, XactTypeString = "Deposit"},
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.Withdrawal, XactTypeString = "Withdrawal"},
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.Buy, XactTypeString = "Buy"},
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.Sell, XactTypeString = "Sell"},
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.Dividend, XactTypeString = "Dividend "},
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.StockSplit, XactTypeString = "Stock Split"},
			};
		}
	}
}
