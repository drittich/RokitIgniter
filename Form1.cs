using NAudio.Wave;

namespace RokitRoker
{
	public partial class Form1 : Form
	{
		WaveOutEvent outputDevice;
		AudioFileReader audioFile;

		public Form1()
		{
			InitializeComponent();
		}

		void btnPlay_Click(object sender, EventArgs e)
		{
			PlaySound();
		}

		void PlaySound()
		{
			if (outputDevice == null)
			{
				outputDevice = new WaveOutEvent();
				outputDevice.PlaybackStopped += OnPlaybackStopped;
			}
			if (audioFile == null)
			{
				audioFile = new AudioFileReader(Path.Combine(Environment.CurrentDirectory, "10Hz.wav"));
				outputDevice.Init(audioFile);
			}
			outputDevice.Play();
		}

		void OnPlaybackStopped(object sender, StoppedEventArgs args)
		{
			Cleanup();
		}

		void Cleanup()
		{
			if (outputDevice != null)
			{
				outputDevice.Dispose();
				outputDevice = null;
			}
			if (audioFile != null)
			{
				audioFile.Dispose();
				audioFile = null;
			}
		}

		async void Form1_LoadAsync(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Minimized;


			FormBorderStyle = FormBorderStyle.FixedSingle;
			MaximizeBox = false;

			PlaySound();

			var timer = new PeriodicTimer(TimeSpan.FromMinutes(25));
			while (await timer.WaitForNextTickAsync())
			{
				PlaySound();
			}
		}

		void btnExit_Click(object sender, EventArgs e)
		{
			Cleanup();
			Environment.Exit(0);	
		}

		void notifyIcon1_Click(object sender, EventArgs e)
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