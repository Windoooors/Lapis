using System.Collections.Generic;
using System.Linq;
using Lapis.Commands.GroupCommands.VocabularyCommands;
using Lapis.Operations.ImageOperation;

namespace Lapis.ImageGenerators;

public class WordleGameImageGenerator
{
    private const float Padding = 30;
    private const float WordBoxWidth = 50;
    private const float SpaceBetweenWordAndKeyboard = 20;
    private const float SpaceBetweenWordBoxes = 10;
    private const float KeyboardBoxWidth = 25;
    private const float KeyboardBoxHeight = 40;
    private const float SpaceBetweenKeyBoxes = 4.3f;

    private const int WordBoxFontSize = 24;
    private const int KeyboardFontSize = 18;

    private readonly Color _correctColor = new(0.275f, 0.522f, 0.149f, 1f);
    private readonly Color _gray = new(0.1f, 0.1f, 0.1f, 1);
    private readonly Color _incorrectColor = new(0.522f, 0.443f, 0.0275f, 1f);

    private readonly string[] _keyboardLetters = ["qwertyuiop", "asdfghjkl", "zxcvbnm"];
    private readonly Color _lightGray = new(0.3f, 0.3f, 0.3f, 1);

    public string Generate(WordleCommand.WordleGame game, bool compressed)
    {
        var imageHeight = Padding * 2
                          + WordBoxWidth * game.MaxTries
                          + SpaceBetweenWordBoxes * (game.MaxTries - 1) +
                          (game.WordLength >= 5
                              ? SpaceBetweenWordAndKeyboard
                                + KeyboardBoxHeight * 3 + SpaceBetweenKeyBoxes * 2
                              : 0);

        var imageWidth = WordBoxWidth * game.WordLength + SpaceBetweenWordBoxes * (game.WordLength - 1) + Padding * 2;

        using var image = new Image((int)imageWidth, (int)imageHeight, Color.LapisThemeColor);

        var colorMaps = new List<int[]>();
        for (var i = 0; i < game.MaxTries; i++) colorMaps.Add(new int[game.WordLength]);

        DrawWordBoxes(image);
        if (game.WordLength >= 5)
            DrawKeyBoxes(image);

        return image.ToBase64(compressed);

        void DrawWordBoxes(Image backgroundImage)
        {
            var left = Padding;
            var top = Padding;

            for (var i = 0; i < game.MaxTries; i++)
            {
                var trueAnswerLetterCount = new Dictionary<char, int>();

                foreach (var letter in game.Word.Word.ToLower())
                    if (!trueAnswerLetterCount.TryAdd(letter, 1))
                        trueAnswerLetterCount[letter]++;

                //var colorMap = new int[game.WordLength];
                var guessedWords = game.GuessedWords.ToList();

                for (var j = 0; j < game.WordLength; j++)
                    if (i >= guessedWords.Count)
                    {
                        colorMaps[i][j] = 0;
                    }
                    else
                    {
                        var letter = guessedWords[i].ToLower()[j];

                        if (game.Word.Word.ToLower()[j] == letter)
                        {
                            trueAnswerLetterCount[letter]--;
                            colorMaps[i][j] = 2;
                        }
                    }

                for (var j = 0; j < game.WordLength; j++)
                    if (i >= guessedWords.Count)
                    {
                        colorMaps[i][j] = 0;
                    }
                    else
                    {
                        var letter = guessedWords[i].ToLower()[j];

                        if (trueAnswerLetterCount.TryGetValue(letter, out var count))
                        {
                            if (count > 0 && colorMaps[i][j] != 2)
                            {
                                colorMaps[i][j] = 1;
                                trueAnswerLetterCount[letter]--;
                            }

                            if (count == 0 && colorMaps[i][j] != 2)
                                colorMaps[i][j] = 0;
                        }
                        else
                        {
                            colorMaps[i][j] = 0;
                        }
                    }

                for (var j = 0; j < game.WordLength; j++)
                {
                    using var keyBoxImage = GenerateWordBox(j, i, colorMaps[i][j]);

                    backgroundImage.DrawImage(keyBoxImage, (int)left, (int)top);

                    left += WordBoxWidth + SpaceBetweenWordBoxes;
                }

                left = Padding;
                top += WordBoxWidth + SpaceBetweenWordBoxes;
            }
        }

        Image GenerateWordBox(int horizontalIndex, int verticalTop, int colorLevel)
        {
            if (verticalTop >= game.GuessedWords.Count) return new Image((int)WordBoxWidth, (int)WordBoxWidth, _gray);

            var letter = game.GuessedWords.ToList()[verticalTop].ToLower()[horizontalIndex];

            var color = colorLevel switch
            {
                2 => _correctColor,
                1 => _incorrectColor,
                _ => _gray
            };

            var resultBoxImage = new Image((int)WordBoxWidth, (int)WordBoxWidth, color);

            resultBoxImage.DrawText(letter.ToString().ToUpper(), Color.White, WordBoxFontSize, FontWeight.Regular,
                HorizontalAlignment.Center, WordBoxWidth / 2, WordBoxWidth / 2 + 8);

            return resultBoxImage;
        }

        void DrawKeyBoxes(Image backgroundImage)
        {
            var top = imageHeight - Padding - SpaceBetweenKeyBoxes * 2 - KeyboardBoxHeight * 3;

            var keyWidth = KeyboardBoxWidth * _keyboardLetters[0].Length +
                           SpaceBetweenKeyBoxes * (_keyboardLetters[0].Length - 1);

            var left = (imageWidth - keyWidth) / 2;
            var originalLeft = left;

            for (var i = 0; i < 3; i++)
            {
                foreach (var letter in _keyboardLetters[i])
                {
                    using var keyboardImage = GenerateKeyBoxImage(letter);
                    backgroundImage.DrawImage(keyboardImage, (int)left, (int)top);

                    left += KeyboardBoxWidth + SpaceBetweenKeyBoxes;
                }

                top += KeyboardBoxHeight + SpaceBetweenKeyBoxes;
                left = (int)(originalLeft + i switch
                {
                    0 => 0.45 * (KeyboardBoxWidth + SpaceBetweenWordBoxes),
                    _ => 1.29 * (KeyboardBoxWidth + SpaceBetweenWordBoxes)
                });
            }
        }

        Image GenerateKeyBoxImage(char letter)
        {
            var colorLevel = 0;

            for (var i = 0; i < game.GuessedWords.Count; i++)
            for (var j = 0; j < game.WordLength; j++)
                if (game.GuessedWords.ToList()[i][j] == letter)
                {
                    if (colorLevel == 0)
                        colorLevel = -1;

                    if (colorMaps[i][j] != 0 && colorMaps[i][j] > colorLevel)
                        colorLevel = colorMaps[i][j];
                }

            var color = colorLevel switch
            {
                -1 => _lightGray,
                1 => _incorrectColor,
                2 => _correctColor,
                _ => _gray
            };

            var keyBoxImage = new Image((int)KeyboardBoxWidth, (int)KeyboardBoxHeight, color);

            keyBoxImage.DrawText(letter.ToString().ToUpper(), Color.White, KeyboardFontSize, FontWeight.Regular,
                HorizontalAlignment.Center,
                KeyboardBoxWidth / 2, KeyboardBoxHeight / 2 + 7);

            return keyBoxImage;
        }
    }
}