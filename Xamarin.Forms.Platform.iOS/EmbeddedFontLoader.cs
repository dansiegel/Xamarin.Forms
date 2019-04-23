using System;
using System.Diagnostics;
using CoreGraphics;
using CoreText;
using Foundation;

namespace Xamarin.Forms.Platform.iOS
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		public bool LoadFont(EmbeddedFont font)
		{
			try
			{
				var data = NSData.FromStream(font.ResourceStream);

				var provider = new CGDataProvider(data);
				var cGFont = CGFont.CreateFromProvider(provider);
				if (CTFontManager.RegisterGraphicsFont(cGFont, out var error))
					return true;
				Debug.WriteLine(error.Description);
			}
			catch(Exception ex)
			{
				Debug.WriteLine(ex);
			}
			return false;
		}
	}
}
