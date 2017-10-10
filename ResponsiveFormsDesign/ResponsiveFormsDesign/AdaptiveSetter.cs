using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace ResponsiveFormsDesign
{
	public class AdaptiveSetter : Element
	{
		private static readonly MethodInfo applyMethod;
		private static readonly MethodInfo unapplyMethod;

		static AdaptiveSetter()
		{
			// TODO: these members are internal

			var setterType = typeof(Setter);
			applyMethod = setterType.GetTypeInfo().GetDeclaredMethod("Apply");
			unapplyMethod = setterType.GetTypeInfo().GetDeclaredMethod("UnApply");
		}

		private Setter realSetter;

		public string Target { get; set; }

		public string Property { get; set; }

		public object Value { get; set; }

		public void Apply(Element element)
		{
			var target = GetTarget(element);
			if (target == null)
				return;

			if (realSetter == null)
				realSetter = CreateRealSetter(target);
			if (realSetter == null)
				return;

			applyMethod.Invoke(realSetter, new object[] { target, false });
		}

		public void UnApply(Element element)
		{
			if (realSetter == null)
				return;

			var target = GetTarget(element);

			unapplyMethod.Invoke(realSetter, new object[] { target, false });
		}

		private BindableObject GetTarget(Element element)
		{
			BindableObject target = null;
			if (string.IsNullOrEmpty(Target))
				target = element;
			else
				target = element.FindByName<BindableObject>(Target);
			return target;
		}

		private Setter CreateRealSetter(BindableObject target)
		{
			BindableProperty prop = null;

			// TODO: find the property was are setting

			var targetType = target.GetType();
			while (targetType != null && prop == null)
			{
				var part = Property.Trim();

				//if (part == string.Empty)
				//	throw new FormatException("Path contains an empty part");

				//int index = -1;
				//int lbIndex = part.IndexOf('[');
				//if (lbIndex != -1)
				//{
				//	int rbIndex = part.LastIndexOf(']');
				//	if (rbIndex == -1)
				//		throw new FormatException("Indexer did not contain closing bracket");

				//	int argLength = rbIndex - lbIndex - 1;
				//	if (argLength == 0)
				//		throw new FormatException("Indexer did not contain arguments");

				//	string argString = part.Substring(lbIndex + 1, argLength);
				//	index = int.Parse(argString);

				//	part = part.Substring(0, lbIndex);
				//	part = part.Trim();
				//}

				var props = targetType.GetTypeInfo().DeclaredFields.Where(fi => fi.IsStatic && fi.IsPublic && fi.FieldType == typeof(BindableProperty));
				prop = props.Select(p => p.GetValue(null) as BindableProperty).FirstOrDefault(p => p.PropertyName == part);

				if (prop == null)
					targetType = targetType.GetTypeInfo().BaseType;
			}

			// TODO: convert the text XAML value into the strong type

			var onPlatformType = typeof(OnPlatform<>).MakeGenericType(prop.ReturnType);
			var platformsMember = onPlatformType.GetTypeInfo().GetDeclaredProperty("Platforms");

			var onPlatform = Activator.CreateInstance(onPlatformType);
			var platforms = (IList)platformsMember.GetValue(onPlatform);

			platforms.Add(new On { Platform = new[] { Device.RuntimePlatform }, Value = Value });

			// TODO: the setter that Xamarin.Forms understands

			return new Setter { Property = prop, Value = onPlatform };
		}
	}
}
