using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace RokitIgniter
{
	/// <summary>
	/// The tray-only Avalonia app. With an Igniter it shows the tray icon and menu; without one (another instance is
	/// already running) it only shows the notice and shuts down when the notice closes.
	/// </summary>
	public partial class App : Application
	{
		public Igniter? Igniter { get; init; }

		/// <summary>The tray and dialog icon file name, relative to AppContext.BaseDirectory.</summary>
		public string IconFileName { get; init; } = "RokitIgniter.png";

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				var icon = new WindowIcon(Path.Combine(AppContext.BaseDirectory, IconFileName));
				if (Igniter is null)
					ShowAlreadyRunningNotice(desktop, icon);
				else
					SetupTrayIcon(desktop, Igniter, icon);
			}

			base.OnFrameworkInitializationCompleted();
		}

		static void ShowAlreadyRunningNotice(IClassicDesktopStyleApplicationLifetime desktop, WindowIcon icon)
		{
			var notice = new MessageDialog { Message = "RokitIgniter is already running.", Icon = icon };
			notice.Closed += (s, e) => desktop.Shutdown();
			notice.Show();
		}

		void SetupTrayIcon(IClassicDesktopStyleApplicationLifetime desktop, Igniter igniter, WindowIcon icon)
		{
			var trayIcon = new TrayIcon
			{
				Icon = icon,
				ToolTipText = "Rokit Igniter",
				Menu = new NativeMenu(),
			};

			var igniteItem = new NativeMenuItem("Ignite");
			igniteItem.Click += async (s, e) => await igniter.IgniteAsync();
			var aboutItem = new NativeMenuItem("About");
			aboutItem.Click += (s, e) => ShowAbout(icon);
			var exitItem = new NativeMenuItem("Exit");
			exitItem.Click += (s, e) =>
			{
				igniter.Dispose();
				trayIcon.IsVisible = false;
				desktop.Shutdown();
			};
			trayIcon.Menu.Items.Add(igniteItem);
			trayIcon.Menu.Items.Add(aboutItem);
			trayIcon.Menu.Items.Add(exitItem);

			// left-click on Windows, activate on Linux; a Windows right-click only opens the menu
			trayIcon.Clicked += async (s, e) => await igniter.IgniteAsync();

			TrayIcon.SetIcons(this, [trayIcon]);
		}

		static void ShowAbout(WindowIcon icon)
		{
			var packageVersion = typeof(Program).Assembly.GetName().Version;
			new MessageDialog { Message = $"RokitIgniter v{packageVersion}", Icon = icon }.Show();
		}
	}
}
