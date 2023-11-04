using System;
using System.Reflection;
using ImageMagick;

namespace LapisBot_Renewed
{
	public class BestImageGenerator
	{
		public BestImageGenerator()
		{

		}

		public static MagickImage Generate(BestDto best, string userId)
		{
            var head = new MagickImage(Program.apiOperator.ImageToPng("https://q.qlogo.cn/g?b=qq&nk=" + userId + "&s=640", Environment.CurrentDirectory + "/temp", "head.png"));
            head.Resize(65, 65);

            var image = GenerateBackground(best);
            new Drawables()
				.Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
				.FontPointSize(36)
				.FillColor(new MagickColor(65535, 65535, 65535, 65535))
				.Text(74, 86, best.Username)
				.Draw(image);
            new Drawables()
				.Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
				.FontPointSize(36)
				.FillColor(new MagickColor(65535, 65535, 65535, 65535))
				.Text(74, 150, best.Rating.ToString())
				.Draw(image);

            image.Composite(head, 8, 25, CompositeOperator.Atop);

			for (int i = 0; i < best.Charts.SdCharts.Length; i++)
			{
				var x = 0;
				var y = 0;
				if (i < 5)
				{
					y = 299;
					x = i * 200;
				}
                if (i > 4 && i < 10)
                {
                    y = 399;
                    x = (i - 5) * 200;
                }
                if (i > 9 && i < 15)
                {
                    y = 499;
                    x = (i - 10) * 200;
                }
                if (i > 14 && i < 20)
                {
                    y = 599;
                    x = (i - 15) * 200;
                }
                if (i > 19 && i < 25)
                {
                    y = 699;
                    x = (i - 20) * 200;
                }
                if (i > 24 && i < 30)
                {
                    y = 799;
                    x = (i - 25) * 200;
                }
                if (i > 29 && i < 35)
                {
                    y = 899;
                    x = (i - 30) * 200;
                }
				image.Composite(GenerateItem(best.Charts.SdCharts[i], i + 1), x, y, CompositeOperator.Atop);
            }

            for (int i = 0; i < best.Charts.DxCharts.Length; i++)
            {
                var x = 0;
                var y = 0;
                if (i < 5)
                {
                    y = 1061;
                    x = i * 200;
                }
                if (i > 4 && i < 10)
                {
                    y = 1161;
                    x = (i - 5) * 200;
                }
                if (i > 9 && i < 15)
                {
                    y = 1261;
                    x = (i - 10) * 200;
                }
                image.Composite(GenerateItem(best.Charts.DxCharts[i], i + 1), x, y, CompositeOperator.Atop);
            }

            return image;
        }

        public static MagickImage GenerateItem(BestDto.ScoreDto score, int rank)
        {
            var difficulty = string.Empty;
            var fontColor = new MagickColor();

            switch (score.LevelIndex)
            {
                case 0:
                    difficulty = "bas";
                    fontColor = new MagickColor(16128, 24832, 6144);
                    break;
                case 1:
                    difficulty = "avd";
                    fontColor = new MagickColor(23296, 19968, 15872);
                    break;
                case 2:
                    difficulty = "exp";
                    fontColor = new MagickColor(65535, 46848, 46848);
                    break;
                case 3:
                    difficulty = "mas";
                    fontColor = new MagickColor(48128, 36864, 65535);
                    break;
                case 4:
                    difficulty = "re";
                    fontColor = new MagickColor(34560, 29184, 46848);
                    break;
            }
            var foreground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/b50_item_foreground_" + difficulty + ".png");
            MagickImage background;
            if (System.IO.File.Exists(Environment.CurrentDirectory + @"/resources/static/mai/cover/" + score.Id.ToString("00000") + ".png"))
                background = new MagickImage(Environment.CurrentDirectory + @"/resources/static/mai/cover/" + score.Id.ToString("00000") + ".png");
            else
                background = new MagickImage(Environment.CurrentDirectory + @"/resources/static/mai/cover/01000.png");

            background.Scale(100, 100);
            var image = new MagickImage("xc:white", new MagickReadSettings() { Width = 200, Height = 100 });
            image.Composite(background, 100, 0, CompositeOperator.Atop);
            image.Composite(foreground, 0, 0, CompositeOperator.Atop);

            var info = new MagickImage("xc:transparent", new MagickReadSettings() { Width = 200, Height = 100 });
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-heavy.otf")
                .FontPointSize(13)
                .FillColor(fontColor)
                .TextAlignment(TextAlignment.Left)
                .Text(14, 23, score.Type)
                .Draw(info);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                .FontPointSize(20)
                .FillColor(fontColor)
                .TextAlignment(TextAlignment.Left)
                .Text(35, 28, score.Title)
                .Draw(info);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-heavy.otf")
                .FontPointSize(20)
                .FillColor(fontColor)
                .TextAlignment(TextAlignment.Left)
                .Text(14, 50, score.Achievements.ToString("0.00") + "%")
                .Draw(info);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-heavy.otf")
                .FontPointSize(20)
                .FillColor(fontColor)
                .TextAlignment(TextAlignment.Left)
                .Text(14, 70, score.DifficultyFactor.ToString("0.0") + "·" + score.Rating.ToString())
                .Draw(info);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                .FontPointSize(15)
                .FillColor(fontColor)
                .TextAlignment(TextAlignment.Left)
                .Text(14, 88, rank.ToString() + "·ID " + score.Id.ToString())
                .Draw(info);
            info.Composite(new MagickImage(Environment.CurrentDirectory + @"/resources/best50/gradient.png"), 0, 0, Channels.Alpha);
            image.Composite(info, 0, 0, CompositeOperator.Atop);

            return image;
        }

		public static MagickImage GenerateBackground(BestDto best)
		{
            var image = new MagickImage("xc:white", new MagickReadSettings() { Width = 1000, Height = 1440 });
            var background = new MagickImage("xc:transparent", new MagickReadSettings() { Width = 1000, Height = 1440 });
            if (best.Charts.SdCharts.Length != 0)
				background = new MagickImage(Environment.CurrentDirectory + @"/resources/static/mai/cover/" + best.Charts.SdCharts[0].Id.ToString("00000") + ".png");
			background.Scale(50, 50);
			background.GaussianBlur(10);
			background.Resize(1000, 1000);
			image.Composite(background, 0, 0, CompositeOperator.Atop);
            var foreground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/best50_background.png");
            image.Composite(foreground, 0, 0, CompositeOperator.Atop);
            return image;
		}
	}
}

