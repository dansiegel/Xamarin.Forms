using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Windows.Storage;

namespace Xamarin.Forms.Platform.UAP
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		const string fontCacheFolderName = "fonts";
		public (bool success, string filePath) LoadFont(EmbeddedFont font)
		{
			try
			{
				var t = ApplicationData.Current.LocalFolder.CreateFolderAsync(fontCacheFolderName, CreationCollisionOption.OpenIfExists);
				var tmpdir = t.AsTask().Result;

				var file = tmpdir.TryGetItemAsync(font.FontName).AsTask().Result;
				string filePath = "";
				if (file != null)
				{
					filePath = file.Path;
					return (true, CleanseFilePath(filePath));
				}

				try
				{

					var f = tmpdir.CreateFileAsync(font.FontName).AsTask().Result;
					filePath = f.Path;
					using (var fileStream = File.Open(f.Path, FileMode.Open))
					{
						font.ResourceStream.CopyTo(fileStream);
					}
					return (true, CleanseFilePath(filePath));
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					File.Delete(filePath);
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			return (false, null);
		}

		static string CleanseFilePath(string filePath)
		{
			var fontName = Path.GetFileName(filePath);
			filePath = Path.Combine("local", fontCacheFolderName, fontName);
			var baseUri = new Uri("ms-appdata://");
			var uri = new Uri(baseUri, filePath);
			var relativePath = uri.ToString().TrimEnd('/');
			return relativePath;
		}
	}
}
