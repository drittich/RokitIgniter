using Microsoft.Extensions.Time.Testing;

namespace RokitIgniter.Tests
{
	/// <summary>
	/// Records the virtual time of every wake tone the Igniter asks for.
	/// </summary>
	sealed class FakeTonePlayer : ITonePlayer
	{
		readonly FakeTimeProvider _time;
		readonly object _lock = new();
		readonly List<DateTimeOffset> _plays = new();
		readonly Func<int, bool> _throwOnPlay;

		/// <param name="throwOnPlay">Given the 1-based play number, whether that play throws (after being recorded).</param>
		public FakeTonePlayer(FakeTimeProvider time, Func<int, bool>? throwOnPlay = null)
		{
			_time = time;
			_throwOnPlay = throwOnPlay ?? (_ => false);
		}

		public void Play()
		{
			int number;
			lock (_lock)
			{
				_plays.Add(_time.GetUtcNow());
				number = _plays.Count;
			}
			if (_throwOnPlay(number))
				throw new InvalidOperationException($"wake tone {number} failed on purpose");
		}

		public int Count
		{
			get { lock (_lock) return _plays.Count; }
		}

		/// <summary>Minutes after the fake clock's start at which each wake tone played.</summary>
		public double[] PlayMinutes
		{
			get { lock (_lock) return _plays.Select(p => (p - _time.Start).TotalMinutes).ToArray(); }
		}

		/// <summary>Waits until at least <paramref name="count"/> wake tones have played, failing after 5 s.</summary>
		public async Task WaitForCountAsync(int count)
		{
			var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
			while (Count < count)
			{
				if (DateTime.UtcNow > deadline)
					Assert.Fail($"Expected {count} wake tones within 5 s, but {Count} played at minutes [{string.Join(", ", PlayMinutes)}]");
				await Task.Delay(10);
			}
		}
	}
}
