using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Xml.Schema;
using LapisBot_Renewed.GroupCommands;
using LapisBot_Renewed.GroupCommands.MaiCommands;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific.AppCompat;

namespace LapisBot_Renewed.ImageGenerators;
using ImageMagick;

public class PlateImageGenerator
{
    public class Color
    {
        public int R;
        public int G;
        public int B;
    }
    
    public MagickColor GetDominantColor(string path)
    {
        var image = new MagickImage(path);
        image.Resize(5, 5);
        var pixels = image.GetPixels();

        var colors = new List<Color>();
        
        foreach (var pixel in pixels)
        {
            var r = pixel.ToColor().R / 100;
            var g = pixel.ToColor().G / 100;
            var b = pixel.ToColor().B / 100;

            colors.Add(new Color() { R = r, G = g, B = b });
        }
        
        var fitColors = new List<List<Color>>();

        var sortedColors = new List<Color>();
        
        foreach (var color in colors)
        {
            if (sortedColors.Contains(color))
                continue;
            
            var colorList = new List<Color>() { color };
            
            foreach (var secondColor in colors)
            {
                if (color == secondColor || sortedColors.Contains(secondColor))
                    continue;
                if ((Math.Abs(secondColor.R - color.R) < 10) && (Math.Abs(secondColor.G - color.G) < 10) &&
                    (Math.Abs(secondColor.B - color.B) < 10))
                {
                    sortedColors.Add(secondColor);
                    colorList.Add(secondColor);
                }
            }

            sortedColors.Add(color);
            fitColors.Add(colorList);
        }
        
        

        var sortedFitColors = fitColors.OrderByDescending(f => f.Count).ToArray();

        var magickColor = new MagickColor((ushort)(sortedFitColors[0][0].R * 100), (ushort)(sortedFitColors[0][0].G * 100),
            (ushort)(sortedFitColors[0][0].B * 100));
        
        image.Dispose();
        return magickColor;
    }

    public string Generate(List<PlateCommand.SongToBeDisplayed> songsToBeDisplayed, List<PlateCommand.SongToBeDisplayed> allSongs, string username, MaiCommand maiCommand, PlateCommand.PlateCategories category, string userId, bool usingHead, int plateVersionIndex, bool isCompressed)
    {
        var difficulties = new Dictionary<string, List<PlateCommand.SongToBeDisplayed>>();
        foreach (var song in songsToBeDisplayed)
        {
            var rating = Math.Round(song.SongDto.Ratings[song.LevelIndex], 1);
            if (rating > 13.6 & rating < 14.0)
            {
                if (!difficulties.ContainsKey("13+"))
                    difficulties.Add("13+", new List<PlateCommand.SongToBeDisplayed>() { song });
                else
                {
                    var songs = new List<PlateCommand.SongToBeDisplayed>();
                    difficulties.TryGetValue("13+", out songs);
                    songs.Add(song);
                }
            }
            if (rating > 13.9 & rating < 14.7)
            {
                if (!difficulties.ContainsKey("14"))
                    difficulties.Add("14", new List<PlateCommand.SongToBeDisplayed>() { song });
                else
                {
                    var songs = new List<PlateCommand.SongToBeDisplayed>();
                    difficulties.TryGetValue("14", out songs);
                    songs.Add(song);
                }
            }
            if (rating > 14.6 & rating < 15.0)
            {
                if (!difficulties.ContainsKey("14+"))
                    difficulties.Add("14+", new List<PlateCommand.SongToBeDisplayed>() { song });
                else
                {
                    var songs = new List<PlateCommand.SongToBeDisplayed>();
                    difficulties.TryGetValue("14+", out songs);
                    songs.Add(song);
                }
            }
            if (rating > 14.9)
            {
                if (!difficulties.ContainsKey("15"))
                    difficulties.Add("15", new List<PlateCommand.SongToBeDisplayed>() { song });
                else
                {
                    var songs = new List<PlateCommand.SongToBeDisplayed>();
                    difficulties.TryGetValue("15", out songs);
                    songs.Add(song);
                }
            }
        }

        var totalHeight = 171;
        
        if (difficulties.ContainsKey("15"))
        {
            List<PlateCommand.SongToBeDisplayed> songs;
            difficulties.TryGetValue("15", out songs);
            totalHeight += 100 * (int)Math.Ceiling((float)songs.Count / 7) + 75;
        }
        if (difficulties.ContainsKey("14+"))
        {
            List<PlateCommand.SongToBeDisplayed> songs;
            difficulties.TryGetValue("14+", out songs);
            totalHeight += 100 * (int)Math.Ceiling((float)songs.Count / 7) + 75;
        }
        if (difficulties.ContainsKey("14"))
        {
            List<PlateCommand.SongToBeDisplayed> songs;
            difficulties.TryGetValue("14", out songs);
            totalHeight += 100 * (int)Math.Ceiling((float)songs.Count / 7) + 75;
        }
        if (difficulties.ContainsKey("13+"))
        {
            List<PlateCommand.SongToBeDisplayed> songs;
            difficulties.TryGetValue("13+", out songs);
            totalHeight += 100 * (int)Math.Ceiling((float)songs.Count / 7) + 75;
        }

        var sortedDifficulties =
            difficulties.OrderByDescending(d =>
                maiCommand.LevelDictionary.Keys.ToArray().IndexOf(d.Key)).ToDictionary();

        totalHeight += 104;

        var image = new MagickImage("xc:transparent", new MagickReadSettings() { Width = 700, Height = totalHeight });

        var backgroundImage = new MagickImage(AppContext.BaseDirectory + @"resource/covers/" +
                                              sortedDifficulties.Values.ToArray()[0][0].SongDto.Id + ".png");

        backgroundImage.Resize(75, 75);
        backgroundImage.GaussianBlur(10);
        if (totalHeight > 700)
        {
            backgroundImage.Resize(totalHeight, totalHeight);
            image.Composite(backgroundImage, (700 - totalHeight) / 2, 0, CompositeOperator.Blend);
        }
        else
        {
            backgroundImage.Resize(700, 700);
            image.Composite(backgroundImage, 0, (totalHeight - 700) / 2, CompositeOperator.Blend);
        }



        backgroundImage.Dispose();
        
        var mask = new MagickImage(AppContext.BaseDirectory + "resource/plate/mask.png");
        
        mask.Resize(totalHeight, totalHeight);
        
        image.Composite(mask, 0, 0, CompositeOperator.Atop);
        
        mask.Dispose();
        
        var header = new MagickImage(AppContext.BaseDirectory + @"resource/plate/header.png");
        
        var nameFormImage = new MagickImage(AppContext.BaseDirectory + @"resource/plate/name_form.png");
        
        int i = 0;
        int j = 0;
        
        var itemsInFirstGroup =
            new MagickImage("xc:transparent", new MagickReadSettings() { Width = 700, Height = totalHeight });
        
        foreach (var song in sortedDifficulties.Values.ToArray()[0])
        {
            var item = GenerateItem(song.SongDto, song.LevelIndex, song.ScoreDto, category);
            itemsInFirstGroup.Composite(item, i * 100, 171 + j * 100, CompositeOperator.Blend);
            item.Dispose();
            i++;
            if (i % 7 == 0)
            {
                i = 0;
                j++;
            }
        }

        
        /*
        var shadowInFirstGroup = itemsInFirstGroup.Clone();
        
        shadowInFirstGroup.Resize(new Percentage(5));

        shadowInFirstGroup.Shadow(0, 0, 100, new Percentage(50), new MagickColor(0, 0, 0));
        
        shadowInFirstGroup.Resize(new Percentage(2000));

        shadowInFirstGroup.Composite(itemsInFirstGroup, Gravity.Center, CompositeOperator.Blend);
        
        image.Composite(shadowInFirstGroup, Gravity.Center, CompositeOperator.Atop);
        
        shadowInFirstGroup.Dispose();*/
        
        image.Composite(itemsInFirstGroup, Gravity.Center, CompositeOperator.Atop);
        
        itemsInFirstGroup.Dispose();

        image.Composite(header, 0, 0, CompositeOperator.Atop);
        
        MagickImage head;
        if (usingHead)
            head = new MagickImage(Program.apiOperator.ImageToPng(
                "https://q.qlogo.cn/g?b=qq&nk=" + userId + "&s=640", Environment.CurrentDirectory + "/temp",
                "head.png"));
        else
        {
            head = new MagickImage(Environment.CurrentDirectory +
                                   @"/resource/best50/best50_userhead_background.png");
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .FontPointSize(36)
                .FillColor(new MagickColor(0, 52428, 65535, 6553))
                .Text(3, 59, username.Substring(0, 1))
                .Draw(head);
        }

        head.Resize(65, 65);
        
        var plateImage = new MagickImage(AppContext.BaseDirectory + "resource/plate_images/" + plateVersionIndex + "/" + (int)category + ".png");

        plateImage.Resize(528, 85);
        
        var plateShadow = new MagickImage(AppContext.BaseDirectory + "resource/plate/plate_shadow.png");
        
        image.Composite(plateShadow, 0, 0, CompositeOperator.Atop);
        image.Composite(plateImage, 2, 15, CompositeOperator.Atop);
        image.Composite(nameFormImage, 0, 0, CompositeOperator.Atop);
        
        nameFormImage.Dispose();
        plateImage.Dispose();
        plateShadow.Dispose();

        var nameForm = new MagickImage("xc:transparent", new MagickReadSettings() { Width = 500, Height = 65 });
        new Drawables()
            .Font(Environment.CurrentDirectory + @"/resource/font-light.otf")
            .FontPointSize(36)
            .FillColor(new MagickColor(65535, 65535, 65535, 65535))
            .Text(2, 59, username)
            .Draw(nameForm);
        
        image.Composite(nameForm, 74, 25, CompositeOperator.Atop);
        image.Composite(head, 8, 25, CompositeOperator.Atop);
            
        nameForm.Dispose();
        head.Dispose();
        header.Dispose();
        
        new Drawables()
            .Font(Environment.CurrentDirectory + @"/resource/font-heavy.otf")
            .FontPointSize(20)
            .FillColor(new MagickColor(65535, 65535, 65535, 65535))
            .Text(6.3f, 162.3f, sortedDifficulties.Keys.ToArray()[0])
            .Draw(image);

        var top = (int)(171 + Math.Ceiling((float)sortedDifficulties.Values.ToArray()[0].Count / 7) * 100 + 75);
        for (int k = 1; k < sortedDifficulties.Values.ToArray().Length; k++)
        {
            i = 0;
            j = 0;
            var itemGroup = new MagickImage("xc:transparent",
                new MagickReadSettings() { Width = 700, Height = totalHeight });
            foreach (var song in sortedDifficulties.Values.ToArray()[k])
            {
                var item = GenerateItem(song.SongDto, song.LevelIndex, song.ScoreDto, category);
                itemGroup.Composite(item, i * 100, top + j * 100, CompositeOperator.Blend);
                item.Dispose();
                i++;
                if (i % 7 == 0)
                {
                    i = 0;
                    j++;
                }
            }
            /*
            var shadow = itemGroup.Clone();
            
            shadow.Resize(new Percentage(5));

            shadow.Shadow(0, 0, 100, new Percentage(50), new MagickColor(0, 0, 0));
            
            shadow.Resize(new Percentage(2000));

            shadow.Composite(itemGroup, Gravity.Center, CompositeOperator.Blend);
        
            image.Composite(shadow, Gravity.Center, CompositeOperator.Atop);
        
            shadow.Dispose();*/
            
            image.Composite(itemGroup, Gravity.Center, CompositeOperator.Atop);
        
            itemGroup.Dispose();

            var banner = new MagickImage(AppContext.BaseDirectory + "resource/plate/banner.png");
            
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font-heavy.otf")
                .FontPointSize(20)
                .FillColor(new MagickColor(65535, 65535, 65535, 65535))
                .Text(6.3f, 110.3f, sortedDifficulties.Keys.ToArray()[k])
                .Draw(banner);

            image.Composite(banner, 0, top - 75 - 44, CompositeOperator.Atop);

            top += (j + 1) * 100 + 75;
        }

        var watermark = new MagickImage(AppContext.BaseDirectory + "resource/plate/watermark.png");
        
        image.Composite(watermark,  17, totalHeight - 150, CompositeOperator.Atop);
        
        watermark.Dispose();

        var re = 0;
        var reAll = 0;
        var mas = 0;
        var masAll = 0;
        var exp = 0;
        var expAll = 0;
        var avd = 0;
        var avdAll = 0;
        var bas = 0;
        var basAll = 0;

        foreach (var song in allSongs)
        {
            switch (song.LevelIndex)
            {
                case 0:
                    basAll++;
                    if (song.ScoreDto.Achievements >= 80 && category == PlateCommand.PlateCategories.bazhe)
                        bas++;
                    else if (song.ScoreDto.Achievements >= 100 && category == PlateCommand.PlateCategories.jiang)
                        bas++;
                    else if ((song.ScoreDto.Fc == "ap" || song.ScoreDto.Fc == "app")&& category == PlateCommand.PlateCategories.shen)
                        bas++;
                    else if ((song.ScoreDto.Fc == "fc" || song.ScoreDto.Fc == "fcp")&& category == PlateCommand.PlateCategories.ji)
                        bas++;
                    else if ((song.ScoreDto.Fs == "fsd" || song.ScoreDto.Fs == "fsdp")&& category == PlateCommand.PlateCategories.wuwu)
                        bas++;
                    break;
                case 1:
                    avdAll++;
                    if (song.ScoreDto.Achievements >= 80 && category == PlateCommand.PlateCategories.bazhe)
                        avd++;
                    else if (song.ScoreDto.Achievements >= 100 && category == PlateCommand.PlateCategories.jiang)
                        avd++;
                    else if ((song.ScoreDto.Fc == "ap" || song.ScoreDto.Fc == "app")&& category == PlateCommand.PlateCategories.shen)
                        avd++;
                    else if ((song.ScoreDto.Fc == "fc" || song.ScoreDto.Fc == "fcp")&& category == PlateCommand.PlateCategories.ji)
                        avd++;
                    else if ((song.ScoreDto.Fs == "fsd" || song.ScoreDto.Fs == "fsdp")&& category == PlateCommand.PlateCategories.wuwu)
                        avd++;
                    break;
                case 2:
                    expAll++;
                    if (song.ScoreDto.Achievements >= 80 && category == PlateCommand.PlateCategories.bazhe)
                        exp++;
                    else if (song.ScoreDto.Achievements >= 100 && category == PlateCommand.PlateCategories.jiang)
                        exp++;
                    else if ((song.ScoreDto.Fc == "ap" || song.ScoreDto.Fc == "app")&& category == PlateCommand.PlateCategories.shen)
                        exp++;
                    else if ((song.ScoreDto.Fc == "fc" || song.ScoreDto.Fc == "fcp")&& category == PlateCommand.PlateCategories.ji)
                        exp++;
                    else if ((song.ScoreDto.Fs == "fsd" || song.ScoreDto.Fs == "fsdp")&& category == PlateCommand.PlateCategories.wuwu)
                        exp++;
                    break;
                case 3:
                    masAll++;
                    if (song.ScoreDto.Achievements >= 80 && category == PlateCommand.PlateCategories.bazhe)
                        mas++;
                    else if (song.ScoreDto.Achievements >= 100 && category == PlateCommand.PlateCategories.jiang)
                        mas++;
                    else if ((song.ScoreDto.Fc == "ap" || song.ScoreDto.Fc == "app")&& category == PlateCommand.PlateCategories.shen)
                        mas++;
                    else if ((song.ScoreDto.Fc == "fc" || song.ScoreDto.Fc == "fcp")&& category == PlateCommand.PlateCategories.ji)
                        mas++;
                    else if ((song.ScoreDto.Fs == "fsd" || song.ScoreDto.Fs == "fsdp")&& category == PlateCommand.PlateCategories.wuwu)
                        mas++;
                    break;
                case 4:
                    reAll++;
                    if (song.ScoreDto.Achievements >= 80 && category == PlateCommand.PlateCategories.bazhe)
                        re++;
                    else if (song.ScoreDto.Achievements >= 100 && category == PlateCommand.PlateCategories.jiang)
                        re++;
                    else if ((song.ScoreDto.Fc == "ap" || song.ScoreDto.Fc == "app")&& category == PlateCommand.PlateCategories.shen)
                        re++;
                    else if ((song.ScoreDto.Fc == "fc" || song.ScoreDto.Fc == "fcp")&& category == PlateCommand.PlateCategories.ji)
                        re++;
                    else if ((song.ScoreDto.Fs == "fsd" || song.ScoreDto.Fs == "fsdp")&& category == PlateCommand.PlateCategories.wuwu)
                        re++;
                    break;
            }
        }

        if (reAll != 0)
        {
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .TextAlignment(TextAlignment.Right)
                .FontPointSize(18)
                .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                .Text(700 - 17f, totalHeight - 125, "Re:MASTER 完成度：" + re + "/" + reAll)
                .Draw(image);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .TextAlignment(TextAlignment.Right)
                .FontPointSize(18)
                .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                .Text(700 - 17f, totalHeight - 100, "MASTER 完成度：" + mas + "/" + masAll)
                .Draw(image);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .TextAlignment(TextAlignment.Right)
                .FontPointSize(18)
                .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                .Text(700 - 17f, totalHeight - 75, "EXPERT 完成度：" + exp + "/" + expAll)
                .Draw(image);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .TextAlignment(TextAlignment.Right)
                .FontPointSize(18)
                .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                .Text(700 - 17f, totalHeight - 50, "ADVANCED 完成度：" + avd + "/" + avdAll)
                .Draw(image);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .TextAlignment(TextAlignment.Right)
                .FontPointSize(18)
                .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                .Text(700 - 17f, totalHeight - 25, "BASIC 完成度：" + bas + "/" + basAll)
                .Draw(image);
        }
        else
        {
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .TextAlignment(TextAlignment.Right)
                .FontPointSize(18)
                .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                .Text(700 - 17f, totalHeight - 100, "MASTER 完成度：" + mas + "/" + masAll)
                .Draw(image);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .TextAlignment(TextAlignment.Right)
                .FontPointSize(18)
                .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                .Text(700 - 17f, totalHeight - 75, "EXPERT 完成度：" + exp + "/" + expAll)
                .Draw(image);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .TextAlignment(TextAlignment.Right)
                .FontPointSize(18)
                .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                .Text(700 - 17f, totalHeight - 50, "ADVANCED 完成度：" + avd + "/" + avdAll)
                .Draw(image);
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font.otf")
                .TextAlignment(TextAlignment.Right)
                .FontPointSize(18)
                .FillColor(new MagickColor(65535, 65535, 65535, 32768))
                .Text(700 - 17f, totalHeight - 25, "BASIC 完成度：" + bas + "/" + basAll)
                .Draw(image);
        }

        if (isCompressed)
        {
            image.SetCompression(CompressionMethod.JPEG);
            image.Format = MagickFormat.Jpeg;
            image.Quality = 90;
        }

        var base64 = image.ToBase64();
        
        image.Dispose();

        return base64;
    }

    public bool isWhiteOnDark(MagickColor color)
    {
        if (color.R * 0.3 + color.G * 0.59 + color.B * 0.11 < 39303.6)
            return true;
        return false;
    }

    public MagickImage GenerateItem(SongDto songDto, int levelIndex , ScoresDto.ScoreDto scoreDto, PlateCommand.PlateCategories category)
    {
        var image = new MagickImage(AppContext.BaseDirectory + "resource/covers/" + songDto.Id + ".png");
        var gradient = new MagickImage(AppContext.BaseDirectory + "resource/plate/gradient.png");

        var dominantColor = GetDominantColor(AppContext.BaseDirectory + "resource/covers/" + songDto.Id + ".png");

        gradient.Colorize(dominantColor,
            new Percentage(100));

        image.Composite(gradient, 0, 0, CompositeOperator.Atop);

        gradient.Dispose();

        var whiteOnBlack = isWhiteOnDark(dominantColor);

        var textColor = new MagickColor();
        
        if (whiteOnBlack)
        {
            textColor = new MagickColor(65535, 65535, 65535);
        }
        else
        {
            textColor = new MagickColor(0, 0, 0);
        }
        
        var textLayer = new MagickImage("xc:transparent",
            new MagickReadSettings() { Width = 100, Height = 100 });
        new Drawables()
            .Font(Environment.CurrentDirectory + @"/resource/font-light.otf")
            .FontPointSize(18)
            .FillColor(textColor)
            .Text(7.6f, 37.2f, songDto.Title)
            .Draw(textLayer);
        textLayer.Composite(new MagickImage(AppContext.BaseDirectory + "resource/plate/gradient_text.png"),
            Channels.Alpha);

        new Drawables()
            .Font(Environment.CurrentDirectory + @"/resource/font.otf")
            .FontPointSize(10)
            .FillColor(new MagickColor(textColor.R, textColor.G, textColor.B, 32768))
            .Text(7.6f, 18.2f, "ID " + songDto.Id)
            .Draw(textLayer);

        var difficulty = "";

        switch (levelIndex)
        {
            case 0 :
                difficulty = "BAS";
                break;
            case 1:
                difficulty = "AVD";
                break;
            case 2:
                difficulty = "EXP";
                break;
            case 3:
                difficulty = "MAS";
                break;
            case 4:
                difficulty = "RE";
                break;
        }

        new Drawables()
            .Font(Environment.CurrentDirectory + @"/resource/font-heavy.otf")
            .FontPointSize(10)
            .FillColor(textColor)
            .Text(7.6f, 50.2f, difficulty)
            .Draw(textLayer);

        var indicatorText = "";
        
        if (category == PlateCommand.PlateCategories.bazhe && (scoreDto.Achievements >= 80))
        {
            if (scoreDto.Achievements >= 80)
                indicatorText = "Cleared";
        }

        if (category == PlateCommand.PlateCategories.jiang && (scoreDto.Achievements >= 100))
        {
            if (scoreDto.Achievements >= 100.5)
                indicatorText = "SSS+";
            else
                indicatorText = "SSS";
        }

        if (category == PlateCommand.PlateCategories.shen && (scoreDto.Fc == "ap" || scoreDto.Fc == "app"))
            if (scoreDto.Fc.Length > 2)
                indicatorText = scoreDto.Fc.Substring(0, scoreDto.Fc.Length - 1).ToUpper() + "+";
            else
                indicatorText = scoreDto.Fc.ToUpper();

        if (category == PlateCommand.PlateCategories.ji && (scoreDto.Fc == "fc" || scoreDto.Fc == "fcp"))
            if (scoreDto.Fc.Length > 2)
                indicatorText = scoreDto.Fc.Substring(0, scoreDto.Fc.Length - 1).ToUpper() + "+";
            else
                indicatorText = scoreDto.Fc.ToUpper();
        
        if (category == PlateCommand.PlateCategories.wuwu && (scoreDto.Fs == "fsd" || scoreDto.Fs == "fsdp"))
            if (scoreDto.Fs.Length > 2)
                indicatorText = scoreDto.Fs.Replace("p", "+").ToUpper();
            else
                indicatorText = scoreDto.Fs.ToUpper();

        if (indicatorText != "")
            new Drawables()
                .Font(Environment.CurrentDirectory + @"/resource/font-heavy.otf")
                .TextAlignment(TextAlignment.Right)
                .FontPointSize(10)
                .FillColor(textColor)
                .Text(100 - 7.6f, 18.2f, indicatorText)
                .Draw(textLayer);
        
        image.Composite(textLayer, 0, 0, CompositeOperator.Atop);
        textLayer.Dispose();

        return image;
    }
}