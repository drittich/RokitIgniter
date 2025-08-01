using System.Media;

namespace TuneGenerator;

public class TuneGenerator : IDisposable
{
	private CancellationTokenSource? _cts;
	private Task? _timerTask;
	private readonly TimeSpan _interval = TimeSpan.FromSeconds(11);

	public TuneGenerator()
	{
		_timerTask = DoStart(_interval);
	}

	private async Task DoStart(TimeSpan interval)
	{
		_cts = new CancellationTokenSource();
		try
		{
			using var timer = new PeriodicTimer(interval);
			do
			{
				PlaySound();
			} while (await timer.WaitForNextTickAsync(_cts.Token));
		}
		catch (OperationCanceledException)
		{
			// ignored
		}
	}

	private static void PlaySound()
	{
		using var player = new SoundPlayer(Path.Combine(Environment.CurrentDirectory, "1Hz.wav"));
		player.Play();
	}

	public async Task Restart()
	{
		if (_cts is not null)
		{
			await _cts.CancelAsync();
			await _timerTask!;
			_cts.Dispose();
		}

		_timerTask = DoStart(_interval);
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