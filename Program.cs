namespace TuneGenerator;

internal static class Program
{
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	private static void Main()
	{
		//make sure only one instance of the application can be running at the same time
		using var mutex = new Mutex(false, "TuneGenerator");
		if (!mutex.WaitOne(0, false))
		{
			MessageBox.Show("TuneGenerator is already running.", "TuneGenerator", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		// To customize application configuration such as set high DPI settings or default font,
		// see https://aka.ms/applicationconfiguration.
		Application.SetHighDpiMode(HighDpiMode.SystemAware);
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		// we don't need a form, so we can just run the ApplicationContext directly
		Application.Run(new TuneGeneratorAppContext());
	}

	private class TuneGeneratorAppContext : ApplicationContext
	{
		private readonly NotifyIcon _notifyIcon;
		private readonly TuneGenerator _tuneGenerator;

		public TuneGeneratorAppContext()
		{
			_notifyIcon = new NotifyIcon();
			_tuneGenerator = new TuneGenerator();

			SetupNotifyIcon();
		}

		private void SetupNotifyIcon()
		{
			_notifyIcon.Text = "TuneGenerator";
			_notifyIcon.Icon = new Icon("TuneGenerator.ico");
			_notifyIcon.ContextMenuStrip = new ContextMenuStrip
			{
				Items =
				{
					new ToolStripMenuItem("Restart", null, async (_, _) => await _tuneGenerator.Restart()),
					new ToolStripMenuItem("Exit", null, ExitApplication_Click)

				}
			};

			_notifyIcon.Click += async (_, e) =>
			{
				if (e is MouseEventArgs { Button: MouseButtons.Right })
				{
					return;
				}

				await _tuneGenerator.Restart();
			};

			_notifyIcon.Visible = true;
		}

		private static void ExitApplication_Click(object? sender, EventArgs e)
		{
			Environment.Exit(0);
		}
	}
}