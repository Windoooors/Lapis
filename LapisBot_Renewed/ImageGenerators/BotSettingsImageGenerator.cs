using System;
using System.Collections.Generic;
using System.IO;
using LapisBot_Renewed.Operations.ImageOperation;

namespace LapisBot_Renewed.ImageGenerators
{
    public class BotSettingsImageGenerator
    {
        public string Generate(BotSettingsCommand.Settings settings, bool isCompressed)
        {
            var image = new Image(Path.Combine(Environment.CurrentDirectory, "resource/settings/background.png"));
            
            image.DrawText(settings.SettingsName, Color.White, 56.26f, FontWeight.Light, 21.4f, 111.69f);

            int top = 170;
            int i = 0;
            foreach (KeyValuePair<string, string> valuePair in settings.DisplayNames)
            {
                var propertyValue = settings.GetType().GetProperty(valuePair.Key)?.GetValue(settings);
                if (propertyValue is bool itemValue)
                {
                    var itemImagePath = Path.Combine(Environment.CurrentDirectory,
                                                     itemValue ? "resource/settings/item_enabled.png" : "resource/settings/item_disabled.png");
                    using var itemImage = new Image(itemImagePath);

                    itemImage.DrawText((i + 1).ToString(), Color.White, 34f, FontWeight.Light, 23, 48);
                    itemImage.DrawText(valuePair.Value, Color.White, 28f, FontWeight.Regular, 80, 46);

                    image.DrawImage(itemImage, 0, top + i * 69);

                    i++;
                }
                else if (propertyValue is string stringValue)
                {
                    using var itemImage = new Image(Path.Combine(Environment.CurrentDirectory, "resource/settings/item_string.png"));

                    itemImage.DrawText((i + 1).ToString(), Color.White, 34f, FontWeight.Light, 23, 48);
                    itemImage.DrawText(valuePair.Value, Color.White, 28f, FontWeight.Regular, 80, 46);
                    itemImage.DrawText(stringValue, Color.White, 28f, FontWeight.Regular,HorizontalAlignment.Right , 500, 46);

                    image.DrawImage(itemImage, 0, top + i * 69);

                    i++;
                }
            }

            var result = image.ToBase64(isCompressed);
            
            image.Dispose();
            
            return result;
        }
    }
}
