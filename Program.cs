using Avalonia;

namespace RokitIgniter
{
	/// <summary>
	/// The composition root: the only place that picks adapters for the current OS.
	/// </summary>
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			// make sure only one instance of the application can be running at the same time: per session on Windows, as
			// before the port; across sessions on Linux, where each shell is its own session
			using var guard = new SingleInstanceGuard(currentSessionOnly: OperatingSystem.IsWindows());
			if (!guard.IsFirstInstance)
				return StartApp(args, () => new App { IconFileName = IconFileName() });

			var tonePlayer = CreateTonePlayer();
			if (tonePlayer is null)
			{
				Console.Error.WriteLine("RokitIgniter: no audio player found. Install pipewire-utils (pw-play), pulseaudio-utils (paplay) or alsa-utils (aplay).");
				return 1;
			}

			var igniter = new Igniter(tonePlayer, TimeProvider.System);
			return StartApp(args, () => new App { Igniter = igniter, IconFileName = IconFileName() });
		}

		static ITonePlayer? CreateTonePlayer()
		{
			if (OperatingSystem.IsWindows())
				return new WindowsTonePlayer();
			return LinuxTonePlayer.FindOnPath();
		}

		static string IconFileName() => OperatingSystem.IsWindows() ? "RokitIgniter.ico" : "RokitIgniter.png";

		static int StartApp(string[] args, Func<App> createApp) =>
			AppBuilder.Configure(createApp)
				.UsePlatformDetect()
				.LogToTrace()
				.StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnExplicitShutdown);
	}
}
