using System.Media;

namespace RokitIgniter
{
	public class Igniter : IDisposable
	{
		CancellationTokenSource? _cts;
		Task? _timerTask;
		readonly TimeSpan interval = TimeSpan.FromMinutes(25);

		public Igniter()
		{
			_timerTask = DoStart(interval);
		}

		async Task DoStart(TimeSpan interval)
		{
			_cts = new();
			try
			{
				using var timer = new PeriodicTimer(interval);
				do
				{
					PlaySound();
				}
				while (await timer.WaitForNextTickAsync(_cts.Token));
			}
			catch (OperationCanceledException)
			{
			}
		}

		void PlaySound()
		{
			using (SoundPlayer player = new SoundPlayer(Path.Combine(Environment.CurrentDirectory, "10Hz.wav")))
				player.Play();
		}

		public async Task IgniteAsync()
		{
			if (_cts is not null)
			{
				_cts.Cancel();
				await _timerTask!;
				_cts.Dispose();
			}

			_timerTask = DoStart(interval);
		}

		public void Dispose()
		{
			if (_cts is not null)
			{
				_cts.Cancel();
				_timerTask!.Wait();
				_cts.Dispose();
			}
			GC.SuppressFinalize(this);
		}
	}
}
