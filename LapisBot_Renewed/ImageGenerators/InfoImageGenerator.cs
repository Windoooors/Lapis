using System;
using System.IO;
using ImageMagick;
using LapisBot_Renewed.GroupCommands.MaiCommands;
using LapisBot_Renewed.GroupCommands;

namespace LapisBot_Renewed.ImageGenerators
{
    public class InfoImageGenerator
    {
        private string GetPictureId(int id)
        {
            return id.ToString("00000");
        }

        private string _coverImagePath;

        private MagickImage GenerateBackground(SongDto song, string title, ApiOperator apiOperator)
        {
            var image = new MagickImage(Environment.CurrentDirectory + @"/resource/random/background.png");

            if (File.Exists(Environment.CurrentDirectory + @"/resource/covers_hd/" + song.Id + ".png"))
                _coverImagePath = Environment.CurrentDirectory + @"/resource/covers_hd/" + song.Id + ".png";
            else
                _coverImagePath = Environment.CurrentDirectory + @"/resource/covers/1000.png";

            var backgroundCoverImage = new MagickImage(_coverImagePath);
            backgroundCoverImage.Resize(64, 64);
            backgroundCoverImage.GaussianBlur(20);
            backgroundCoverImage.Resize(1400, 1400);

            backgroundCoverImage.Composite(image, 0, 0, CompositeOperator.Atop);
            
            image.Dispose();

            image = backgroundCoverImage;

            image.Crop(1400, 1280);

            MagickReadSettings backgroundLayerSettings = new MagickReadSettings
            {
                Width = 1280,
                Height = 1280
            };
            var backgroundLayer = new MagickImage("xc:transparent", backgroundLayerSettings);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font-heavy.otf")
                .FontPointSize(400)
                .FillColor(new MagickColor(65535, 65535, 65535, 5300))
                .Text(0, 310, song.Title)
                .Draw(backgroundLayer);
            backgroundLayer.Rotate(-90);
            
            image.Composite(backgroundLayer, 0, -150, CompositeOperator.Atop);
            
            backgroundLayer.Dispose();
            
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .FontPointSize(48)
                .FillColor(new MagickColor(65535, 65535, 65535, 5300))
                .Text(13, 1200, title)
                .Draw(image);
            return image;
        }

        private MagickImage GenerateDifficultyLayer(SongDto song, InfoCommand.GetScoreDto.Level[] levels)
        {
            var difficultyLayerImage = new MagickImage("xc:transparent", new MagickReadSettings() { Width = 6600, Height = 1080 });
            if (levels != null)
            {
                MagickImage image = null;
                foreach (InfoCommand.GetScoreDto.Level level in levels)
                {
                    var y = 0;
                    switch (level.LevelIndex)
                    {
                        case 0:
                            y = 0;
                            break;
                        case 1:
                            y = 137;
                            break;
                        case 2:
                            y = 274;
                            break;
                        case 3:
                            y = 411;
                            break;
                        case 4:
                            y = 548;
                            break;
                    }

                    var x = 0;
                    new Drawables()
                        .Font(Environment.CurrentDirectory + @"/resource/font-light.otf")
                        .FontPointSize(24)
                        .FillColor(new MagickColor(65535, 65535, 65535))
                        .Text(0, y + 88,
                            level.Achievement.ToString("0.0000") +
                            "% ")
                        .Draw(difficultyLayerImage);
                    if (level.Achievement.ToString("0.0000").Length ==
                        8)
                        x = 130;
                    if (level.Achievement.ToString("0.0000").Length ==
                        7)
                        x = 117;
                    if (level.Achievement.ToString("0.0000").Length ==
                        6)
                        x = 104;

                    if (level.Rate == InfoCommand.Rate.Sss)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/sss.png");
                    else if (level.Rate == InfoCommand.Rate.Sssp)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/sss_plus.png");
                    else if (level.Rate == InfoCommand.Rate.Ss)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/ss.png");
                    else if (level.Rate == InfoCommand.Rate.Ssp)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/ss_plus.png");
                    else if (level.Rate == InfoCommand.Rate.Sp)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/s_plus.png");
                    else if (level.Rate == InfoCommand.Rate.S)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/s.png");
                    else if (level.Rate == InfoCommand.Rate.Aaa)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/aaa.png");
                    else if (level.Rate == InfoCommand.Rate.Aa)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/aa.png");
                    else if (level.Rate == InfoCommand.Rate.A)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/a.png");
                    else if (level.Rate == InfoCommand.Rate.Bbb)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/bbb.png");
                    else if (level.Rate == InfoCommand.Rate.Bb)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/bb.png");
                    else if (level.Rate == InfoCommand.Rate.B)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/b.png");
                    else if (level.Rate == InfoCommand.Rate.C)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/c.png");
                    else if (level.Rate == InfoCommand.Rate.D)
                        image = new MagickImage(Environment.CurrentDirectory + @"/resource/ratings/d.png");
                    if (image != null)
                    {
                        if (image.BaseHeight == 22 || image.BaseHeight == 23)
                            difficultyLayerImage.Composite(image, x, y + 66, CompositeOperator.Blend);
                        if (image.BaseHeight == 19 || image.BaseHeight == 20 || image.BaseHeight == 18)
                            difficultyLayerImage.Composite(image, x, y + 69, CompositeOperator.Blend);
                        image.Dispose();
                    }

                    var fcIndicatorText = string.Empty;
                    var fsIndicatorText = string.Empty;

                    if (level.Fc.Length > 2)
                        fcIndicatorText = level.Fc.Substring(0, level.Fc.Length - 1).ToUpper() + "+";
                    else
                        fcIndicatorText = level.Fc.ToUpper();
                    if (level.Fs.Length > 2)
                        fsIndicatorText = level.Fs.Replace("p", "+").ToUpper();
                    else
                        fsIndicatorText = level.Fs.ToUpper();

                    var indicatorText = string.Empty;
                    if (fcIndicatorText != string.Empty)
                        indicatorText = fcIndicatorText + " " + fsIndicatorText;
                    else
                        indicatorText = fsIndicatorText;
                    if (indicatorText.Length != 0)
                        indicatorText.TrimEnd();

                    if (indicatorText == string.Empty)
                        continue;
                    new Drawables()
                        .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                        .FontPointSize(18)
                        .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                        .Text(0, y + 63, indicatorText)
                        .Draw(difficultyLayerImage);
                }
            }

            int[] difficultyFactorYPositions = { 170, 307, 444, 581, 718 };
            int[] charterYPositions = { 124, 262, 399, 536, 673 };

            for (int i = 0; i < song.Ratings.Length; i++)
            {
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resource/font-light.otf")
                    .FontPointSize(24)
                    .FillColor(new MagickColor(65535, 65535, 65535))
                    .Text(0, difficultyFactorYPositions[i] - 24, song.Ratings[i].ToString("0.0"))
                    .Draw(difficultyLayerImage);
                
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resource/font-light.otf")
                    .FontPointSize(18)
                    .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                    .Text(0, difficultyFactorYPositions[i], "fit " + song.FitRatings[i].ToString("0.00"))
                    .Draw(difficultyLayerImage);
                
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resource/font-light.otf")
                    .FontPointSize(24)
                    .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                    .Text(0, charterYPositions[i] - 10, song.Charts[i].Charter == "-" ? "未知作谱者" : "by " + song.Charts[i].Charter)
                    .Draw(difficultyLayerImage);
            }
            
            if (song.Ratings.Length == 4 && song.Id.ToString().Length != 6)
            {
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resource/font-light.otf")
                    .FontPointSize(40)
                    .FillColor(new MagickColor(65535, 65535, 65535))
                    .Text(0, 718, "NaN")
                    .Draw(difficultyLayerImage);
            }
            return difficultyLayerImage;
        }

        public string Generate(SongDto song, string title, InfoCommand.GetScoreDto.Level[] levels, bool isCompressed)
        {
            var image = GenerateBackground(song, title, Program.apiOperator);

            var difficultyLayer = GenerateDifficultyLayer(song, levels);
            
            image.Composite(difficultyLayer, 90, 305, CompositeOperator.Atop);
            
            difficultyLayer.Dispose();
            
            var coverImageShadow = new MagickImage(Environment.CurrentDirectory + @"/resource/random/coverimage.png");
            image.Composite(coverImageShadow, 0, 0, CompositeOperator.Atop);
            
            coverImageShadow.Dispose();

            var coverImage = new MagickImage(_coverImagePath);
            coverImage.Resize(1077, 1077);
            image.Composite(coverImage, 324, 207, CompositeOperator.Atop);
            
            coverImage.Dispose();
            
            MagickImage foreImage;
            if (song.Id.ToString().Length == 6)
                foreImage = new MagickImage(Environment.CurrentDirectory + @"/resource/random/foreground_utage.png");
            else
                foreImage = new MagickImage(Environment.CurrentDirectory + @"/resource/random/foreground.png");
            image.Composite(foreImage, 0, 0, CompositeOperator.Atop);
            
            foreImage.Dispose();

            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .FontPointSize(55)
                .FillColor(new MagickColor(65535, 65535, 65535))
                .Text(10, 190, song.Title)
                .Draw(image);

            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .FontPointSize(42)
                .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                .Text(10, 127, song.BasicInfo.Artist)
                .Draw(image);

            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font-heavy.otf")
                .FontPointSize(90)
                .FillColor(new MagickColor(65535, 65535, 65535, 5300))
                .TextAlignment(TextAlignment.Right)
                .Text(1393, 87, "ID " + song.Id.ToString())
                .Draw(image);

            var songTypeLayer = new MagickImage("xc:transparent", new MagickReadSettings() { Width = 128, Height = 128 });

            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .FontPointSize(36)
                .FillColor(new MagickColor(65535, 65535, 65535, 22768))
                .Text(0, 40, song.Type)
                .Draw(songTypeLayer);

            songTypeLayer.Rotate(-90);
            image.Composite(songTypeLayer, 30, 214, CompositeOperator.Atop);
            
            songTypeLayer.Dispose();
            
            //image.Resize(1047, 952);
            if (isCompressed)
            {
                image.SetCompression(CompressionMethod.JPEG);
                image.Format = MagickFormat.Jpeg;
                image.Quality = 90;
            }

            var result = image.ToBase64();
            image.Dispose();
            return result;
        }
    }
}
