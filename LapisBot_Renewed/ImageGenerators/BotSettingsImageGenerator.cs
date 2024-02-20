using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using ImageMagick;
namespace LapisBot_Renewed.ImageGenerators
{
    public class BotSettingsImageGenerator
    {
        public string Generate(BotSettingsCommand.Settings settings, bool isCompressed)
        {
            var image = new MagickImage(Environment.CurrentDirectory + @"/resources/settings/background.png");

            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                .FontPointSize(56.26f)
                .FillColor(new MagickColor(65535, 65535, 65535, 65535))
                .Text(21.4, 111.69, settings.SettingsName)
                .Draw(image);

            int top = 170;
            int i = 0;
            foreach (KeyValuePair<string, string> valuePair in settings.DisplayNames)
            {
                var itemValue = (bool)settings.GetType().GetProperty(valuePair.Key).GetValue(settings);
                var _top = top + i * 69;
                MagickImage itemImage;
                if (itemValue)
                    itemImage = new MagickImage(Environment.CurrentDirectory + @"/resources/settings/item_enabled.png");
                else
                    itemImage = new MagickImage(Environment.CurrentDirectory + @"/resources/settings/item_disabled.png");
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font-light.otf")
                    .FontPointSize(34f)
                    .FillColor(new MagickColor(65535, 65535, 65535, 65535))
                    .Text(23, 48, (i + 1).ToString())
                    .Draw(itemImage);
                new Drawables()
                    .Font(Environment.CurrentDirectory + @"/resources/font.otf")
                    .FontPointSize(28f)
                    .FillColor(new MagickColor(65535, 65535, 65535, 65535))
                    .Text(80, 46, valuePair.Value)
                    .Draw(itemImage);
                image.Composite(itemImage, 0, _top, CompositeOperator.Atop);
                i++;
            }

            /*if (isCompressed)
            {
                image.SetCompression(CompressionMethod.JPEG);
                image.Format = MagickFormat.Jpeg;
                image.Quality = 90;
            }*/

            return image.ToBase64();
        }
    }
}
