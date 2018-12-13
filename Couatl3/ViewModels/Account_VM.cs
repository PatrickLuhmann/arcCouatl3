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
			if (selectedTransaction != null)
			{
				if (selectedTransaction.Type == ModelService.TransactionType.Null)
				{
					// TODO: Do something here? Or just let it slide?
				}
				else if (selectedTransaction.Type == ModelService.TransactionType.Deposit ||
				         selectedTransaction.Type == ModelService.TransactionType.Withdrawal)
				{
					selectedTransaction.TheTransaction.Type = (int)selectedTransaction.Type;
					selectedTransaction.TheTransaction.Date = selectedTransaction.Date;
					selectedTransaction.TheTransaction.Security = null; // N/A for Deposit/Withdrawal
					selectedTransaction.TheTransaction.Quantity = 0.0M; // N/A for Deposit/Withdrawal
					selectedTransaction.TheTransaction.Fee = selectedTransaction.Fee;
					selectedTransaction.TheTransaction.Value = selectedTransaction.Value;

					ModelService.UpdateTransaction(selectedTransaction.TheTransaction);
				}
				else if (selectedTransaction.Type == ModelService.TransactionType.Buy)
				{
					// TODO: For now, assume Type can't change, because then we would need to roll back what this xact used to do.
					selectedTransaction.TheTransaction.Type = (int)selectedTransaction.Type;

					// If Security has changed and the original was not null, then back out that Position.
					Debug.WriteLine("Buy: Symbol was " + selectedTransaction.TheTransaction.Security + " and is now " + selectedTransaction.Symbol);
					Security newSec = ModelService.GetSecurities().Find(s => s.Symbol == selectedTransaction.Symbol);
					if (selectedTransaction.TheTransaction.Security != null &&
						selectedTransaction.TheTransaction.Security.SecurityId != newSec.SecurityId)
					{
						// Set the security in the transaction.
						selectedTransaction.TheTransaction.Security = newSec;

						// Take away from the old position...
						Position oldPos = MyPositions.First(p => p.ThePosition.Security.SecurityId == selectedTransaction.TheTransaction.Security.SecurityId).ThePosition;
						oldPos.Quantity -= selectedTransaction.Quantity;
						ModelService.UpdatePosition(oldPos);

						// ... and give to the new position.
						Position newPos = MyPositions.FirstOrDefault(p => p.ThePosition.Security.SecurityId == newSec.SecurityId).ThePosition;
						if (newPos == null)
						{
							newPos = new Position();
							newPos.Security = newSec;
							newPos.Quantity = selectedTransaction.Quantity;
							TheAccount.Positions.Add(newPos);
							ModelService.UpdateAccount(TheAccount);
						}
						else
						{
							newPos.Quantity += selectedTransaction.Quantity;
							ModelService.UpdatePosition(newPos);
						}
					}
					// Changing from Null, so just add to position.
					else if (selectedTransaction.TheTransaction.Security == null)
					{
						// Set the security in the transaction.
						selectedTransaction.TheTransaction.Security = newSec;

						// TEST CODE
						selectedTransaction.TheTransaction.Date = selectedTransaction.Date;
						selectedTransaction.TheTransaction.Quantity = selectedTransaction.Quantity;
						selectedTransaction.TheTransaction.Fee = selectedTransaction.Fee;
						selectedTransaction.TheTransaction.Value = selectedTransaction.Value;
						ModelService.UpdateTransaction(selectedTransaction.TheTransaction);
						// END TEST CODE

						// Give to the new position?
						Position newPos = null;
						if (MyPositions.Count > 0)
						{
							newPos = MyPositions.FirstOrDefault(p => p.ThePosition.Security.SecurityId == newSec.SecurityId)?.ThePosition;
						}
						if (newPos == null)
						{
							newPos = new Position();
							newPos.Security = newSec;
							newPos.Quantity = selectedTransaction.Quantity;
							TheAccount.Positions.Add(newPos);
							ModelService.UpdateAccount(TheAccount);
						}
						else
						{
							newPos.Quantity += selectedTransaction.Quantity;
							ModelService.UpdatePosition(newPos);
						}
					}
					// The Security stays the same, so maybe something else changed.
					else if (selectedTransaction.TheTransaction.Security.SecurityId == newSec.SecurityId)
					{
						// Get the Position for this security.
						Position existingPos = MyPositions.First(p => p.ThePosition.Security.SecurityId == newSec.SecurityId).ThePosition;

						// Subtract the old Quantity.
						existingPos.Quantity -= selectedTransaction.TheTransaction.Quantity;

						// Add the new Quantity.
						existingPos.Quantity += selectedTransaction.Quantity;

						ModelService.UpdatePosition(existingPos);
					}
					else
					{
						// If we get here then there is a corner case I didn't cover.
						throw new NotImplementedException();
					}

					// TODO: Why does this line throw exception in case Buy.SameSecurity?
					//selectedTransaction.TheTransaction.Security = newSec;

					selectedTransaction.TheTransaction.Date = selectedTransaction.Date;
					selectedTransaction.TheTransaction.Quantity = selectedTransaction.Quantity;
					selectedTransaction.TheTransaction.Fee = selectedTransaction.Fee;
					selectedTransaction.TheTransaction.Value = selectedTransaction.Value;

					// TODO: Figure out why this throws an exception when changing the quantity of a Buy transaction.
					ModelService.UpdateTransaction(selectedTransaction.TheTransaction);
					// Change Security?
					// TODO: Assume Security already exists.

					// Change Account.Positions?
					// Case 1: Brand new position, means existing records not modified; new record added.
					// Case 2: Existing position, means one existing record will change but not the others.
					// Case 3: Change security from ABC to XYZ, means existing ABC record will change (or be
					//         deleted), XYZ record will be changed (if it exists) or added.
					// Brute force way is to simply delete all existing records and rebuild from scratch.

					// Change Price?
					// Change Position?
				}


				CalculateCashBalance();
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
				Transaction_VM tvm = new Transaction_VM();
				tvm.TheTransaction = t;
				MyTransactions.Add(tvm);
			}

			CalculateCashBalance();
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
					case 1:
					case 4:
					case 5:
						balance += xact.TheTransaction.Value;
						break;
					case 2:
					case 3:
						balance -= xact.TheTransaction.Value;
						break;
				}
				xact.CashBalance = balance;
			}

			// Update the Account value.
			TheAccount.Cash = balance;
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

				Date = theTransaction.Date;
				type = (ModelService.TransactionType)theTransaction.Type;
				//Symbol = (theTransaction.Security != null) ? theTransaction.Security.Symbol : null;
				//Symbol = theTransaction.Security ?? theTransaction.Security.Symbol;
				Symbol = theTransaction.Security?.Symbol;
				Quantity = theTransaction.Quantity;
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

		public List<string> SecSymbolList { get; set; }
		public string Symbol { get; set; }

		public decimal Quantity { get; set; }

		public decimal Fee { get; set; }

		public decimal Value { get; set; }

		public decimal CashBalance { get; set; } = 0.0M;

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

			// Get list of symbols for Security ComboBox.
			List<Security> tempSecs = ModelService.GetSecurities();
			SecSymbolList = new List<string>();
			foreach (Security sec in tempSecs)
			{
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
