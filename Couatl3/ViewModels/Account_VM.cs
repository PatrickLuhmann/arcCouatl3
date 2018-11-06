using Couatl3.Models;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace Couatl3.ViewModels
{
	public class Account_VM : ViewModelBase
	{
		public Account TheAccount { get; set; }
		public ObservableCollection<Position_VM> Positions { get; set; }
		public ObservableCollection<Transaction_VM> Transactions { get; set; }

		public Account_VM(Account acct)
		{
			TheAccount = acct;

			foreach (var p in TheAccount.Positions)
			{
				Position_VM pvm = new Position_VM();
				pvm.ThePosition = p;
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

	public class Transaction_VM { }
}
