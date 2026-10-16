namespace RokitIgniter
{
	/// <summary>
	/// Plays the wake tone once. Knows nothing about scheduling; may throw.
	/// </summary>
	public interface ITonePlayer
	{
		void Play();
	}
}
