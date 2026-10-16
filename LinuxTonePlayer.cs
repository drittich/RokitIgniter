using System.Diagnostics;

namespace RokitIgniter
{
	/// <summary>
	/// Plays the bundled wake tone by starting an external player (pw-play, paplay or aplay) without waiting for it.
	/// </summary>
	public sealed class LinuxTonePlayer : ITonePlayer
	{
		static readonly string[] PlayerNames = ["pw-play", "paplay", "aplay"];

		readonly string _playerPath;
		readonly string _wavPath = Path.Combine(AppContext.BaseDirectory, "10Hz.wav");

		LinuxTonePlayer(string playerPath)
		{
			_playerPath = playerPath;
		}

		/// <summary>
		/// Picks the first of pw-play, paplay and aplay found on PATH, or returns null if none is installed.
		/// </summary>
		public static LinuxTonePlayer? FindOnPath()
		{
			var dirs = (Environment.GetEnvironmentVariable("PATH") ?? "")
				.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
			foreach (var name in PlayerNames)
			{
				foreach (var dir in dirs)
				{
					var path = Path.Combine(dir, name);
					if (File.Exists(path))
						return new LinuxTonePlayer(path);
				}
			}
			return null;
		}

		public void Play()
		{
			// disposing the Process object doesn't stop the player
			using var process = Process.Start(new ProcessStartInfo(_playerPath)
			{
				ArgumentList = { _wavPath },
				UseShellExecute = false,
			});
		}
	}
}
