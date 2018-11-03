using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Couatl3_Model;
using Microsoft.EntityFrameworkCore;

namespace Couatl3_WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			// TODO: See if there is a better place for this.
			// TODO: Will this change if/when there is a New File command?
			using (var db = new CouatlContext())
			{
				db.Database.Migrate();
			}

			//AccountListArea.Content = new AccountsView();
		}

		private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new CreateAccountDialogBox();
			dlg.Owner = this;

			dlg.ShowDialog();
		}
	}
}
