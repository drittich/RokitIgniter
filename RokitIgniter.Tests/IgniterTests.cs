using Microsoft.Extensions.Time.Testing;

namespace RokitIgniter.Tests
{
	public class IgniterTests
	{
		static readonly TimeSpan Minute = TimeSpan.FromMinutes(1);

		readonly FakeTimeProvider _time = new();

		[Fact]
		public async Task Construction_plays_exactly_one_wake_tone()
		{
			var player = new FakeTonePlayer(_time);
			using var igniter = new Igniter(player, _time);

			await player.WaitForCountAsync(1);
			Assert.Equal(new[] { 0.0 }, player.PlayMinutes);
		}

		[Fact]
		public async Task Next_wake_tone_plays_at_minute_25_and_not_before()
		{
			var player = new FakeTonePlayer(_time);
			using var igniter = new Igniter(player, _time);
			await player.WaitForCountAsync(1);

			_time.Advance(24 * Minute + TimeSpan.FromSeconds(59));
			_time.Advance(TimeSpan.FromSeconds(1));
			await player.WaitForCountAsync(2);

			Assert.Equal(new[] { 0.0, 25.0 }, player.PlayMinutes);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(4)]
		public async Task Over_N_intervals_N_plus_1_wake_tones_play(int n)
		{
			var player = new FakeTonePlayer(_time);
			using var igniter = new Igniter(player, _time);
			await player.WaitForCountAsync(1);

			for (int i = 1; i <= n; i++)
			{
				_time.Advance(25 * Minute);
				await player.WaitForCountAsync(i + 1);
			}

			Assert.Equal(n + 1, player.Count);
		}

		[Fact]
		public async Task Ignite_plays_a_wake_tone_right_away()
		{
			var player = new FakeTonePlayer(_time);
			using var igniter = new Igniter(player, _time);
			await player.WaitForCountAsync(1);
			_time.Advance(7 * Minute);

			await igniter.IgniteAsync().WaitAsync(TimeSpan.FromSeconds(5));
			await player.WaitForCountAsync(2);

			Assert.Equal(new[] { 0.0, 7.0 }, player.PlayMinutes);
		}

		[Fact]
		public async Task Ignite_at_minute_10_moves_next_wake_tone_to_minute_35()
		{
			var player = new FakeTonePlayer(_time);
			using var igniter = new Igniter(player, _time);
			await player.WaitForCountAsync(1);

			_time.Advance(10 * Minute);
			await igniter.IgniteAsync().WaitAsync(TimeSpan.FromSeconds(5));
			await player.WaitForCountAsync(2);

			_time.Advance(15 * Minute); // minute 25
			_time.Advance(10 * Minute); // minute 35
			await player.WaitForCountAsync(3);

			Assert.Equal(new[] { 0.0, 10.0, 35.0 }, player.PlayMinutes);
		}

		[Fact]
		public async Task Quick_ignites_each_play_one_wake_tone_and_leave_a_single_cycle()
		{
			var player = new FakeTonePlayer(_time);
			using var igniter = new Igniter(player, _time);
			await player.WaitForCountAsync(1);
			_time.Advance(5 * Minute);

			// overlapping clicks: start every Ignite before awaiting any of them
			var ignites = Enumerable.Range(0, 5).Select(_ => igniter.IgniteAsync()).ToArray();
			await Task.WhenAll(ignites).WaitAsync(TimeSpan.FromSeconds(5));
			await player.WaitForCountAsync(6);
			Assert.Equal(new[] { 0.0, 5, 5, 5, 5, 5 }, player.PlayMinutes);

			_time.Advance(25 * Minute); // minute 30
			await player.WaitForCountAsync(7);
			_time.Advance(25 * Minute); // minute 55
			await player.WaitForCountAsync(8);

			Assert.Equal(new[] { 0.0, 5, 5, 5, 5, 5, 30, 55 }, player.PlayMinutes);
		}

		[Fact]
		public async Task After_dispose_no_more_wake_tones_play()
		{
			var player = new FakeTonePlayer(_time);
			using var igniter = new Igniter(player, _time);
			await player.WaitForCountAsync(1);
			_time.Advance(25 * Minute);
			await player.WaitForCountAsync(2);

			await Task.Run(igniter.Dispose).WaitAsync(TimeSpan.FromSeconds(5));
			for (int i = 0; i < 4; i++)
				_time.Advance(25 * Minute);
			// a stray tone would play inline during Advance or shortly after on the thread pool
			await Task.Delay(100);

			Assert.Equal(new[] { 0.0, 25.0 }, player.PlayMinutes);
		}

		[Fact]
		public async Task Scheduled_wake_tones_keep_playing_after_the_tone_player_throws()
		{
			var player = new FakeTonePlayer(_time, throwOnPlay: number => number <= 2);
			using var igniter = new Igniter(player, _time);
			await player.WaitForCountAsync(1);

			_time.Advance(25 * Minute);
			await player.WaitForCountAsync(2);
			_time.Advance(25 * Minute);
			await player.WaitForCountAsync(3);

			Assert.Equal(new[] { 0.0, 25.0, 50.0 }, player.PlayMinutes);
		}
	}
}
