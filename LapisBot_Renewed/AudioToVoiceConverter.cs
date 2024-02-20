using System;
using System.IO;
using System.Resources;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace LapisBot_Renewed
{
	public class AudioToVoiceConverter
	{
		public string ConvertAudio(string path)
		{
			return ConvertCore(path);
		}
		
		public string ConvertSong(int id)
		{
			var outputPath = AppContext.BaseDirectory + "temp/" + id + ".silk";
			if (!File.Exists(outputPath))
			{
				return ConvertCore(AppContext.BaseDirectory + "resources/tracks/" + id + ".mp3", outputPath);
			}
			return outputPath;
        }

		public static string ConvertCore(string path)
		{
			var regex = new Regex(".mp3$");
			var outputPath = regex.Replace(path, ".silk");
			if (OperatingSystem.IsMacOS())
			{
				var command = "ffmpeg -y -i " + path + " -acodec pcm_s16le -f s16le -ac 1 " + AppContext.BaseDirectory + "temp/tmp.pcm \n" + AppContext.BaseDirectory + "resources/silk_codec-macos " + "pts -i " + AppContext.BaseDirectory + "temp/tmp.pcm" + " -s 44100 -o " + outputPath;
				ApiOperator.Bash(command);
			}
			if (OperatingSystem.IsLinux())
			{
				var command = "ffmpeg -y -i " + path + " -acodec pcm_s16le -f s16le -ac 1 " + AppContext.BaseDirectory + "temp/tmp.pcm \n" + AppContext.BaseDirectory + "resources/silk_codec-linux-x64 " + "pts -i " + AppContext.BaseDirectory + "temp/tmp.pcm" + " -s 44100 -o " + outputPath;
				ApiOperator.Bash(command);
			}

			return outputPath;
		}
		
		public static string ConvertCore(string path, string outputPath)
		{
			if (OperatingSystem.IsMacOS())
			{
				var command = "ffmpeg -y -i " + path + " -acodec pcm_s16le -f s16le -ac 1 " + AppContext.BaseDirectory + "temp/tmp.pcm \n" + AppContext.BaseDirectory + "resources/silk_codec-macos " + "pts -i " + AppContext.BaseDirectory + "temp/tmp.pcm" + " -s 44100 -o " + outputPath;
				ApiOperator.Bash(command);
			}
			if (OperatingSystem.IsLinux())
			{
				var command = "ffmpeg -y -i " + path + " -acodec pcm_s16le -f s16le -ac 1 " + AppContext.BaseDirectory + "temp/tmp.pcm \n" + AppContext.BaseDirectory + "resources/silk_codec-linux-x64 " + "pts -i " + AppContext.BaseDirectory + "temp/tmp.pcm" + " -s 44100 -o " + outputPath;
				ApiOperator.Bash(command);
			}

			return outputPath;
		}
	}
}

