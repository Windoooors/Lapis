using System;
using System.IO;
using Lapis.Commands.GroupCommands;
using Lapis.Operations.ImageOperation;
using Lapis.Settings;

namespace Lapis.ImageGenerators;

public class BotSettingsImageGenerator
{
    public string Generate(long groupId, bool isCompressed)
    {
        using var image = new Image(512, 768);

        var top = 0;

        var tempTop = top;
        foreach (var category in SettingsItems.Categories)
        {
            tempTop += 100;
            foreach (var commandItem in category.Items)
            {
                tempTop += 69;
                foreach (var item in commandItem.Items) tempTop += 69;
            }
        }

        if (tempTop > 768)
            image.Resize(512, tempTop + 100);

        foreach (var category in SettingsItems.Categories)
        {
            top += 100;
            image.DrawText(category.DisplayName, Color.White, 48, FontWeight.Light, 14.8f, top - 20);

            foreach (var commandItem in category.Items)
            {
                using var commandNameBackground = new Image(Path.Combine(Environment.CurrentDirectory,
                    "resource/settings/command_background.png"));

                commandNameBackground.DrawText(commandItem.Identifier, Color.White, 28f, FontWeight.Regular,
                    HorizontalAlignment.Center, 93, 46);
                commandNameBackground.DrawText(commandItem.DisplayName, Color.White, 28f, FontWeight.Light,
                    HorizontalAlignment.Left, 196, 46);

                image.DrawImage(commandNameBackground, 0, top);

                top += 69;

                foreach (var item in commandItem.Items)
                {
                    var itemValue =
                        SettingsCommand.Instance.GetValue(
                            new SettingsIdentifierPair(commandItem.Identifier, item.Identifier), groupId);

                    var itemImagePath = Path.Combine(Environment.CurrentDirectory,
                        itemValue ? "resource/settings/item_enabled.png" : "resource/settings/item_disabled.png");
                    using var itemImage = new Image(itemImagePath);

                    itemImage.DrawText(item.Identifier, Color.White, 34f, FontWeight.Light, 23, 46);
                    itemImage.DrawText(item.DisplayName, Color.White, 28f, FontWeight.Regular, 80, 46);

                    image.DrawImage(itemImage, 0, top);

                    top += 69;
                }
            }
        }

        using var background = new Image(Path.Combine(Environment.CurrentDirectory,
            "resource/settings/background.png"));

        background.Resize(image.Width, image.Height);

        background.DrawImage(image, 0, 0);

        var result = background.ToBase64(isCompressed);

        return result;
    }
}