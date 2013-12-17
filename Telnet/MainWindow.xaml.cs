using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Telnet
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			if (!NewTab())
			{
				Close();
			}
		}

		public bool NewTab()
		{
			Connect connect = new Connect();

			if (connect.ShowDialog() == true)
			{
				TabItem item = new TabItem();

				try
				{
					var page = new CharacterPage(connect.CharacterName.Text, connect.CharacterPassword.Password);

					page.Quit += () => { ((CharacterPage)item.Content).Dispose(); CharacterTabs.SelectedIndex = 0; CharacterTabs.Items.Remove(item); };
					page.Page += () => { if (item != CharacterTabs.SelectedItem) { System.Media.SystemSounds.Asterisk.Play(); item.Background = Brushes.LightGreen; } else if (!this.IsActive) { System.Media.SystemSounds.Asterisk.Play(); } };
					page.Whisper += () => { if (item != CharacterTabs.SelectedItem) { item.Background = Brushes.LightBlue; } };

					item.Content = page;
					item.Header = connect.CharacterName.Text;

					CharacterTabs.Items.Insert(CharacterTabs.Items.Count - 1, item);

					return true;
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message, e.GetType().ToString());
					return false;
				}
			}

			return false;
		}

		private void Window_Closing_1(object sender, CancelEventArgs e)
		{
			foreach (var tab in CharacterTabs.Items)
			{
				var charPage = ((TabItem)tab).Content as CharacterPage;

				if (charPage != null)
				{
					charPage.Dispose();
				}
			}
		}

		private void CharacterTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedTab = CharacterTabs.SelectedItem as TabItem;
			var oldTab = e.RemovedItems.Count > 0 ? e.RemovedItems[0] as TabItem : null;

			if (oldTab != null && oldTab.Header.ToString() != "New")
			{
				((CharacterPage)oldTab.Content).ClearValue(BackgroundProperty);
			}

			if (selectedTab != null)
			{
				if (selectedTab.Header.ToString() == "New")
				{
					if (NewTab())
					{
						CharacterTabs.SelectedItem = CharacterTabs.Items[CharacterTabs.Items.Count - 2];
					}
					else if (CharacterTabs.Items.Count > 1)
					{
						try
						{
							CharacterTabs.SelectedItem = e.RemovedItems[0];
						}
						catch
						{
							CharacterTabs.SelectedIndex = 0;
						}
					}
					else
					{
						Close();
					}
				}
				else
				{
					((TabItem)CharacterTabs.SelectedItem).ClearValue(BackgroundProperty);

					for (int i = 0; i < CharacterTabs.Items.Count - 1; ++i)
					{
						var tab = CharacterTabs.Items[i] as TabItem;

						if (tab.Content != null)
						{
							((CharacterPage)tab.Content).ScrollToEnd();
						}
					}
				}
			}
		}

		private void Window_PreviewKeyDown_1(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.System || e.KeyboardDevice.Modifiers == ModifierKeys.Control)
			{
				int numTabs = CharacterTabs.Items.Count - 1;
				Key key = e.Key == Key.System ? e.SystemKey : e.Key;

				switch (key)
				{
					case Key.N:
						CharacterTabs.SelectedIndex = CharacterTabs.Items.Count - 1;
						e.Handled = true;
						break;

					case Key.D1:
						if (numTabs >= 1)
						{
							CharacterTabs.SelectedIndex = 0;
							e.Handled = true;
						}
						break;

					case Key.D2:
						if (numTabs >= 2)
						{
							CharacterTabs.SelectedIndex = 1;
							e.Handled = true;
						}
						break;

					case Key.D3:
						if (numTabs >= 3)
						{
							CharacterTabs.SelectedIndex = 2;
							e.Handled = true;
						}
						break;

					case Key.D4:
						if (numTabs >= 4)
						{
							CharacterTabs.SelectedIndex = 3;
							e.Handled = true;
						}
						break;

					case Key.D5:
						if (numTabs >= 5)
						{
							CharacterTabs.SelectedIndex = 4;
							e.Handled = true;
						}
						break;

					case Key.D6:
						if (numTabs >= 6)
						{
							CharacterTabs.SelectedIndex = 5;
							e.Handled = true;
						}
						break;

					case Key.D7:
						if (numTabs >= 7)
						{
							CharacterTabs.SelectedIndex = 6;
							e.Handled = true;
						}
						break;

					case Key.D8:
						if (numTabs >= 8)
						{
							CharacterTabs.SelectedIndex = 7;
							e.Handled = true;
						}
						break;

					case Key.D9:
						if (numTabs >= 9)
						{
							CharacterTabs.SelectedIndex = 8;
							e.Handled = true;
						}
						break;

					case Key.D0:
						if (numTabs >= 10)
						{
							CharacterTabs.SelectedIndex = 9;
							e.Handled = true;
						}
						break;
				}
			}
		}
	}
}
