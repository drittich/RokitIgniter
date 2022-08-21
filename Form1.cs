using System.Media;
using System.Reflection;

namespace RokitIgniter
{
	public partial class Form1 : Form
	{
		CancellationTokenSource? _cts;
		Task? _timerTask;
		readonly TimeSpan interval = TimeSpan.FromMinutes(25);

		public Form1()
		{
			InitializeComponent();
		}

		void Form1_Load(object sender, EventArgs e)
		{
			var versionString = string.Join(".", Assembly.GetEntryAssembly()?.GetName().Version!.ToString().Split(".").SkipLast(1)!);
			Text = "Rokit Igniter " + versionString;
			WindowState = FormWindowState.Minimized;
			// this removes it from Alt-Tab when in tray
			Hide();

			FormBorderStyle = FormBorderStyle.FixedSingle;
			MaximizeBox = false;

			_timerTask = DoStart(interval, lblNextIgnition);
		}

		async Task DoStart(TimeSpan interval, Label lbl)
		{
			_cts = new();
			try
			{
				using var timer = new PeriodicTimer(interval);
				do
				{
					lbl.Text = DateTime.Now.Add(interval).ToString("h:mm:ss tt");
					PlaySound();
				}
				while (await timer.WaitForNextTickAsync(_cts.Token));
			}
			catch (OperationCanceledException)
			{
			}
		}

		async void BtnIgnite_Click(object sender, EventArgs e)
		{
			if (_cts is not null)
			{
				_cts.Cancel();
				await _timerTask!;
				_cts.Dispose();
			}

			_timerTask = DoStart(interval, lblNextIgnition);
		}

		void PlaySound()
		{
			using (SoundPlayer player = new SoundPlayer(Path.Combine(Environment.CurrentDirectory, "10Hz.wav")))
				player.Play();
		}

		async void BtnExit_Click(object sender, EventArgs e)
		{
			if (_cts is not null)
			{
				_cts.Cancel();
				await _timerTask!;
				_cts.Dispose();
			}

			Environment.Exit(0);
		}

		void NotifyIcon1_Click(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Normal;
			notifyIcon1.Visible = false;
			ShowInTaskbar = true;
		}

		void Form1_Resize(object sender, EventArgs e)
		{
			notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;

			if (FormWindowState.Minimized == WindowState)
			{
				notifyIcon1.Visible = true;
				FormBorderStyle = FormBorderStyle.FixedToolWindow;
				ShowInTaskbar = false;
			}
			else if (FormWindowState.Normal == WindowState)
			{
				notifyIcon1.Visible = false;
				FormBorderStyle = FormBorderStyle.FixedSingle;
			}
		}
	}
	
}