using Couatl3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couatl3.ViewModels
{
	public class GlobalPositions_VM
	{
		private List<GlobalPosition_VM> _Positions;
		public List<GlobalPosition_VM> Positions
		{
			get
			{
				return _Positions;
			}
			private set
			{
				_Positions = value;
			}
		}

		public GlobalPositions_VM()
		{
			_Positions = new List<GlobalPosition_VM>();

			// Get the open accounts.
			List<Account> accounts = ModelService.GetAccounts(true);

			// Get the positions for each account and accumulate them in Positions.
			foreach (var account in accounts)
			{
				List<Position> account_positions = account.Positions;

				foreach (Position acctPos in account_positions)
				{
					GlobalPosition_VM pos = _Positions.Find(p => p.ThePosition.SecurityId == acctPos.SecurityId);
					if (pos == null)
					{
						string secName = ModelService.GetSecurityNameFromId(acctPos.SecurityId);
						string secSym = ModelService.GetSymbolFromId(acctPos.SecurityId);
						GlobalPosition_VM glPos = new GlobalPosition_VM(acctPos.SecurityId)
						{
							ThePosition = acctPos,
							SecurityName = secName,
							Symbol = secSym,
						};
						// Clear invalid properties.
						glPos.ThePosition.Account = null;
						glPos.ThePosition.AccountId = 0;
						_Positions.Add(glPos);
					}
					else
					{
						pos.ThePosition.Quantity += acctPos.Quantity;
					}
				}
			}

			// Determine the current value of each position, now
			// that we have the final quantity for each of them.
			foreach (var pos in _Positions)
			{
				pos.Value = pos.ThePosition.Quantity * ModelService.GetNewestPrice(pos.ThePosition.SecurityId);
			}
		}
	}

	public class GlobalPosition_VM
	{
		public Position ThePosition { get; set; }

		public string SecurityName { get; set; }

		public string Symbol { get; set; }

		public decimal Value { get; set; }

		private List<SecurityLot_VM> _SecLots = new List<SecurityLot_VM>();
		public List<SecurityLot_VM> SecLots
		{
			get
			{
				return _SecLots;
			}
			set
			{

			}
		}

		public GlobalPosition_VM(int secId)
		{
			// Get the Transactions for the security.
			List<Transaction> xacts = ModelService.GetTransactions().Where(tr => tr.SecurityId == secId).ToList();

			// Create a SecLot for each Buy.
			foreach (Transaction xact in xacts)
			{
				if (xact.Type == (int)ModelService.TransactionType.Buy)
				{
					_SecLots.Add(new SecurityLot_VM
					{
						Date = xact.Date,
						Quantity = xact.Quantity,
						TotalCostBasis = xact.Value,
						PerShareCostBasis = xact.Value / xact.Quantity,
						// we will calculate the rest once we have final quantity.
					});
				}
			}

			// Back out the Sells.
			// TODO: Use LotAssignments (using FIFO for now).
			// TODO: This code doesn't take Account boundaries into account. The LotAssignment object
			// provides an equivalent. Until then need to do it manually.
			foreach (Transaction xact in xacts)
			{
				if (xact.Type == (int)ModelService.TransactionType.Sell)
				{
					decimal qty = xact.Quantity;
					do
					{
						if (qty < _SecLots[0].Quantity)
						{
							// Must do this before Quantity is altered.
							// NOTE: Must do multiplication before division to get correct value.
							_SecLots[0].TotalCostBasis = ((_SecLots[0].Quantity - qty) * _SecLots[0].TotalCostBasis) / _SecLots[0].Quantity;
							// NOTE: Per-share cost basis doesn't change due to this Sell.
							// Subtract the shares from the affected Lot.
							_SecLots[0].Quantity -= qty;

							// This Sell has been completely handled, so break out of the loop.
							break;
						}
						else
						{
							// The affected Lot is completely consumed, so remove it.
							// Adjust qty for comparison to the next lot in the list.
							qty -= _SecLots[0].Quantity;
							_SecLots.RemoveAt(0);
						}
					} while (true);
				}
			}

			// Calculate current values using the most recent price.
			decimal price = ModelService.GetNewestPrice(secId);
			foreach (SecurityLot_VM secLot in _SecLots)
			{
				secLot.Value = secLot.Quantity * price;
				secLot.NetGain = secLot.Value - secLot.TotalCostBasis;
				secLot.PercentGain = (secLot.Value - secLot.TotalCostBasis) / secLot.TotalCostBasis;
			}
		}
	}

	public class SecurityLot_VM
	{
		public DateTime Date { get; set; }

		//public string Symbol { get; set; }

		//public string Name { get; set; }

		public decimal Quantity { get; set; }

		public decimal TotalCostBasis { get; set; }

		public decimal PerShareCostBasis { get; set; }

		public decimal Value { get; set; }
		
		// Total gain.
		// TODO: If ever get daily prices, add a Day Gain property.
		public decimal NetGain { get; set; }

		public decimal PercentGain { get; set; }

		// TODO: Add a Notes property?
	}
}
