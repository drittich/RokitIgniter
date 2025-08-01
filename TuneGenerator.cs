using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace TuneGenerator;

public class TuneGenerator : IDisposable
{
	private readonly WaveOutEvent? _waveOutEvent;

	public TuneGenerator()
	{
		var signalGenerator = new SignalGenerator
		{ 
			Gain = 0.2, 
			Frequency = 5,
			Type = SignalGeneratorType.Sin
		};
		_waveOutEvent = new WaveOutEvent();
		_waveOutEvent.Init(signalGenerator);
	}

	public void Play()
	{
		_waveOutEvent?.Play();
	}

	public void Stop()
	{
		_waveOutEvent?.Stop();
	}

	public void Dispose()
	{
		if (_waveOutEvent is not null)
		{
			_waveOutEvent.Stop();
			_waveOutEvent.Dispose();
		}
		GC.SuppressFinalize(this);
	}
}