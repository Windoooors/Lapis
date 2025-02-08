using System;
using System.IO;
using LapisBot_Renewed.Operations.ImageOperation;
using LapisBot_Renewed.GroupCommands.MaiCommands;

namespace LapisBot_Renewed.ImageGenerators
{
    public class BestImageGenerator
    {
        public string Generate(BestDto best, string userId, bool usingHead, bool isCompressed)
        {
            Image head;
            if (usingHead)
                head = Program.apiOperator.UrlToImage("https://q.qlogo.cn/g?b=qq&nk=" + userId + "&s=640"); 
            else
            {
                head = new Image(Environment.CurrentDirectory +
                                       @"/resource/best50/best50_userhead_background.png");
                head.DrawText(best.Username.Substring(0, 1), new Color(0, 0.8f, 1, 0.1f), 36, FontWeight.Regular, 3,
                    59);
            }

            head.Resize(65, 65);

            var image = GenerateBackground(best);
            var nameForm = new Image(500, 65);
            
            nameForm.DrawText(best.Username, Color.White, 36, FontWeight.Light, 2, 59);
            
            image.DrawImage(nameForm, 74, 25);
            
            nameForm.Dispose();
            
            if (best.Rating <= 999)
            {
                using var ratingBackground =
                    new Image(Environment.CurrentDirectory + @"/resource/best50/score_background_regular.png");
                image.DrawImage(ratingBackground, 73, 90);
                image.DrawText(best.Rating.ToString(), Color.White, 36, FontWeight.Light, 74, 150);
            }
            else if (best.Rating > 999 && best.Rating <= 1999)
            {
                using var ratingBackground =
                    new Image(Environment.CurrentDirectory + @"/resource/best50/score_background_blue.png");
                image.DrawImage(ratingBackground, 73, 90);
                image.DrawText(best.Rating.ToString(), new Color(0, 0.24f, 0.36f, 1), 36, FontWeight.Light, 74, 150);
            }
            else if (best.Rating > 1999 && best.Rating <= 3999)
            {
                using var ratingBackground =
                    new Image(Environment.CurrentDirectory + @"/resource/best50/score_background_green.png");
                image.DrawImage(ratingBackground, 73, 90);
                image.DrawText(best.Rating.ToString(), new Color(0.25f, 0.38f, 0.09f, 1), 36, FontWeight.Light, 74, 150);
            }
            else if (best.Rating > 3999 && best.Rating <= 6999)
            {
                using var ratingBackground =
                    new Image(Environment.CurrentDirectory + @"/resource/best50/score_background_yellow.png");
                image.DrawImage(ratingBackground, 73, 90);
                image.DrawText(best.Rating.ToString(), new Color(0.36f, 0.30f, 0.24f, 1), 36, FontWeight.Light, 74, 150);
            }
            else if (best.Rating > 6999 && best.Rating <= 9999)
            {
                using var ratingBackground =
                    new Image(Environment.CurrentDirectory + @"/resource/best50/score_background_red.png");
                image.DrawImage(ratingBackground, 73, 90);
                image.DrawText(best.Rating.ToString(), new Color(1, 0.71f, 0.71f, 1), 36, FontWeight.Light, 74, 150);
            }
            else if (best.Rating > 9999 && best.Rating <= 11999)
            {
                using var ratingBackground =
                    new Image(Environment.CurrentDirectory + @"/resource/best50/score_background_purple.png");
                image.DrawImage(ratingBackground, 73, 90);
                image.DrawText(best.Rating.ToString(), new Color(0.73f, 0.56f, 1f, 1), 36, FontWeight.Light, 74, 150);
            }
            else if (best.Rating > 11999 && best.Rating <= 12999)
            {
                using var ratingBackground =
                    new Image(Environment.CurrentDirectory + @"/resource/best50/score_background_bronze.png");
                image.DrawImage(ratingBackground, 73, 90);
                image.DrawText(best.Rating.ToString(), new Color(0.36f, 0.18f, 0.07f, 1), 36, FontWeight.Light, 74, 150);
            }
            else if (best.Rating > 12999 && best.Rating <= 13999)
            {
                using var ratingBackground =
                    new Image(Environment.CurrentDirectory + @"/resource/best50/score_background_silver.png");
                image.DrawImage(ratingBackground, 73, 90);
                image.DrawText(best.Rating.ToString(), new Color(0.1f, 0.1f, 0.1f, 1), 36, FontWeight.Light, 74, 150);
            }
            else if (best.Rating > 13999 && best.Rating <= 14499)
            {
                using var ratingBackground =
                    new Image(Environment.CurrentDirectory + @"/resource/best50/score_background_gold.png");
                image.DrawImage(ratingBackground, 73, 90);
                image.DrawText(best.Rating.ToString(), new Color(0.36f, 0.3f, 0.24f, 1), 36, FontWeight.Light, 74, 150);
            }
            else if (best.Rating > 14499 && best.Rating <= 14999)
            {
                using var ratingBackground =
                    new Image(Environment.CurrentDirectory + @"/resource/best50/score_background_platinum.png");
                image.DrawImage(ratingBackground, 73, 90);
                image.DrawText(best.Rating.ToString(), new Color(0.36f, 0.3f, 0.24f, 1), 36, FontWeight.Light, 74, 150);
            }
            else if (best.Rating > 14999)
            {
                using var ratingBackground =
                    new Image(Environment.CurrentDirectory + @"/resource/best50/score_background_rainbow.png");
                image.DrawImage(ratingBackground, 73, 90);
                image.DrawText(best.Rating.ToString(), new Color(0.1f, 0f, 0.37f, 1), 36, FontWeight.Light, 74, 150);
            }

            image.DrawImage(head, 8, 25);
            
            head.Dispose();
            
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

                var item = GenerateItem(best.Charts.SdCharts[i], i + 1);

                image.DrawImage(item, x, y);
                
                item.Dispose();
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

                var item = GenerateItem(best.Charts.DxCharts[i], i + 1);

                image.DrawImage(item, x, y);
                
                item.Dispose();
            }

            var result = image.ToBase64(isCompressed);
            
            image.Dispose();
            
            return result;
        }

        private Image GenerateItem(BestDto.ScoreDto score, int rank)
        {
            var difficulty = string.Empty;
            var fontColor = new Color();

            var fcIndicatorText = string.Empty;
            var fsIndicatorText = string.Empty;

            if (score.Fc.Length > 2)
                fcIndicatorText = score.Fc.Substring(0, score.Fc.Length - 1).ToUpper() + "+";
            else
                fcIndicatorText = score.Fc.ToUpper();
            if (score.Fs.Length > 2)
                fsIndicatorText = score.Fs.Replace("p", "+").ToUpper();
            else
                fsIndicatorText = score.Fs.ToUpper();

            switch (score.LevelIndex)
            {
                case 0:
                    difficulty = "bas";
                    fontColor = new Color(16128 / 65535f, 24832 / 65535f, 6144 / 65535f, 1);
                    break;
                case 1:
                    difficulty = "avd";
                    fontColor = new Color(23296 / 65535f, 19968 / 65535f, 15872 / 65535f, 1);
                    break;
                case 2:
                    difficulty = "exp";
                    fontColor = new Color(65535 / 65535f, 46848 / 65535f, 46848 / 65535f, 1);
                    break;
                case 3:
                    difficulty = "mas";
                    fontColor = new Color(48128 / 65535f, 36864 / 65535f, 65535 / 65535f, 1);
                    break;
                case 4:
                    difficulty = "re";
                    fontColor = new Color(34560 / 65535f, 29184 / 65535f, 46848 / 65535f, 1);
                    break;
            }

            var foreground = new Image(Environment.CurrentDirectory + @"/resource/best50/b50_item_foreground_" +
                                       difficulty + ".png");
            Image background;
            if (File.Exists(Environment.CurrentDirectory + @"/resource/covers/" + score.Id +
                            ".png"))
                background = new Image(Environment.CurrentDirectory + @"/resource/covers/" +
                                       score.Id + ".png");
            else
                background = new Image(Environment.CurrentDirectory + @"/resource/covers/1000.png");

            background.Scale(100, 100);
            var image = new Image(350, 100);
            image.DrawImage(foreground, 0, 0);
            image.DrawImage(background, 0, 0);

            foreground.Dispose();
            background.Dispose();

            var info = new Image(350, 100);

            info.DrawText(score.Type, fontColor, 13, FontWeight.Heavy, 114, 23);
            info.DrawText(score.Title, fontColor, 16, FontWeight.Light, 135, 25);
            info.DrawText(score.Achievements.ToString("0.0000") + "%", fontColor, 20, FontWeight.Heavy, 114, 48);
            info.DrawText(score.DifficultyFactor.ToString("0.0") + "·" + score.Rating, fontColor, 20, FontWeight.Heavy,
                114, 68);
            info.DrawText("#" + rank + "·ID " + score.Id, fontColor, 15, FontWeight.Light, 114, 88);

            var imagePath = "";
            var rateBackgroundImagePath = "";
            var rateShadowImagePath = "";
            if (score.rate == InfoCommand.Rate.Sss)
            {
                imagePath = Environment.CurrentDirectory + @"/resource/ratings_hd/sss.png";
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/sss.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_sss.png");
            }
            else if (score.rate == InfoCommand.Rate.Sssp)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/sss_plus.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/sss_plus.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_sss.png");
            }
            else if (score.rate == InfoCommand.Rate.Ss)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/ss.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/ss.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_ss.png");
            }
            else if (score.rate == InfoCommand.Rate.Ssp)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/ss_plus.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/ss_plus.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_ss.png");
            }
            else if (score.rate == InfoCommand.Rate.Sp)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/s_plus.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/s_plus.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_s.png");
            }
            else if (score.rate == InfoCommand.Rate.S)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/s.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/s.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_s.png");
            }
            else if (score.rate == InfoCommand.Rate.Aaa)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/aaa.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/aaa.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_a.png");
            }
            else if (score.rate == InfoCommand.Rate.Aa)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/aa.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/aa.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_a.png");
            }
            else if (score.rate == InfoCommand.Rate.A)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/a.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/a.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_a.png");
            }
            else if (score.rate == InfoCommand.Rate.Bbb)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/bbb.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/bbb.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_a.png");
            }
            else if (score.rate == InfoCommand.Rate.Bb)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/bb.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/bb.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_a.png");
            }
            else if (score.rate == InfoCommand.Rate.B)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/b.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/b.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_a.png");
            }
            else if (score.rate == InfoCommand.Rate.C)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/c.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/c.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_a.png");
            }
            else if (score.rate == InfoCommand.Rate.D)
            {
                imagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/d.png");
                rateShadowImagePath = (Environment.CurrentDirectory + @"/resource/ratings_hd/shadows/d.png");
                rateBackgroundImagePath =
                    (Environment.CurrentDirectory + @"/resource/best50/item_rate_background_a.png");
            }

            var stars = 0;
            var starRate = (float)score.DxScore / score.MaxDxScore * 100;
            if (starRate <= 100 && starRate >= 97)
                stars = 5;
            if (starRate < 97 && starRate >= 95)
                stars = 4;
            if (starRate < 95 && starRate >= 93)
                stars = 3;
            if (starRate < 93 && starRate >= 90)
                stars = 2;
            if (starRate < 90 && starRate >= 85)
                stars = 1;
            if (starRate < 85)
                stars = 0;

            using var gradient = new Image(Environment.CurrentDirectory + @"/resource/best50/gradient.png");
            info.FuseAlpha(gradient);
            image.DrawImage(info, 0, 0);

            using var rateBackgroundImage = new Image(rateBackgroundImagePath);
            using var rateShadowImage = new Image(rateShadowImagePath);
            using var rateImage = new Image(imagePath);

            var width = rateBackgroundImage.Width;
            rateBackgroundImage.DrawText("DX 分数", new Color(0, 0, 0, 0.21f), 12, FontWeight.Heavy,
                HorizontalAlignment.Right, width - 10, 23);
            rateBackgroundImage.DrawText(score.DxScore + "/" + score.MaxDxScore, new Color(0, 0, 0, 0.6f), 12,
                FontWeight.Regular, HorizontalAlignment.Right, width - 10, 36);
            var indicatorText = fcIndicatorText + " " + fsIndicatorText;
            if (indicatorText.Length != 0)
                indicatorText.TrimEnd();
            rateBackgroundImage.DrawText(indicatorText, new Color(0, 0, 0, 0.6f), 10, FontWeight.Heavy,
                HorizontalAlignment.Right, width - 9, 58);
            rateImage.Scale(80, 80);
            rateShadowImage.Scale(80, 80);
            rateBackgroundImage.DrawImage(rateImage, (int)(rateBackgroundImage.Width - rateImage.Width + 5),
                (int)(96 - rateImage.Height), CompositeOperator.DstOut);
            rateBackgroundImage.DrawImage(rateShadowImage, (int)(rateBackgroundImage.Width - rateImage.Width + 5),
                (int)(96 - rateImage.Height));

            using var starImage = new Image(Environment.CurrentDirectory + @"/resource/best50/star.png");
            
            for (int i = 0; i < stars; i++)
            {
                rateBackgroundImage.DrawImage(starImage,
                    (rateBackgroundImage.Width - (int)(i * 11f) - 20),
                    37
                );
            }
            
            image.DrawImage(rateBackgroundImage, info.Width - rateBackgroundImage.Width,
                info.Height - rateBackgroundImage.Height
            );
            info.Dispose();

            return image;
        }

        private Image GenerateBackground(BestDto best)
        {
            var image = new Image(1750, 1440, Color.White);
            var background = new Image(1750, 1440);
            if (best.Charts.SdCharts.Length != 0)
            {
                background.Dispose();
                background = new Image(Environment.CurrentDirectory + @"/resource/covers/" +
                                       best.Charts.SdCharts[0].Id.ToString() + ".png");
            }

            background.Scale(75, 75);
            background.GaussianBlur(10);
            background.Resize(1750, 1750);
            image.DrawImage(background, 0, 0);
            var foreground = new Image(Environment.CurrentDirectory + @"/resource/best50/best50_background.png");
            image.DrawImage(foreground, 0, 0);
            
            background.Dispose();
            foreground.Dispose();
            
            return image;
        }
    }
}

