using System.Media;
using System.Runtime.Versioning;

namespace RokitIgniter
{
	/// <summary>
	/// Plays the bundled wake tone with SoundPlayer, which loads the WAV and plays it on its own thread.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public sealed class WindowsTonePlayer : ITonePlayer
	{
		public void Play()
		{
			using (SoundPlayer player = new SoundPlayer(Path.Combine(AppContext.BaseDirectory, "10Hz.wav")))
				player.Play();
		}
	}
}
