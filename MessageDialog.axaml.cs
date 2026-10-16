using Avalonia.Controls;

namespace RokitIgniter
{
	/// <summary>
	/// The one small dialog, used for About and for the "already running" notice.
	/// </summary>
	public partial class MessageDialog : Window
	{
		public MessageDialog()
		{
			InitializeComponent();
			OkButton.Click += (s, e) => Close();
		}

		public string Message
		{
			get => MessageText.Text ?? "";
			set => MessageText.Text = value;
		}
	}
}
