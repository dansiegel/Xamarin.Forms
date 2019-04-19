using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;
using WApplication = Windows.UI.Xaml.Application;

namespace Xamarin.Forms.Platform.UWP
{
	public static class FontExtensions
	{
		static Dictionary<string, FontFamily> FontFamilies = new Dictionary<string, FontFamily>();

		public static void ApplyFont(this Control self, Font font)
		{
			self.FontSize = font.UseNamedSize ? font.NamedSize.GetFontSize() : font.FontSize;
			self.FontFamily = font.ToFontFamily();
			self.FontStyle = font.FontAttributes.HasFlag(FontAttributes.Italic) ? FontStyle.Italic : FontStyle.Normal;
			self.FontWeight = font.FontAttributes.HasFlag(FontAttributes.Bold) ? FontWeights.Bold : FontWeights.Normal;
		}

		public static void ApplyFont(this TextBlock self, Font font)
		{
			var fontFamily = new FontFamily(font.FontFamily);
			self.FontSize = font.UseNamedSize ? font.NamedSize.GetFontSize() : font.FontSize;
			self.FontFamily = font.ToFontFamily();
			self.FontStyle = font.FontAttributes.HasFlag(FontAttributes.Italic) ? FontStyle.Italic : FontStyle.Normal;
			self.FontWeight = font.FontAttributes.HasFlag(FontAttributes.Bold) ? FontWeights.Bold : FontWeights.Normal;
		}

		public static void ApplyFont(this Windows.UI.Xaml.Documents.TextElement self, Font font)
		{
			self.FontSize = font.UseNamedSize ? font.NamedSize.GetFontSize() : font.FontSize;
			self.FontFamily = font.ToFontFamily();
			self.FontStyle = font.FontAttributes.HasFlag(FontAttributes.Italic) ? FontStyle.Italic : FontStyle.Normal;
			self.FontWeight = font.FontAttributes.HasFlag(FontAttributes.Bold) ? FontWeights.Bold : FontWeights.Normal;
		}

		internal static void ApplyFont(this Control self, IFontElement element)
		{
			self.FontSize = element.FontSize;
			self.FontFamily = element.FontFamily.ToFontFamily();
			self.FontStyle = element.FontAttributes.HasFlag(FontAttributes.Italic) ? FontStyle.Italic : FontStyle.Normal;
			self.FontWeight = element.FontAttributes.HasFlag(FontAttributes.Bold) ? FontWeights.Bold : FontWeights.Normal;
		}

		internal static double GetFontSize(this NamedSize size)
		{
			// These are values pulled from the mapped sizes on Windows Phone, WinRT has no equivalent sizes, only intents.
			switch (size)
			{
				case NamedSize.Default:
					return (double)WApplication.Current.Resources["ControlContentThemeFontSize"];
				case NamedSize.Micro:
					return 15.667;
				case NamedSize.Small:
					return 18.667;
				case NamedSize.Medium:
					return 22.667;
				case NamedSize.Large:
					return 32;
				case NamedSize.Body:
					return 14;
				case NamedSize.Caption:
					return 12;
				case NamedSize.Header:
					return 46;
				case NamedSize.Subtitle:
					return 20;
				case NamedSize.Title:
					return 24;
				default:
					throw new ArgumentOutOfRangeException("size");
			}
		}

		internal static bool IsDefault(this IFontElement self)
		{
			return self.FontFamily == null && self.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Label), true) && self.FontAttributes == FontAttributes.None;
		}

		public static FontFamily ToFontFamily(this Font font) => font.FontFamily.ToFontFamily();
		public static FontFamily ToFontFamily(this string fontFamily)
		{
			if (string.IsNullOrWhiteSpace(fontFamily))
				return (FontFamily)WApplication.Current.Resources["ContentControlThemeFontFamily"];

			//Return from Cache!
			if(FontFamilies.TryGetValue(fontFamily, out var f))
			{
				return f;
			}
			//Cach this puppy!
			var formated = string.Join(", ", GetAllFontPossibilities(fontFamily));
			var font = new FontFamily(formated);
			FontFamilies[fontFamily] = font;
			return font;
		}

		static IEnumerable<string> GetAllFontPossibilities(string fontFamily)
		{
			if(fontFamily.Contains("#"))
			{
				yield return fontFamily;
			}
			else
			{
				const string path = "Assets/Fonts/";
				string[] extensions = new[]
				{
					".ttf",
					".otf",
				};

				foreach (var ext in extensions)
				{
					var name = string.Join(' ', GetFontName(fontFamily));
					var formated = $"{path}{fontFamily}{ext}#{name}";
					yield return formated;
				}
			}
		}
		static IEnumerable<string> GetFontName(string fontFamily)
		{
			string currentString = "";
			char lastCharacter = ' ';
			var index = fontFamily.LastIndexOf("-");
			var cleansedstring = index > 0 ? fontFamily.Substring(0, index) : fontFamily;
			foreach (var c in cleansedstring)
			{
				if (c == '_' || c == '-')
				{
					yield return currentString;
					currentString = "";
				}
				else
				{
					if (char.IsUpper(c) && char.IsLower(lastCharacter))
					{
						yield return currentString;
						currentString = "";
					}
					currentString += c;
					lastCharacter = c;
				}
			}
			//Send what is left!
			if (!string.IsNullOrWhiteSpace(currentString))
				yield return currentString.Trim();

		}

		static (bool success, FontFamily fontFamily) LoadFontFamilyFromAssets(string fontFamily)
		{
			var font = new FontFamily(fontFamily);
			var success = fontFamily.Contains(font.Source);
			return (success, font);
		}

	}
}