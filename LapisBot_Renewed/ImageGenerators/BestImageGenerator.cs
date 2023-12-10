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

        public static MagickImage Generate(BestDto best, string userId, bool usingHead)
        {
            MagickImage head;
            if (usingHead)
                head = new MagickImage(Program.apiOperator.ImageToPng("https://q.qlogo.cn/g?b=qq&nk=" + userId + "&s=640", Environment.CurrentDirectory + "/temp", "head.png"));
            else
            {
                head = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/best50_userhead_background.png");
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font.otf")
                    .FontPointSize(36)
                    .FillColor(new MagickColor(0, 52428, 65535, 6553))
                    .Text(3, 59, best.Username.Substring(0, 1))
                    .Draw(head);
            }
            head.Resize(65, 65);

            var image = GenerateBackground(best);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                .FontPointSize(36)
                .FillColor(new MagickColor(65535, 65535, 65535, 65535))
                .Text(74, 86, best.Username)
                .Draw(image);
            MagickImage ratingBackground = null;
            if (best.Rating <= 999)
            {

                ratingBackground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/score_background_regular.png");
                image.Composite(ratingBackground, 8, 25, CompositeOperator.Atop);
                new Drawables()
.Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
.FontPointSize(36)
.FillColor(new MagickColor(65535, 65535, 65535, 65535))
.Text(74, 150, best.Rating.ToString())
.Draw(image);
            }
            else if (best.Rating > 999 && best.Rating <= 1999)
            {

                ratingBackground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/score_background_blue.png");
                image.Composite(ratingBackground, 8, 25, CompositeOperator.Atop);
                new Drawables()
.Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
.FontPointSize(36)
.FillColor(new MagickColor(0, 15677, 23644))
.Text(74, 150, best.Rating.ToString())
.Draw(image);
            }
            else if (best.Rating > 1999 && best.Rating <= 3999)
            {

                ratingBackground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/score_background_green.png");
                image.Composite(ratingBackground, 8, 25, CompositeOperator.Atop);
                new Drawables()
.Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
.FontPointSize(36)
.FillColor(new MagickColor(16128, 24832, 6144))
.Text(74, 150, best.Rating.ToString())
.Draw(image);
            }
            else if (best.Rating > 3999 && best.Rating <= 6999)
            {
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(36)
                    .FillColor(new MagickColor(23296, 19968, 15872))
                    .Text(74, 150, best.Rating.ToString())
                    .Draw(image);
                ratingBackground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/score_background_yellow.png");
                image.Composite(ratingBackground, 8, 25, CompositeOperator.Atop);
            }
            else if (best.Rating > 6999 && best.Rating <= 9999)
            {

                ratingBackground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/score_background_red.png");
                image.Composite(ratingBackground, 73, 90, CompositeOperator.Atop);
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(36)
                    .FillColor(new MagickColor(65535, 46848, 46848))
                    .Text(74, 150, best.Rating.ToString())
                    .Draw(image);
            }
            else if (best.Rating > 9999 && best.Rating <= 11999)
            {

                ratingBackground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/score_background_purple.png");
                image.Composite(ratingBackground, 73, 90, CompositeOperator.Atop);
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(36)
                    .FillColor(new MagickColor(48128, 36864, 65535))
                    .Text(74, 150, best.Rating.ToString())
                    .Draw(image);
            }
            else if (best.Rating > 11999 && best.Rating <= 12999)
            {

                ratingBackground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/score_background_bronze.png");
                image.Composite(ratingBackground, 73, 90, CompositeOperator.Atop);
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(36)
                    .FillColor(new MagickColor(23644, 11565, 4369))
                    .Text(74, 150, best.Rating.ToString())
                    .Draw(image);
            }
            else if (best.Rating > 12999 && best.Rating <= 13999)
            {

                ratingBackground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/score_background_silver.png");
                image.Composite(ratingBackground, 73, 90, CompositeOperator.Atop);
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(36)
                    .FillColor(new MagickColor(5911, 5911, 5911))
                    .Text(74, 150, best.Rating.ToString())
                    .Draw(image);
            }
            else if (best.Rating > 13999 && best.Rating <= 14499)
            {

                ratingBackground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/score_background_gold.png");
                image.Composite(ratingBackground, 73, 90, CompositeOperator.Atop);
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(36)
                    .FillColor(new MagickColor(23296, 19968, 15872))
                    .Text(74, 150, best.Rating.ToString())
                    .Draw(image);
            }
            else if (best.Rating > 14499 && best.Rating <= 14999)
            {

                ratingBackground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/score_background_platinum.png");
                image.Composite(ratingBackground, 73, 90, CompositeOperator.Atop);
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(36)
                    .FillColor(new MagickColor(23296, 19968, 15872))
                    .Text(74, 150, best.Rating.ToString())
                    .Draw(image);
            }
            else if (best.Rating > 14999)
            {

                ratingBackground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/score_background_rainbow.png");
                image.Composite(ratingBackground, 73, 90, CompositeOperator.Atop);
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(36)
                    .FillColor(new MagickColor(6682, 0, 24185))
                    .Text(74, 150, best.Rating.ToString())
                    .Draw(image);
            }
            image.Composite(head, 8, 25, CompositeOperator.Atop);
            for (int i = 0; i < best.Charts.SdCharts.Length; i++)
            {
                var x = 0;
                var y = 0;
                if (i < 5)
                {
                    y = 299;
                    x = i * 350;
                }
                if (i > 4 && i < 10)
                {
                    y = 399;
                    x = (i - 5) * 350;
                }
                if (i > 9 && i < 15)
                {
                    y = 499;
                    x = (i - 10) * 350;
                }
                if (i > 14 && i < 20)
                {
                    y = 599;
                    x = (i - 15) * 350;
                }
                if (i > 19 && i < 25)
                {
                    y = 699;
                    x = (i - 20) * 350;
                }
                if (i > 24 && i < 30)
                {
                    y = 799;
                    x = (i - 25) * 350;
                }
                if (i > 29 && i < 35)
                {
                    y = 899;
                    x = (i - 30) * 350;
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
                    x = i * 350;
                }
                if (i > 4 && i < 10)
                {
                    y = 1161;
                    x = (i - 5) * 350;
                }
                if (i > 9 && i < 15)
                {
                    y = 1261;
                    x = (i - 10) * 350;
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
            if (System.IO.File.Exists(Environment.CurrentDirectory + @"/resources/covers/" + score.Id.ToString("00000") + ".png"))
                background = new MagickImage(Environment.CurrentDirectory + @"/resources/covers/" + score.Id.ToString("00000") + ".png");
            else
                background = new MagickImage(Environment.CurrentDirectory + @"/resources/covers/01000.png");

            background.Scale(100, 100);
            var image = new MagickImage("xc:white", new MagickReadSettings() { Width = 350, Height = 100 });
            image.Composite(foreground, 0, 0, CompositeOperator.Atop);
            image.Composite(background, 0, 0, CompositeOperator.Atop);

            var info = new MagickImage("xc:transparent", new MagickReadSettings() { Width = 350, Height = 100 });
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-heavy.otf")
                .FontPointSize(13)
                .FillColor(fontColor)
                .TextAlignment(TextAlignment.Left)
                .Text(114, 23, score.Type)
                .Draw(info);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                .FontPointSize(20)
                .FillColor(fontColor)
                .TextAlignment(TextAlignment.Left)
                .Text(135, 28, score.Title)
                .Draw(info);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-heavy.otf")
                .FontPointSize(20)
                .FillColor(fontColor)
                .TextAlignment(TextAlignment.Left)
                .Text(114, 50, Math.Round(score.Achievements, 2, MidpointRounding.ToNegativeInfinity).ToString("0.00") + "%")
                .Draw(info);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-heavy.otf")
                .FontPointSize(20)
                .FillColor(fontColor)
                .TextAlignment(TextAlignment.Left)
                .Text(114, 70, score.DifficultyFactor.ToString("0.0") + "·" + score.Rating.ToString())
                .Draw(info);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                .FontPointSize(15)
                .FillColor(fontColor)
                .TextAlignment(TextAlignment.Left)
                .Text(114, 88, "#" + rank.ToString() + "·ID " + score.Id.ToString())
                .Draw(info);
            MagickImage _image = null;
            if (score.rate == InfoCommand.Rate.SSS)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/sss.png");
            else if (score.rate == InfoCommand.Rate.SSSp)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/sss_plus.png");
            else if (score.rate == InfoCommand.Rate.SS)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/ss.png");
            else if (score.rate == InfoCommand.Rate.SSp)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/ss_plus.png");
            else if (score.rate == InfoCommand.Rate.Sp)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/s_plus.png");
            else if (score.rate == InfoCommand.Rate.S)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/s.png");
            else if (score.rate == InfoCommand.Rate.AAA)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/aaa.png");
            else if (score.rate == InfoCommand.Rate.AA)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/aa.png");
            else if (score.rate == InfoCommand.Rate.A)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/a.png");
            else if (score.rate == InfoCommand.Rate.BBB)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/bbb.png");
            else if (score.rate == InfoCommand.Rate.BB)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/bb.png");
            else if (score.rate == InfoCommand.Rate.B)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/b.png");
            else if (score.rate == InfoCommand.Rate.C)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/c.png");
            else if (score.rate == InfoCommand.Rate.D)
                _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings_hd/d.png");
            if (_image != null)
            {
                //if (_image.BaseHeight == 44)
                info.Composite(_image, info.BaseWidth - _image.BaseWidth - 14, 75 - _image.BaseHeight, CompositeOperator.Blend);
                //if (_image.BaseHeight == 38)
                //info.Composite(_image, info.BaseWidth - _image.BaseWidth - 20, 88 - _image.BaseHeight, CompositeOperator.Blend);
            }
            info.Composite(new MagickImage(Environment.CurrentDirectory + @"/resources/best50/gradient.png"), 0, 0, Channels.Alpha);
            image.Composite(info, 0, 0, CompositeOperator.Atop);

            return image;
        }

        public static MagickImage GenerateBackground(BestDto best)
        {
            var image = new MagickImage("xc:white", new MagickReadSettings() { Width = 1750, Height = 1440 });
            var background = new MagickImage("xc:transparent", new MagickReadSettings() { Width = 1750, Height = 1440 });
            if (best.Charts.SdCharts.Length != 0)
                background = new MagickImage(Environment.CurrentDirectory + @"/resources/covers/" + best.Charts.SdCharts[0].Id.ToString("00000") + ".png");
            background.Scale(75, 75);
            background.GaussianBlur(10);
            background.Resize(1750, 1750);
            image.Composite(background, 0, 0, CompositeOperator.Atop);
            var foreground = new MagickImage(Environment.CurrentDirectory + @"/resources/best50/best50_background.png");
            image.Composite(foreground, 0, 0, CompositeOperator.Atop);
            return image;
        }
    }
}

