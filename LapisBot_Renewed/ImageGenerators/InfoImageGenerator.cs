using System;
using System.Drawing;
using System.IO;
using ImageMagick;
namespace LapisBot_Renewed
{
    public class InfoImageGenerator
    {
        public InfoImageGenerator()
        {
        }

        //public static System.Drawing.Text.PrivateFontCollection PrivateFonts;

        private static string GetPictureID(int id)
        {
            return id.ToString("00000");
        }

        private static string coverImagePath;

        public static MagickImage GenerateBackground(int index, SongDto[] songs, string title, ApiOperator apiOperator)
        {
            var image = new MagickImage(Environment.CurrentDirectory + @"/resources/random/background.png");
            try
            {
                if (File.Exists(Environment.CurrentDirectory + @"/resources/covers_hd/" + songs[index].Id + ".png"))
                    coverImagePath = Environment.CurrentDirectory + @"/resources/covers_hd/" + songs[index].Id + ".png";
                else
                    coverImagePath = apiOperator.ImageToPng("https://www.diving-fish.com/covers/" + GetPictureID(songs[index].Id) + ".png", Environment.CurrentDirectory + @"/temp", "cover_temp.png");
            }
            catch
            {
                coverImagePath = Environment.CurrentDirectory + @"/resources/covers/01000.png";
            }
            var backgroundCoverImage = new MagickImage(coverImagePath);
            backgroundCoverImage.Resize(64, 64);
            backgroundCoverImage.GaussianBlur(20);
            backgroundCoverImage.Resize(1400, 1400);

            backgroundCoverImage.Composite(image, 0, 0, CompositeOperator.Atop);

            image = backgroundCoverImage;

            image.Crop(1400, 1280);

            MagickReadSettings backgroundLayerSettings = new MagickReadSettings
            {
                Width = 1280,
                Height = 1280
            };
            var backgroundLayer = new MagickImage("xc:transparent", backgroundLayerSettings);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-heavy.otf")
                .FontPointSize(400)
                .FillColor(new MagickColor(65535, 65535, 65535, 5300))
                .Text(0, 310, songs[index].Title)
                .Draw(backgroundLayer);
            backgroundLayer.Rotate(-90);
            image.Composite(backgroundLayer, 0, -150, CompositeOperator.Atop);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font.otf")
                .FontPointSize(48)
                .FillColor(new MagickColor(65535, 65535, 65535, 5300))
                .Text(13, 1200, title)
                .Draw(image);
            return image;
        }

        public static MagickImage GenerateDifficultyLayer(int index, SongDto[] songs, InfoCommand.GetScore.Level[] levels)
        {
            var difficultyLayerImage = new MagickImage("xc:transparent", new MagickReadSettings() { Width = 6600, Height = 1080 });
            if (levels != null)
            {
                MagickImage _image = null;
                foreach (InfoCommand.GetScore.Level level in levels)
                {
                    var y = 0;
                    switch (level.levelIndex)
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
                        .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                        .FontPointSize(24)
                        .FillColor(new MagickColor(65535, 65535, 65535))
                        .Text(0, y + 92, Math.Round(level.achievement, 2, MidpointRounding.ToNegativeInfinity).ToString("0.00") + "% ")
                        .Draw(difficultyLayerImage);
                    if (Math.Round(level.achievement, 2, MidpointRounding.ToNegativeInfinity).ToString("0.00").Length == 6)
                        x = 100;
                    if (Math.Round(level.achievement, 2, MidpointRounding.ToNegativeInfinity).ToString("0.00").Length == 5)
                        x = 87;
                    if (Math.Round(level.achievement, 2, MidpointRounding.ToNegativeInfinity).ToString("0.00").Length == 4)
                        x = 74;

                    if (level.rate == InfoCommand.Rate.SSS)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/sss.png");
                    else if (level.rate == InfoCommand.Rate.SSSp)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/sss_plus.png");
                    else if (level.rate == InfoCommand.Rate.SS)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/ss.png");
                    else if (level.rate == InfoCommand.Rate.SSp)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/ss_plus.png");
                    else if (level.rate == InfoCommand.Rate.Sp)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/s_plus.png");
                    else if (level.rate == InfoCommand.Rate.S)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/s.png");
                    else if (level.rate == InfoCommand.Rate.AAA)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/aaa.png");
                    else if (level.rate == InfoCommand.Rate.AA)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/aa.png");
                    else if (level.rate == InfoCommand.Rate.A)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/a.png");
                    else if (level.rate == InfoCommand.Rate.BBB)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/bbb.png");
                    else if (level.rate == InfoCommand.Rate.BB)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/bb.png");
                    else if (level.rate == InfoCommand.Rate.B)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/b.png");
                    else if (level.rate == InfoCommand.Rate.C)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/c.png");
                    else if (level.rate == InfoCommand.Rate.D)
                        _image = new MagickImage(Environment.CurrentDirectory + @"/resources/ratings/d.png");
                    if (_image != null)
                    {
                        if (_image.BaseHeight == 22 || _image.BaseHeight == 23)
                            difficultyLayerImage.Composite(_image, x, y + 70, CompositeOperator.Blend);
                        if (_image.BaseHeight == 19 || _image.BaseHeight == 20 || _image.BaseHeight == 18)
                            difficultyLayerImage.Composite(_image, x, y + 73, CompositeOperator.Blend);
                    }
                }
            }
            if (songs[index].Ratings.Length == 5)
            {
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(40)
                    .FillColor(new MagickColor(65535, 65535, 65535))
                    .Text(0, 170, songs[index].Ratings[0].ToString("0.0"))
                    .Text(0, 307, songs[index].Ratings[1].ToString("0.0"))
                    .Text(0, 444, songs[index].Ratings[2].ToString("0.0"))
                    .Text(0, 581, songs[index].Ratings[3].ToString("0.0"))
                    .Text(0, 718, songs[index].Ratings[4].ToString("0.0"))
                    .Draw(difficultyLayerImage);
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(24)
                    .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                    .Text(0, 124, songs[index].Charts[0].Charter == "-" ? "未知作谱者" : "by " + songs[index].Charts[0].Charter)
                    .Text(0, 262, songs[index].Charts[1].Charter == "-" ? "未知作谱者" : "by " + songs[index].Charts[1].Charter)
                    .Text(0, 399, songs[index].Charts[2].Charter == "-" ? "未知作谱者" : "by " + songs[index].Charts[2].Charter)
                    .Text(0, 536, songs[index].Charts[3].Charter == "-" ? "未知作谱者" : "by " + songs[index].Charts[3].Charter)
                    .Text(0, 673, songs[index].Charts[4].Charter == "-" ? "未知作谱者" : "by " + songs[index].Charts[4].Charter)
                    .Draw(difficultyLayerImage);
            }
            else
            {
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(40)
                    .FillColor(new MagickColor(65535, 65535, 65535))
                    .Text(0, 170, songs[index].Ratings[0].ToString("0.0"))
                    .Text(0, 307, songs[index].Ratings[1].ToString("0.0"))
                    .Text(0, 444, songs[index].Ratings[2].ToString("0.0"))
                    .Text(0, 581, songs[index].Ratings[3].ToString("0.0"))
                    .Text(0, 718, "NaN")
                    .Draw(difficultyLayerImage);
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(24)
                    .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                    .Text(0, 124, songs[index].Charts[0].Charter == "-" ? "未知作谱者" : "by " + songs[index].Charts[0].Charter)
                    .Text(0, 262, songs[index].Charts[1].Charter == "-" ? "未知作谱者" : "by " + songs[index].Charts[1].Charter)
                    .Text(0, 399, songs[index].Charts[2].Charter == "-" ? "未知作谱者" : "by " + songs[index].Charts[2].Charter)
                    .Text(0, 536, songs[index].Charts[3].Charter == "-" ? "未知作谱者" : "by " + songs[index].Charts[3].Charter)
                    .Draw(difficultyLayerImage);
            }
            return difficultyLayerImage;
        }

        public static string Generate(int index, SongDto[] songs, string title, InfoCommand.GetScore.Level[] levels)
        {
            var image = GenerateBackground(index, songs, title, Program.apiOperator);

            image.Composite(GenerateDifficultyLayer(index, songs, levels), 90, 305, CompositeOperator.Atop);

            var coverImageShadow = new MagickImage(Environment.CurrentDirectory + @"/resources/random/coverimage.png");
            image.Composite(coverImageShadow, 0, 0, CompositeOperator.Atop);

            var coverImage = new MagickImage(coverImagePath);
            coverImage.Resize(1077, 1077);
            image.Composite(coverImage, 324, 207, CompositeOperator.Atop);

            var foreImage = new MagickImage(Environment.CurrentDirectory + @"/resources/random/foreground.png");
            image.Composite(foreImage, 0, 0, CompositeOperator.Atop);

            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font.otf")
                .FontPointSize(55)
                .FillColor(new MagickColor(65535, 65535, 65535))
                .Text(10, 190, songs[index].Title)
                .Draw(image);

            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font.otf")
                .FontPointSize(42)
                .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                .Text(10, 127, songs[index].BasicInfo.Artist)
                .Draw(image);

            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-heavy.otf")
                .FontPointSize(90)
                .FillColor(new MagickColor(65535, 65535, 65535, 5300))
                .TextAlignment(TextAlignment.Right)
                .Text(1393, 87, "ID " + songs[index].Id.ToString())
                .Draw(image);

            var songTypeLayer = new MagickImage("xc:transparent", new MagickReadSettings() { Width = 128, Height = 128 });

            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font.otf")
                .FontPointSize(36)
                .FillColor(new MagickColor(65535, 65535, 65535, 22768))
                .Text(0, 40, songs[index].Type)
                .Draw(songTypeLayer);

            songTypeLayer.Rotate(-90);
            image.Composite(songTypeLayer, 30, 214, CompositeOperator.Atop);
            //image.Resize(1047, 952);
            image.SetCompression(CompressionMethod.JPEG);
            image.Format = MagickFormat.Jpeg;
            image.Quality = 90;
            return image.ToBase64();
        }
    }
}
