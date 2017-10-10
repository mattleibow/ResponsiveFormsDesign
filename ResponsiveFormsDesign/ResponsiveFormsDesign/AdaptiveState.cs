using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace ResponsiveFormsDesign
{
	[ContentProperty("Setters")]
	public class AdaptiveState : BindableObject
	{
		public static readonly BindableProperty MinWindowWidthProperty =
			BindableProperty.Create(
				nameof(MinWindowWidth),
				typeof(double),
				typeof(AdaptiveState),
				0.0,
				propertyChanged: OnMinWindowWidthChanged);

		public double MinWindowWidth
		{
			get { return (double)GetValue(MinWindowWidthProperty); }
			set { SetValue(MinWindowWidthProperty, value); }
		}

		private static void OnMinWindowWidthChanged(BindableObject bindable, object oldValue, object newValue)
		{
			Debug.WriteLine("OnMinWindowWidthChanged: " + newValue);
		}

		public IList<AdaptiveSetter> Setters { get; } = new List<AdaptiveSetter>();

		public void Apply(Element element)
		{
			Debug.WriteLine("Applying: " + MinWindowWidth);
			foreach (var setter in Setters)
			{
				setter.Apply(element);
			}
		}

		public void UnApply(Element element)
		{
			Debug.WriteLine("UnApplying: " + MinWindowWidth);
			foreach (var setter in Setters)
			{
				setter.UnApply(element);
			}
		}
	}
}
