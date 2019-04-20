using System.Diagnostics;
using System.Linq;
using Xamarin.Forms.Internals;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public static partial class FontExtensions
	{
		static readonly string DefaultFontName = UIFont.SystemFontOfSize(12).Name;

		public static UIFont ToUIFont(this Font self) => ToNativeFont(self);

		internal static UIFont ToUIFont(this IFontElement element) => ToNativeFont(element);

		static UIFont _ToNativeFont(string family, float size, FontAttributes attributes)
		{
			var bold = (attributes & FontAttributes.Bold) != 0;
			var italic = (attributes & FontAttributes.Italic) != 0;

			if (family != null && family != DefaultFontName)
			{
				try
				{
					UIFont result = null;
					if (UIFont.FamilyNames.Contains(family))
					{
						var descriptor = new UIFontDescriptor().CreateWithFamily(family);

						if (bold || italic)
						{
							var traits = (UIFontDescriptorSymbolicTraits)0;
							if (bold)
								traits = traits | UIFontDescriptorSymbolicTraits.Bold;
							if (italic)
								traits = traits | UIFontDescriptorSymbolicTraits.Italic;

							descriptor = descriptor.CreateWithTraits(traits);
							result = UIFont.FromDescriptor(descriptor, size);
							if (result != null)
								return result;
						}
					}
					var cleasnedFont = CleanseFontName(family);
					result = UIFont.FromName(cleasnedFont, size);
					if (result != null)
						return result;
				}
				catch
				{
					Debug.WriteLine("Could not load font named: {0}", family);
				}
			}

			if (bold && italic)
			{
				var defaultFont = UIFont.SystemFontOfSize(size);

				var descriptor = defaultFont.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Bold | UIFontDescriptorSymbolicTraits.Italic);
				return UIFont.FromDescriptor(descriptor, 0);
			}

			if (italic)
				return UIFont.ItalicSystemFontOfSize(size);

			if (bold)
				return UIFont.BoldSystemFontOfSize(size);

			return UIFont.SystemFontOfSize(size);
		}
		static string CleanseFontName(string fontName)
		{
			var hashIndex = fontName.IndexOf("#", System.StringComparison.Ordinal);
			if (hashIndex < 1)
				return fontName;
			var font = fontName.Substring(0, hashIndex);
			var extensions = new[]
			{
				".ttf",
				".otf",
			};
			foreach(var ext in extensions)
			{
				if(font.EndsWith(ext, System.StringComparison.Ordinal))
				{
					return font.Substring(0, font.Length - 4);
				}
			}
			return font;
		}
	}
}