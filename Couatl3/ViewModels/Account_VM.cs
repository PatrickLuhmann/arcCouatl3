﻿using Couatl3.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;

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

		public bool InAddState { get; set; } = false;
		public RelayCommand AddTransactionCmd { get; set; }
		public RelayCommand DeleteTransactionCmd { get; set; }
		public RelayCommand UpdateTransactionCmd { get; set; }

		private void AddTransaction()
		{
			Transaction t = new Transaction();
			Transaction_VM tvm = new Transaction_VM();
			tvm.TheTransaction = t;
			MyTransactions.Add(tvm);
		}

		public void DeleteTransaction()
		{

		}
		
		public void UpdateTransaction()
		{
			if (InAddState)
			{
			}
			else
			{

			}
		}

		public Account_VM(Account acct)
		{
			TheAccount = acct;

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

	public class Transaction_VM
	{
		private Transaction theTransaction;
		public Transaction TheTransaction
		{
			get { return theTransaction; }
			set
			{
				theTransaction = value;
				//TODO: Implement method to calculate cash balance per transaction.
				CashBalance = 3.4M;
			}
		}

		public decimal CashBalance { get; set; } = 1.2M;
	}
}
