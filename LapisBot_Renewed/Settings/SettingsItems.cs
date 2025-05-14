namespace LapisBot.Settings;

public static class SettingsItems
{
    public static readonly SettingsCategory[] Categories =
    [
        new()
        {
            DisplayName = "舞萌相关",
            Items =
            [
                new SettingsItemsOfACommand
                {
                    DisplayName = "Best 50",
                    Identifier = "b50",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "歌曲信息",
                    Identifier = "info",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        },
                        new SettingsItem
                        {
                            Identifier = "2",
                            DisplayName = "歌曲试听",
                            DefaultValue = false
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "随机歌曲",
                    Identifier = "random",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        },
                        new SettingsItem
                        {
                            Identifier = "2",
                            DisplayName = "歌曲试听",
                            DefaultValue = false
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "别名查询",
                    Identifier = "alias",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "别名添加",
                    Identifier = "aliasadd",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "牌子查询",
                    Identifier = "plate",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "歌曲猜谜",
                    Identifier = "song",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "歌名开字母",
                    Identifier = "letter",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "歌曲搜索",
                    Identifier = "search",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "舞萌信息绑定",
                    Identifier = "bind",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "水鱼网舞萌成绩更新",
                    Identifier = "update",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                }
            ]
        },
        new()
        {
            DisplayName = "贴纸生成相关",
            Items =
            [
                new SettingsItemsOfACommand
                {
                    DisplayName = "喜报贴纸生成",
                    Identifier = "fortune",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "悲报贴纸生成",
                    Identifier = "obituary",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                }
            ]
        },
        new()
        {
            DisplayName = "单词相关",
            Items =
            [
                new SettingsItemsOfACommand
                {
                    DisplayName = "猜单词",
                    Identifier = "word",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "查单词",
                    Identifier = "dictionary",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                }
            ]
        },
        new()
        {
            DisplayName = "杂项",
            Items =
            [
                new SettingsItemsOfACommand
                {
                    DisplayName = "简洁指令",
                    Identifier = "litecommand",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "图片压缩",
                    Identifier = "compress",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "帮助",
                    Identifier = "help",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "关于",
                    Identifier = "about",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = true
                        }
                    ]
                }
            ]
        }
    ];
}