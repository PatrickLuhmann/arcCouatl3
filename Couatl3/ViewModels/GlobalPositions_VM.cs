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
		private List<Position> positions;
		public List<Position> Positions
		{
			get
			{
				// TODO: Do we need to recalculate every time? Or is once at construction time sufficient?
				return positions;
			}
			private set
			{
				positions = value;
			}
		}
		
		public GlobalPositions_VM()
		{
			positions = new List<Position>();

			// Get the open accounts.
			List<Account> accounts = ModelService.GetAccounts(true);

			// Get the positions for each account and accumulate them in Positions.
			foreach (var account in accounts)
			{
				List<Position> account_positions = account.Positions;

				foreach (Position acctPos in account_positions)
				{
					Position pos = positions.Find(p => p.SecurityId == acctPos.SecurityId);
					if (pos == null)
					{
						positions.Add(new Position
						{
							PositionId = acctPos.PositionId,
							Quantity = acctPos.Quantity,
							SecurityId = acctPos.SecurityId,
							// This Position can aggregate multiple Accounts, so ignore those fields.
						});
					}
					else
					{
						pos.Quantity += acctPos.Quantity;
					}
				}
			}
		}
	}
}
