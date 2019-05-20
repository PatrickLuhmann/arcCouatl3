using Couatl3.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Couatl3.ViewModels
{
	public class Account_VM : ViewModelBase
	{
		public Account TheAccount { get; set; }
		public ObservableCollection<Position_VM> MyPositions { get; private set; }
		public ObservableCollection<Transaction_VM> MyTransactions { get; private set; }
		public decimal Value
		{
			get
			{
				return ModelService.GetAccountValue(TheAccount);
			}
		}

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
			ModelService.AddTransaction(TheAccount, t);

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
				// TODO: Move the selection to the next/previous item in the list.
				SelectedTransaction = null;
				MyParent.NotifyNumXacts();
				RaisePropertyChanged("Value");
				RaisePropertyChanged("TheAccount");
			}
		}

		public void UpdateTransaction()
		{
			if (selectedTransaction != null)
			{
				// TODO: Validate fields by Type?
				// EX: There is no reason to have a Quantity or SecurityId for a Buy transaction.
				Transaction newXact = new Transaction();
				newXact.Type = (int)selectedTransaction.Type;
				newXact.Date = selectedTransaction.Date;
				newXact.Quantity = selectedTransaction.Quantity ?? 0;
				newXact.Fee = selectedTransaction.Fee;
				newXact.Value = selectedTransaction.Value;
				newXact.SecurityId = selectedTransaction.GetSecurityIdFromCombo();

				// Delete the existing transaction.
				ModelService.DeleteTransaction(selectedTransaction.TheTransaction);
				// Add the new transaction.
				ModelService.AddTransaction(TheAccount, newXact);

				// NOTE: MyTransactions does not need to be modified, because
				// we are reusing the same Transaction_VM "bucket".
				SelectedTransaction.TheTransaction = newXact;

				//TODO: Update the things the ViewModel tracks.
				CalculateCashBalance();
				PopulatePositions();
				RaisePropertyChanged("Value");
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
			PopulatePositions();

			// Populate MyTransactions.
			MyTransactions = new ObservableCollection<Transaction_VM>();
			foreach (var t in TheAccount.Transactions)
			{
				Transaction_VM tvm = new Transaction_VM();
				tvm.TheTransaction = t;
				MyTransactions.Add(tvm);
			}

			CalculateCashBalance();
		}

		private void PopulatePositions()
		{
			// First, remove the existing positions. This is easier
			// than trying to figure out which ones need to change.
			// Also, simply allocating a new collection object (which
			// will be empty) doesn't work; the RaisePropertyChanged
			// doesn't go through for some reason.
			if (MyPositions != null)
				while (MyPositions.Count > 0)
					MyPositions.RemoveAt(0);
			else
				MyPositions = new ObservableCollection<Position_VM>();

			// Now put each position in the collection.
			foreach (var p in TheAccount.Positions)
			{
				Position_VM pvm = new Position_VM();
				pvm.ThePosition = p;
				MyPositions.Add(pvm);
			}

			// Tell the View to redraw the table.
			RaisePropertyChanged("MyPosition");
		}

		private void CalculateCashBalance()
		{
			// This does not change MyTransactions. You could add .ToList() but I don't think that will help me.
			MyTransactions.OrderBy(t => t.TheTransaction.Date);

			// Update the running total for each transaction.
			ListCollectionView blah = (ListCollectionView) CollectionViewSource.GetDefaultView(MyTransactions);
			blah.CustomSort = new SortTransactionVmByDate();
			decimal balance = 0.0M;
			foreach (Transaction_VM xact in blah)
			{
				switch (xact.TheTransaction.Type)
				{
					case (int)ModelService.TransactionType.Deposit:
					case (int)ModelService.TransactionType.Sell:
					case (int)ModelService.TransactionType.Dividend:
						balance += xact.TheTransaction.Value;
						break;
					case (int)ModelService.TransactionType.Withdrawal:
					case (int)ModelService.TransactionType.Buy:
					case (int)ModelService.TransactionType.Fee:
						balance -= xact.TheTransaction.Value;
						break;
				}
				xact.CashBalance = balance;
			}

			// Update the Account value.
			RaisePropertyChanged("TheAccount");
		}
	}

	public class SortTransactionVmByDate : IComparer
	{
		public int Compare(object a, object b)
		{
			if (((Transaction_VM)a).TheTransaction.Date > ((Transaction_VM)b).TheTransaction.Date)
				return 1;
			else
				return -1;
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
				Value = thePosition.Quantity * ModelService.GetNewestPrice(thePosition.SecurityId);
				Symbol = ModelService.GetSymbolFromId(ThePosition.SecurityId);
				Name = ModelService.GetSecurityNameFromId(ThePosition.SecurityId);
			}
		}
		public decimal Value { get; set; }
		public string Symbol { get; set; }
		public string Name { get; set; }
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

				Date = theTransaction.Date;
				type = (ModelService.TransactionType)theTransaction.Type;
				Symbol = ModelService.GetSymbolFromId(theTransaction.SecurityId);
				if (Symbol == "$$INVALID$$")
					Symbol = "";
				// Not all transaction types have a meaningful Quantity.
				switch (type)
				{
					case ModelService.TransactionType.Buy:
					case ModelService.TransactionType.Sell:
					case ModelService.TransactionType.StockSplit:
						Quantity = theTransaction.Quantity;
						break;
					case ModelService.TransactionType.Deposit:
					case ModelService.TransactionType.Withdrawal:
					case ModelService.TransactionType.Dividend:
					default:
						Quantity = null;
						break;
				}
				Fee = theTransaction.Fee;
				Value = theTransaction.Value;

			}
		}

		public DateTime Date { get; set; }

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
				//theTransaction.Type = (int)type;
				//RaisePropertyChanged("Type");
				//RaisePropertyChanged("TheTransaction");
			}
		}

		private List<Security> Securities;
		public int GetSecurityIdFromCombo()
		{
			//return Securities[SymbolComboBoxIdx].SecurityId;
			Security sec = Securities.Find(s => s.Symbol == Symbol);
			if (sec != null)
				return sec.SecurityId;
			else
				return -1;
		}
		public List<string> SecSymbolList { get; set; }

		private string symbol;
		public string Symbol
		{
			get
			{
				return symbol;
			}
			set
			{
				symbol = value;
				RaisePropertyChanged("Symbol");
			}
		}
		//public int SymbolComboBoxIdx { get; set; }

		private decimal? quantity;
		public decimal? Quantity
		{
			get
			{
				return quantity;
			}
			set
			{
				quantity = value;
				RaisePropertyChanged("Quantity");
			}
		}

		public decimal Fee { get; set; }

		public decimal Value { get; set; }

		public decimal CashBalance { get; set; } = 0.0M;

		public Transaction_VM()
		{
			// TODO: Is this really the right place for this?
			XactTypeList = new List<ComboBoxXactType>()
			{
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.Deposit, XactTypeString = "Deposit"},
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.Withdrawal, XactTypeString = "Withdrawal"},
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.Buy, XactTypeString = "Buy"},
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.Sell, XactTypeString = "Sell"},
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.Dividend, XactTypeString = "Dividend "},
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.StockSplit, XactTypeString = "Stock Split"},
				new ComboBoxXactType() {XactEnum = ModelService.TransactionType.Fee, XactTypeString = "Fee"},
			};

			// Get list of symbols for Security ComboBox.
			// TODO: Creating these lists seems like a waste. Why can't there be a "global" source for the securities?
			Securities = ModelService.GetSecurities();
			SecSymbolList = new List<string>();
			System.Diagnostics.Debug.WriteLine("Construct list of security symbols for the Security ComboBox");
			SecSymbolList.Add("");
			foreach (Security sec in Securities)
			{
				Debug.WriteLine($"[{sec.SecurityId}] {sec.Symbol} - {sec.Name}");
				SecSymbolList.Add(sec.Symbol);
			}
		}
	}

	public class TransactionTypeToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// TODO: These strings need to be in a resource file or something.
			switch ((int)value)
			{
				case 1:
					return "Deposit";
				case 2:
					return "Withdrawal";
				case 3:
					return "Buy";
				case 4:
					return "Sell";
				case 5:
					return "Dividend";
				case 6:
					return "Stock Split";
				case 7:
					return "NOT IMPLEMENTED";
				case 8:
					return "NOT IMPLEMENTED";
				case 9:
					return "Fee";
				default:
					return "Unknown";
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
