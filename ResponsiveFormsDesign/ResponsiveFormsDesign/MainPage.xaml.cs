using System;
using Xamarin.Forms;

namespace ResponsiveFormsDesign
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

			SizeChanged += OnResized;
		}

		private void OnResized(object sender, EventArgs e)
		{
			Adaptive.WindowSize = new Size(Width, Height);
		}
	}
}
