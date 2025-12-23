namespace Lapis.Settings;

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
                    Identifier = "alias_add",
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
                    DisplayName = "谱面分数线",
                    Identifier = "cutoff",
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
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "玩家信息查询",
                    Identifier = "me",
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
            DisplayName = "群聊相关",
            Items =
            [
                new SettingsItemsOfACommand
                {
                    DisplayName = "娶群友",
                    Identifier = "marry",
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
                            DisplayName = "需要用户同意使用群友互动功能",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "透群友",
                    Identifier = "rape",
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
                            DisplayName = "需要用户同意使用群友互动功能",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "被群友透",
                    Identifier = "being_raped",
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
                            DisplayName = "需要用户同意使用群友互动功能",
                            DefaultValue = true
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "群友搜索",
                    Identifier = "msearch",
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
                    DisplayName = "群友别名查询",
                    Identifier = "malias",
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
                    DisplayName = "群友别名添加",
                    Identifier = "malias_add",
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
                    DisplayName = "聊天记录总结",
                    Identifier = "tldr",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = false
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "透群友排行榜",
                    Identifier = "rape_rank",
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
                    DisplayName = $"禁言 {BotConfiguration.Instance.BotName}",
                    Identifier = "mute",
                    Items =
                    [
                        new SettingsItem
                        {
                            Identifier = "1",
                            DisplayName = "启用",
                            DefaultValue = false
                        }
                    ]
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "简洁指令",
                    Identifier = "lite_command",
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
                },
                new SettingsItemsOfACommand
                {
                    DisplayName = "退群",
                    Identifier = "quit",
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