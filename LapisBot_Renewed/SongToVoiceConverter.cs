using System;
using System.IO;
using System.Resources;
using Xamarin.Forms;

namespace LapisBot_Renewed
{
	public class SongToVoiceConverter
	{
		public static string Convert(int id)
		{
			if (!File.Exists(AppContext.BaseDirectory + "temp/" + id + ".silk"))
			{
				if (OperatingSystem.IsMacOS())
				{
					var command = "ffmpeg -y -i " + AppContext.BaseDirectory + "resources/tracks/" + id + ".mp3" + " -acodec pcm_s16le -f s16le -ac 1 " + AppContext.BaseDirectory + "temp/tmp.pcm \n" + AppContext.BaseDirectory + "resources/silk_codec-macos " + "pts -i " + AppContext.BaseDirectory + "temp/tmp.pcm" + " -s 44100 -o " + AppContext.BaseDirectory + "temp/" + id + ".silk";

					ApiOperator.Bash(command);
				}
				if (OperatingSystem.IsLinux())
				{
					var command = "ffmpeg -y -i " + AppContext.BaseDirectory + "resources/tracks/" + id + ".mp3" + " -acodec pcm_s16le -f s16le -ac 1 " + AppContext.BaseDirectory + "temp/tmp.pcm \n" + AppContext.BaseDirectory + "resources/silk_codec-linux-x64 " + "pts -i " + AppContext.BaseDirectory + "temp/tmp.pcm" + " -s 44100 -o " + AppContext.BaseDirectory + "temp/" + id + ".silk";

					ApiOperator.Bash(command);
				}
			}

			return AppContext.BaseDirectory + "temp/" + id + ".silk";
			//return AppContext.BaseDirectory + "resources/tracks/" + id + ".mp3";

        }
	}
}

