using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;

namespace Xamarin.Forms.Platform.Android
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		//TODO: Maybe change this;

		static EmbeddedFontLoader()
		{

		}
		public bool LoadFont(EmbeddedFont font)
		{
			var tmpdir = Path.GetTempPath();
			var filePath = Path.Combine(tmpdir, font.FontName);
			if (File.Exists(filePath))
				return true;
			try
			{
				using (var fileStream = File.Create(filePath))
				{
					font.ResourceStream.CopyTo(fileStream);
				}
				return true;
			}
			catch(Exception ex)
			{
				Debug.WriteLine(ex);
				File.Delete(filePath);
			}
			return false;
		}
	}
}
