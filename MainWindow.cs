using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AramDashboard
{
    public class MainWindow : Window
    {
        private readonly ContentControl _content;
        private readonly TextBlock _title;
        private readonly TextBlock _subtitle;

        public MainWindow()
        {
            Title = "ARAM Dashboard";
            Width = 1180;
            Height = 760;
            MinWidth = 960;
            MinHeight = 640;
            Background = Theme.Brush(Theme.Background);
            Foreground = Theme.Brush(Theme.Text);
            FontFamily = new FontFamily("Microsoft YaHei UI");
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Grid root = new Grid();
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Border sidebar = new Border();
            sidebar.Background = Theme.Brush(Color.FromRgb(10, 14, 21));
            sidebar.BorderBrush = Theme.Brush(Theme.Border);
            sidebar.BorderThickness = new Thickness(0, 0, 1, 0);
            sidebar.Padding = new Thickness(18, 22, 18, 18);

            StackPanel nav = new StackPanel();
            TextBlock logo = new TextBlock();
            logo.Text = "ARAM";
            logo.FontSize = 26;
            logo.FontWeight = FontWeights.Bold;
            logo.Foreground = Theme.Brush(Theme.Gold);
            nav.Children.Add(logo);

            TextBlock logoSub = new TextBlock();
            logoSub.Text = "DASHBOARD";
            logoSub.FontSize = 10;
            logoSub.Margin = new Thickness(1, 0, 0, 28);
            logoSub.Foreground = Theme.Brush(Theme.Muted);
            nav.Children.Add(logoSub);

            nav.Children.Add(Nav("推荐", "推荐"));
            nav.Children.Add(Nav("数据", "数据"));
            nav.Children.Add(Nav("战绩", "战绩"));
            nav.Children.Add(Nav("设置", "设置"));

            TextBlock version = new TextBlock();
            version.Text = "自建数据 · 无账号登录";
            version.FontSize = 11;
            version.Margin = new Thickness(2, 30, 0, 0);
            version.Foreground = Theme.Brush(Theme.Muted);
            nav.Children.Add(version);

            sidebar.Child = nav;
            Grid.SetColumn(sidebar, 0);
            root.Children.Add(sidebar);

            Border main = new Border();
            main.Padding = new Thickness(34, 28, 34, 28);
            StackPanel mainStack = new StackPanel();

            _title = new TextBlock();
            _title.Text = "推荐";
            _title.FontSize = 28;
            _title.FontWeight = FontWeights.SemiBold;
            _title.Foreground = Theme.Brush(Theme.Text);
            mainStack.Children.Add(_title);

            _subtitle = new TextBlock();
            _subtitle.Text = "根据当前数据生成的推荐";
            _subtitle.FontSize = 12;
            _subtitle.Margin = new Thickness(0, 6, 0, 24);
            _subtitle.Foreground = Theme.Brush(Theme.Muted);
            mainStack.Children.Add(_subtitle);

            _content = new ContentControl();
            _content.Content = Page("推荐", "英雄推荐、海克斯推荐、出装与三海克斯组合会在这里展示。");
            mainStack.Children.Add(_content);
            main.Child = mainStack;
            Grid.SetColumn(main, 1);
            root.Children.Add(main);

            Content = root;
            ShowPage("推荐");
        }

        private Button Nav(string text, string page)
        {
            Button button = Theme.NavButton(text);
            button.Click += delegate { ShowPage(page); };
            return button;
        }

        private void ShowPage(string page)
        {
            _title.Text = page;
            string subtitle;
            string body;
            if (page == "数据")
            {
                subtitle = "英雄、海克斯与英雄×海克斯统计";
                body = "数据页：英雄强度、海克斯价值、筛选、搜索、排序。\n\n数据来自自己的数据库与 101/RESG，不依赖 HexBox 账号。";
            }
            else if (page == "战绩")
            {
                subtitle = "最近对局与队友表现";
                body = "战绩页：最近对局、英雄/海克斯/装备、队友与对手。\n\n数据从 LCU 只读获取。";
            }
            else if (page == "设置")
            {
                subtitle = "本地配置";
                body = "设置页：客户端路径、数据源、OCR、外观。\n\n无账号登录，无遥测。";
            }
            else
            {
                subtitle = "根据当前数据生成的推荐";
                body = "推荐页：今日推荐、英雄推荐、海克斯推荐、出装/加点、三海克斯组合。\n\n后续接入本地数据库。";
            }
            _subtitle.Text = subtitle;
            _content.Content = Page(page, body);
        }

        private Border Page(string title, string body)
        {
            Border panel = new Border();
            panel.Background = Theme.Brush(Theme.Panel);
            panel.BorderBrush = Theme.Brush(Theme.Border);
            panel.BorderThickness = new Thickness(1);
            panel.CornerRadius = new CornerRadius(10);
            panel.Padding = new Thickness(24);
            panel.Margin = new Thickness(0, 0, 0, 12);

            StackPanel stack = new StackPanel();
            TextBlock heading = new TextBlock();
            heading.Text = title;
            heading.FontSize = 18;
            heading.FontWeight = FontWeights.SemiBold;
            heading.Foreground = Theme.Brush(Theme.Gold);
            stack.Children.Add(heading);

            TextBlock text = new TextBlock();
            text.Text = body;
            text.FontSize = 13;
            text.LineHeight = 23;
            text.TextWrapping = TextWrapping.Wrap;
            text.Margin = new Thickness(0, 12, 0, 0);
            text.Foreground = Theme.Brush(Theme.Muted);
            stack.Children.Add(text);
            panel.Child = stack;
            return panel;
        }
    }
}
