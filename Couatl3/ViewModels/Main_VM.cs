using GalaSoft.MvvmLight;
using Couatl3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couatl3.ViewModels
{
    public class Main_VM : ViewModelBase
    {
		public Main_VM()
		{
			// TODO: Move this to a ViewModel.
			using (var db = new CouatlContext())
			{
				db.Database.Migrate();
			}
		}
	}
}
