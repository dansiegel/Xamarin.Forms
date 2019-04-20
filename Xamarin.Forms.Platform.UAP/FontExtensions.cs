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
			//Try First if it is a system font;
			var formated = string.Join(", ", GetAllFontPossibilities(fontFamily));
			return new FontFamily(formated);
		}

		static IEnumerable<string> GetAllFontPossibilities(string fontFamily)
		{

			//Always send the base back
			yield return fontFamily;

			const string path = "Assets/Fonts/";
			string[] extensions = new[]
			{
				".ttf",
				".otf",
			};
			//If 
			var hasExtension = extensions.Any(fontFamily.Contains);
			if(hasExtension)
			{
				//Add the path as well for good measusure!
				if(!fontFamily.StartsWith(path))
				{
					yield return $"{path}{fontFamily}";
				}
			}
			else
			{

				var hashIndex = fontFamily.IndexOf('#');
				//Names sometimes don't have spaces that are required by UWP For example,  "CuteFont-Regular#CuteFont"  should be "CuteFont-Regular#Cute Font"
				var name = hashIndex > 0 ? fontFamily.Substring(hashIndex + 1) : fontFamily;
				name = string.Join(' ', GetFontName(name));
				var fontFamilyName = hashIndex > 0 ? fontFamily.Substring(0, hashIndex) : fontFamily;
				foreach (var ext in extensions)
				{
					var formated = $"{path}{fontFamilyName}{ext}#{name}";
					yield return formated;
				}
			}
		}
		static IEnumerable<string> GetFontName(string fontFamily)
		{
			if(fontFamily.Contains(' '))
			{
				yield return fontFamily;
				//We are done theyhave spaces, they have it handled.
				yield break;
			}
			string currentString = "";
			char lastCharacter = ' ';
			var index = fontFamily.LastIndexOf("-");
			bool multipleCaps = false;
			var cleansedstring = index > 0 ? fontFamily.Substring(0, index) : fontFamily;
			foreach (var c in cleansedstring)
			{
				//Always break on these characters
				if (c == '_' || c == '-')
				{
					yield return currentString;
					//Reset everything,
					currentString = "";
					lastCharacter = ' ';
					multipleCaps = false;
				}
				else
				{
					
					if (char.IsUpper(c))
					{
						//If the last character is lowercase, we are in a new CamelCase font
						if (char.IsLower(lastCharacter))
						{
							yield return currentString;
							currentString = "";
							lastCharacter = ' ';
						}
						else if (char.IsUpper(lastCharacter))
						{
							multipleCaps = true;
						}
					}

					//Detect multipl UpperCase letters so we can seperate things like PTSansNarrow into "PT Sans Narrow"
					else if (multipleCaps && currentString.Length > 1)
					{
						var last = currentString.Last();
						yield return currentString.Substring(0, currentString.Length - 1);
						//Reset everything so it doesnt do a space
						multipleCaps = false;
						lastCharacter = ' ';
						currentString = last.ToString();
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