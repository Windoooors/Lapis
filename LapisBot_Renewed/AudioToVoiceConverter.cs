using System;
using System.IO;
using System.Resources;
using System.Text.RegularExpressions;

namespace LapisBot_Renewed
{
	public class AudioToVoiceConverter
	{
		public string GetSongPath(int id)
		{
			var outputPath = AppContext.BaseDirectory + "temp/" + id + ".silk";
			if (!File.Exists(outputPath))
			{
				return AppContext.BaseDirectory + "resource/tracks/" + id + ".mp3";
			}
			return outputPath;
        }
	}
}

