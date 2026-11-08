using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AramDashboard
{
    public class MainWindow : Window
    {
        private readonly ContentControl _content;
        private readonly TextBlock _title;
        private readonly TextBlock _subtitle;
        private readonly DataStore _store;

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

            _store = new DataStore(AppDomain.CurrentDomain.BaseDirectory);

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
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            StackPanel mainStack = new StackPanel();

            _title = new TextBlock();
            _title.Text = "推荐";
            _title.FontSize = 28;
            _title.FontWeight = FontWeights.SemiBold;
            _title.Foreground = Theme.Brush(Theme.Text);
            mainStack.Children.Add(_title);

            _subtitle = new TextBlock();
            _subtitle.Text = "";
            _subtitle.FontSize = 12;
            _subtitle.Margin = new Thickness(0, 6, 0, 24);
            _subtitle.Foreground = Theme.Brush(Theme.Muted);
            mainStack.Children.Add(_subtitle);

            _content = new ContentControl();
            mainStack.Children.Add(_content);
            scroll.Content = mainStack;
            main.Child = scroll;
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
            if (page == "数据")
            {
                _subtitle.Text = "英雄、海克斯与英雄×海克斯统计";
                _content.Content = BuildDataPage();
            }
            else if (page == "战绩")
            {
                _subtitle.Text = "最近对局与队友表现";
                _content.Content = BuildHistoryPage();
            }
            else if (page == "设置")
            {
                _subtitle.Text = "本地配置";
                _content.Content = BuildSettingsPage();
            }
            else
            {
                _subtitle.Text = "根据当前数据生成的推荐";
                _content.Content = BuildRecommendPage();
            }
        }

        private Border Panel(UIElement child)
        {
            Border panel = new Border();
            panel.Background = Theme.Brush(Theme.Panel);
            panel.BorderBrush = Theme.Brush(Theme.Border);
            panel.BorderThickness = new Thickness(1);
            panel.CornerRadius = new CornerRadius(10);
            panel.Padding = new Thickness(20);
            panel.Margin = new Thickness(0, 0, 0, 14);
            panel.Child = child;
            return panel;
        }

        private UIElement BuildRecommendPage()
        {
            StackPanel stack = new StackPanel();
            TextBlock intro = new TextBlock();
            intro.Text = "全局海克斯强度（按适配分排序）";
            intro.FontSize = 14;
            intro.Margin = new Thickness(0, 0, 0, 10);
            intro.Foreground = Theme.Brush(Theme.Gold);
            stack.Children.Add(intro);

            DataGrid grid = MakeAugmentGrid();
            List<AugmentRow> rows = new List<AugmentRow>();
            int count = Math.Min(40, _store.TopAugments.Count);
            for (int i = 0; i < count; i++)
            {
                rows.Add(_store.TopAugments[i]);
            }
            grid.ItemsSource = rows;
            stack.Children.Add(grid);
            return Panel(stack);
        }

        private UIElement BuildDataPage()
        {
            StackPanel stack = new StackPanel();
            TextBlock intro = new TextBlock();
            intro.Text = "英雄胜率 · 数据日期 " + (_store.DataDate ?? "--");
            intro.FontSize = 14;
            intro.Margin = new Thickness(0, 0, 0, 10);
            intro.Foreground = Theme.Brush(Theme.Gold);
            stack.Children.Add(intro);

            TextBox search = new TextBox();
            search.Height = 34;
            search.Margin = new Thickness(0, 0, 0, 10);
            search.Padding = new Thickness(10, 0, 10, 0);
            search.Background = Theme.Brush(Theme.Background);
            search.Foreground = Theme.Brush(Theme.Text);
            search.BorderBrush = Theme.Brush(Theme.Border);
            stack.Children.Add(search);

            DataGrid list = MakeChampionGrid();
            stack.Children.Add(list);

            Action refresh = delegate
            {
                List<ChampionRow> rows = new List<ChampionRow>();
                string query = search.Text.Trim();
                int shown = 0;
                for (int i = 0; i < _store.Champions.Count && shown < 100; i++)
                {
                    ChampionRow row = _store.Champions[i];
                    if (query.Length > 0 && row.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }
                    rows.Add(row);
                    shown++;
                }
                list.ItemsSource = rows;
            };
            search.TextChanged += delegate { refresh(); };
            refresh();
            return Panel(stack);
        }

        private UIElement BuildHistoryPage()
        {
            StackPanel stack = new StackPanel();
            TextBlock text = new TextBlock();
            text.Text = "最近对局";
            text.FontSize = 16;
            text.Foreground = Theme.Brush(Theme.Gold);
            stack.Children.Add(text);

            TextBlock detail = new TextBlock();
            detail.Text = "LCU 战绩接入中。\n\n这里将显示最近对局、队友/对手、英雄、海克斯和装备。";
            detail.FontSize = 13;
            detail.LineHeight = 23;
            detail.Margin = new Thickness(0, 10, 0, 0);
            detail.Foreground = Theme.Brush(Theme.Muted);
            stack.Children.Add(detail);
            return Panel(stack);
        }

        private UIElement BuildSettingsPage()
        {
            StackPanel stack = new StackPanel();
            AddSetting(stack, "数据目录", _store.DataDir);
            AddSetting(stack, "数据日期", _store.DataDate ?? "--");
            AddSetting(stack, "英雄数量", _store.ChampionCount.ToString());
            AddSetting(stack, "海克斯数量", _store.AugmentCount.ToString());
            AddSetting(stack, "账号登录", "无");
            AddSetting(stack, "遥测", "无");
            Button reload = Theme.NavButton("重新加载数据");
            reload.Width = 180;
            reload.HorizontalAlignment = HorizontalAlignment.Left;
            reload.Click += delegate
            {
                _store.Load();
                ShowPage("设置");
            };
            stack.Children.Add(reload);
            return Panel(stack);
        }

        private static void AddSetting(StackPanel stack, string key, string value)
        {
            TextBlock line = new TextBlock();
            line.Text = key + "：" + value;
            line.FontSize = 13;
            line.Margin = new Thickness(0, 0, 0, 10);
            line.Foreground = Theme.Brush(Theme.Muted);
            stack.Children.Add(line);
        }

        private static DataGrid MakeChampionGrid()
        {
            DataGrid grid = BaseGrid();
            grid.Columns.Add(TextColumn("#", "Rank", 50));
            grid.Columns.Add(TextColumn("英雄", "Name", 240));
            grid.Columns.Add(TextColumn("胜率", "WinRate", 120, "{0:P1}"));
            grid.Columns.Add(TextColumn("选取率", "PickRate", 120, "{0:P1}"));
            return grid;
        }

        private static DataGrid MakeAugmentGrid()
        {
            DataGrid grid = BaseGrid();
            grid.Columns.Add(TextColumn("海克斯", "Name", 260));
            grid.Columns.Add(TextColumn("评级", "Rating", 100));
            grid.Columns.Add(TextColumn("分数", "Score", 80));
            grid.Columns.Add(TextColumn("胜率", "WinRate", 100, "{0:0.0}%"));
            return grid;
        }

        private static DataGrid BaseGrid()
        {
            DataGrid grid = new DataGrid();
            grid.Height = 500;
            grid.AutoGenerateColumns = false;
            grid.IsReadOnly = true;
            grid.HeadersVisibility = DataGridHeadersVisibility.Column;
            grid.GridLinesVisibility = DataGridGridLinesVisibility.None;
            grid.Background = Theme.Brush(Theme.Panel);
            grid.Foreground = Theme.Brush(Theme.Text);
            grid.BorderThickness = new Thickness(0);
            grid.RowBackground = Theme.Brush(Theme.Panel);
            grid.AlternatingRowBackground = Theme.Brush(Color.FromRgb(24, 33, 47));
            grid.HorizontalGridLinesBrush = Theme.Brush(Theme.Border);
            return grid;
        }

        private static DataGridTextColumn TextColumn(string header, string path, double width, string format = null)
        {
            DataGridTextColumn column = new DataGridTextColumn();
            column.Header = header;
            column.Width = width;
            Binding binding = new Binding(path);
            if (format != null)
            {
                binding.StringFormat = format;
            }
            column.Binding = binding;
            return column;
        }
    }
}
