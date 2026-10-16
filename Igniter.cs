namespace RokitIgniter
{
	/// <summary>
	/// Owns the cycle: plays the wake tone on construction and then every interval.
	/// Ignite plays the wake tone right away and restarts the cycle; Dispose stops it.
	/// </summary>
	public class Igniter : IDisposable
	{
		readonly ITonePlayer _tonePlayer;
		readonly TimeProvider _timeProvider;
		readonly TimeSpan _interval;
		readonly object _lock = new();
		CancellationTokenSource _cts;
		Task _cycle;
		bool _disposed;

		public Igniter(ITonePlayer tonePlayer, TimeProvider timeProvider, TimeSpan? interval = null)
		{
			_tonePlayer = tonePlayer;
			_timeProvider = timeProvider;
			_interval = interval ?? TimeSpan.FromMinutes(25);
			_cts = new();
			_cycle = RunCycleAsync(_cts.Token);
		}

		// Plays the wake tone synchronously before the first await, so the tone has played by the time this returns.
		// ConfigureAwait(false) keeps the loop off the caller's synchronization context, so a synchronous Dispose
		// on the UI thread can't deadlock waiting for it.
		async Task RunCycleAsync(CancellationToken token)
		{
			try
			{
				using var timer = new PeriodicTimer(_interval, _timeProvider);
				do
				{
					PlayWakeTone();
				}
				while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false));
			}
			catch (OperationCanceledException) { }
		}

		void PlayWakeTone()
		{
			try
			{
				_tonePlayer.Play();
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"RokitIgniter: wake tone failed: {ex}");
			}
		}

		public async Task IgniteAsync()
		{
			CancellationTokenSource oldCts;
			Task oldCycle;
			// swap the cycle under the lock so overlapping calls each play one tone and leave a single cycle
			lock (_lock)
			{
				if (_disposed)
					return;

				oldCts = _cts;
				oldCycle = _cycle;
				oldCts.Cancel();
				_cts = new();
				_cycle = RunCycleAsync(_cts.Token);
			}

			await oldCycle.ConfigureAwait(false);
			oldCts.Dispose();
		}

		public void Dispose()
		{
			CancellationTokenSource cts;
			Task cycle;
			lock (_lock)
			{
				if (_disposed)
					return;

				_disposed = true;
				cts = _cts;
				cycle = _cycle;
				cts.Cancel();
			}

			cycle.Wait();
			cts.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
