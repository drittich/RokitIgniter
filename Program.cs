namespace RokitIgniter
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// we don't need a form, so we can just run the ApplicationContext directly
			Application.Run(new MyCustomApplicationContext());
		}

		public class MyCustomApplicationContext : ApplicationContext
		{
			private NotifyIcon notifyIcon;
			private Igniter igniter;

			public MyCustomApplicationContext()
			{
				notifyIcon = new();
				igniter = new();
				SetupNotifyIcon();
			}

			private void SetupNotifyIcon()
			{
				notifyIcon!.Text = "Rokit Igniter";
				notifyIcon.Icon = new("RokitIgniter.ico");
				notifyIcon.ContextMenuStrip = new();
				notifyIcon.ContextMenuStrip.Items.Add("Ignite", null, async (s, e) => await Ignite_ClickAsync(s, e));
				notifyIcon.ContextMenuStrip.Items.Add("About", null, ShowAboutMessageBox_Click);
				notifyIcon.ContextMenuStrip.Items.Add("Exit", null, ExitApplication_Click);

				notifyIcon!.Click += async (s, e) =>
				{
					if (e is MouseEventArgs me && me.Button == MouseButtons.Right)
					{
						return;
					}

					await igniter.IgniteAsync();
				};

				notifyIcon.Visible = true;
			}

			private void ExitApplication_Click(object? sender, EventArgs e)
			{
				Environment.Exit(0);
			}

			async Task Ignite_ClickAsync(object? sender, EventArgs? e)
			{
				await igniter!.IgniteAsync();
			}

			void ShowAboutMessageBox_Click(object? sender, EventArgs? e)
			{
				var packageVersion = typeof(Program).Assembly.GetName().Version;
				MessageBox.Show($"RokitIgniter v{packageVersion}", "RokitIgniter", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
	}
}