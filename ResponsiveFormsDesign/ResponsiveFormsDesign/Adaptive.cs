using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace ResponsiveFormsDesign
{
	public class Adaptive : Behavior<VisualElement>
	{
		private static readonly WindowSizeTracker tracker = new WindowSizeTracker();

		private class WindowSizeTracker : BindableObject
		{
			public static readonly BindableProperty WindowSizeProperty =
				BindableProperty.Create(
					nameof(WindowSize),
					typeof(Size),
					typeof(Adaptive),
					Size.Zero,
					BindingMode.TwoWay,
					propertyChanged: OnWindowSizeChanged);

			public Size WindowSize
			{
				get => (Size)GetValue(WindowSizeProperty);
				set => SetValue(WindowSizeProperty, value);
			}

			private static void OnWindowSizeChanged(BindableObject bindable, object oldValue, object newValue)
			{
				var track = bindable as WindowSizeTracker;
				track.WindowSizeChanged?.Invoke((Size)newValue);
			}

			public event Action<Size> WindowSizeChanged;
		}

		private WeakReference<VisualElement> element;

		public static readonly BindableProperty StatesProperty =
			BindableProperty.Create(
				nameof(States),
				typeof(AdaptiveStates),
				typeof(Adaptive),
				new AdaptiveStates(),
				propertyChanged: OnStatesChanged);

		public static readonly BindableProperty CurrentStateProperty =
			BindableProperty.Create(
				nameof(CurrentState),
				typeof(AdaptiveState),
				typeof(Adaptive),
				null,
				propertyChanged: OnCurrentStateChanged);

		public AdaptiveState CurrentState
		{
			get => (AdaptiveState)GetValue(CurrentStateProperty);
			set => SetValue(CurrentStateProperty, value);
		}

		public AdaptiveStates States
		{
			get => (AdaptiveStates)GetValue(StatesProperty);
			set => SetValue(StatesProperty, value);
		}

		public static Size WindowSize
		{
			get => tracker.WindowSize;
			set => tracker.WindowSize = value;
		}

		protected override void OnAttachedTo(VisualElement bindable)
		{
			base.OnAttachedTo(bindable);

			element = new WeakReference<VisualElement>(bindable);
			tracker.WindowSizeChanged += OnWindowSizeChanged;
		}

		protected override void OnDetachingFrom(VisualElement bindable)
		{
			tracker.WindowSizeChanged -= OnWindowSizeChanged;

			base.OnDetachingFrom(bindable);
		}

		private void OnWindowSizeChanged(Size newSize)
		{
			CurrentState = States.OrderBy(s => s.MinWindowWidth).LastOrDefault(s => s.MinWindowWidth < newSize.Width);
		}

		private static void OnStatesChanged(BindableObject bindable, object oldValue, object newValue)
		{
			Debug.WriteLine($"OnStatesChanged");

			var adaptive = ((Adaptive)bindable);

			if (oldValue is AdaptiveStates oldList)
				oldList.CollectionChanged -= adaptive.OnStatesItemsChanged;

			if (oldValue is AdaptiveStates newList)
				newList.CollectionChanged += adaptive.OnStatesItemsChanged;

			adaptive.OnWindowSizeChanged(WindowSize);
		}

		private void OnStatesItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Debug.WriteLine($"OnStatesItemsChanged");

			OnWindowSizeChanged(WindowSize);
		}

		private static void OnCurrentStateChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var adaptive = bindable as Adaptive;
			if (adaptive.element.TryGetTarget(out var element))
			{
				var oldState = oldValue as AdaptiveState;
				var newState = newValue as AdaptiveState;

				oldState?.UnApply(element);
				newState?.Apply(element);
			}
		}
	}
}
