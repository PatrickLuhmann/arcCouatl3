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
		private List<GlobalPosition_VM> positions;
		public List<GlobalPosition_VM> Positions
		{
			get
			{
				return positions;
			}
			private set
			{
				positions = value;
			}
		}

		public GlobalPositions_VM()
		{
			positions = new List<GlobalPosition_VM>();

			// Get the open accounts.
			List<Account> accounts = ModelService.GetAccounts(true);

			// Get the positions for each account and accumulate them in Positions.
			foreach (var account in accounts)
			{
				List<Position> account_positions = account.Positions;

				foreach (Position acctPos in account_positions)
				{
					GlobalPosition_VM pos = positions.Find(p => p.ThePosition.SecurityId == acctPos.SecurityId);
					if (pos == null)
					{
						string secName = ModelService.GetSecurityNameFromId(acctPos.SecurityId);
						string secSym = ModelService.GetSymbolFromId(acctPos.SecurityId);
						GlobalPosition_VM glPos = new GlobalPosition_VM()
						{
							ThePosition = acctPos,
							SecurityName = secName,
							Symbol = secSym,
						};
						// Clear invalid properties.
						glPos.ThePosition.Account = null;
						glPos.ThePosition.AccountId = 0;
						positions.Add(glPos);
					}
					else
					{
						pos.ThePosition.Quantity += acctPos.Quantity;
					}
				}
			}

			// Determine the current value of each position, now
			// that we have the final quantity for each of them.
			foreach (var pos in positions)
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
	}
}
