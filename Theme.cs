using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AramDashboard
{
    public class Theme
    {
        public static readonly Color Background = Color.FromRgb(13, 18, 27);
        public static readonly Color Panel = Color.FromRgb(20, 28, 40);
        public static readonly Color PanelHover = Color.FromRgb(30, 41, 57);
        public static readonly Color Border = Color.FromRgb(54, 64, 82);
        public static readonly Color Text = Color.FromRgb(232, 237, 245);
        public static readonly Color Muted = Color.FromRgb(146, 158, 176);
        public static readonly Color Gold = Color.FromRgb(232, 184, 92);
        public static readonly Color Green = Color.FromRgb(120, 224, 160);

        public static SolidColorBrush Brush(Color color)
        {
            return new SolidColorBrush(color);
        }

        public static Button NavButton(string text)
        {
            Button button = new Button();
            button.Content = text;
            button.Height = 46;
            button.Margin = new Thickness(0, 0, 0, 6);
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.HorizontalContentAlignment = HorizontalAlignment.Left;
            button.Padding = new Thickness(18, 0, 18, 0);
            button.FontFamily = new FontFamily("Microsoft YaHei UI");
            button.FontSize = 14;
            button.Foreground = Brush(Text);
            button.Background = Brush(Panel);
            button.BorderBrush = Brush(Border);
            button.Template = ButtonTemplate();
            return button;
        }

        public static ControlTemplate ButtonTemplate()
        {
            string xaml =
                "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
                "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='{x:Type Button}'>" +
                "<Border x:Name='b' Background='#14202C' BorderBrush='#364052' BorderThickness='1' CornerRadius='8' Padding='16,0'>" +
                "<ContentPresenter VerticalAlignment='Center' HorizontalAlignment='Left'/>" +
                "</Border>" +
                "<ControlTemplate.Triggers>" +
                "<Trigger Property='IsMouseOver' Value='True'>" +
                "<Setter TargetName='b' Property='Background' Value='#1E2939'/>" +
                "<Setter TargetName='b' Property='BorderBrush' Value='#E8B85C'/>" +
                "</Trigger>" +
                "<Trigger Property='IsPressed' Value='True'>" +
                "<Setter TargetName='b' Property='Background' Value='#0D141E'/>" +
                "</Trigger>" +
                "</ControlTemplate.Triggers>" +
                "</ControlTemplate>";
            return (ControlTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
        }
    }
}
