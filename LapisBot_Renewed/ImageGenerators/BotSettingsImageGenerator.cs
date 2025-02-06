using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ImageMagick;
using ImageMagick.Drawing;

namespace LapisBot_Renewed.ImageGenerators
{
    public class BotSettingsImageGenerator
    {
        public string Generate(BotSettingsCommand.Settings settings, bool isCompressed)
        {
            var image = new MagickImage(Environment.CurrentDirectory + @"/resource/settings/background.png");

            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font-light.otf")
                .FontPointSize(56.26f)
                .FillColor(new MagickColor(65535, 65535, 65535, 65535))
                .Text(21.4, 111.69, settings.SettingsName)
                .Draw(image);

            int top = 170;
            int i = 0;
            foreach (KeyValuePair<string, string> valuePair in settings.DisplayNames)
            {
                if (settings.GetType().GetProperty(valuePair.Key).GetValue(settings) is bool)
                {
                    var itemValue = (bool)settings.GetType().GetProperty(valuePair.Key).GetValue(settings);
                    var _top = top + i * 69;
                    MagickImage itemImage;
                    if (itemValue)
                        itemImage = new MagickImage(Environment.CurrentDirectory +
                                                    @"/resource/settings/item_enabled.png");
                    else
                        itemImage = new MagickImage(Environment.CurrentDirectory +
                                                    @"/resource/settings/item_disabled.png");
                    new Drawables()
                        .Font(Environment.CurrentDirectory + @"/resource/font-light.otf")
                        .FontPointSize(34f)
                        .FillColor(new MagickColor(65535, 65535, 65535, 65535))
                        .Text(23, 48, (i + 1).ToString())
                        .Draw(itemImage);
                    new Drawables()
                        .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                        .FontPointSize(28f)
                        .FillColor(new MagickColor(65535, 65535, 65535, 65535))
                        .Text(80, 46, valuePair.Value)
                        .Draw(itemImage);
                    image.Composite(itemImage, 0, _top, CompositeOperator.Atop);
                    itemImage.Dispose();
                    i++;
                }
                else if (settings.GetType().GetProperty(valuePair.Key).GetValue(settings) is string)
                {
                    var itemValue = (string)settings.GetType().GetProperty(valuePair.Key).GetValue(settings);
                    var _top = top + i * 69;
                    MagickImage itemImage;
                    itemImage = new MagickImage(Environment.CurrentDirectory +
                                                @"/resource/settings/item_string.png");
                    new Drawables()
                        .Font(Environment.CurrentDirectory + @"/resource/font-light.otf")
                        .FontPointSize(34f)
                        .FillColor(new MagickColor(65535, 65535, 65535, 65535))
                        .Text(23, 48, (i + 1).ToString())
                        .Draw(itemImage);
                    new Drawables()
                        .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                        .FontPointSize(28f)
                        .FillColor(new MagickColor(65535, 65535, 65535, 65535))
                        .Text(80, 46, valuePair.Value)
                        .Draw(itemImage);
                    if (itemValue != "")
                        new Drawables()
                            .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                            .FontPointSize(28f)
                            .FillColor(new MagickColor(65535, 65535, 65535, 65535))
                            .TextAlignment(TextAlignment.Right)
                            .Text(500, 46, itemValue)
                            .Draw(itemImage);
                    image.Composite(itemImage, 0, _top, CompositeOperator.Atop);
                    itemImage.Dispose();
                    i++;
                }

                /*if (isCompressed)
                {
                    image.SetCompression(CompressionMethod.JPEG);
                    image.Format = MagickFormat.Jpeg;
                    image.Quality = 90;
                }*/
            }
            var result = image.ToBase64();
            image.Dispose();
            return result;
        }
    }
}
