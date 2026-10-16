namespace RokitIgniter
{
	/// <summary>
	/// Holds the named mutex "RokitIgniter" so only one instance runs per user. Keep it alive for the life of the process,
	/// and dispose it on the thread that acquired it.
	/// </summary>
	public sealed class SingleInstanceGuard : IDisposable
	{
		readonly Mutex _mutex;

		public bool IsFirstInstance { get; }

		/// <param name="currentSessionOnly">
		/// True scopes the mutex to the current session (Windows' Local\ namespace, so each Windows session runs its own
		/// instance). False shares it across all sessions of the current user (on Linux, so a launcher start and a
		/// terminal start see each other). Either way it stays private to the current user.
		/// </param>
		public SingleInstanceGuard(bool currentSessionOnly)
		{
			_mutex = new Mutex(false, "RokitIgniter", new NamedWaitHandleOptions { CurrentUserOnly = true, CurrentSessionOnly = currentSessionOnly });
			try
			{
				IsFirstInstance = _mutex.WaitOne(0, false);
			}
			catch (AbandonedMutexException)
			{
				// the previous holder died without releasing it; we own it now
				IsFirstInstance = true;
			}
		}

		public void Dispose()
		{
			if (IsFirstInstance)
				_mutex.ReleaseMutex();
			_mutex.Dispose();
		}
	}
}
