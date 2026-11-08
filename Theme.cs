using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AramDashboard
{
    public class Theme
    {
        public static readonly Color Background = Color.FromRgb(245, 243, 238);
        public static readonly Color Panel = Color.FromRgb(255, 255, 255);
        public static readonly Color PanelHover = Color.FromRgb(250, 246, 238);
        public static readonly Color Border = Color.FromRgb(224, 218, 207);
        public static readonly Color Text = Color.FromRgb(43, 38, 32);
        public static readonly Color Muted = Color.FromRgb(122, 113, 102);
        public static readonly Color Gold = Color.FromRgb(184, 132, 38);
        public static readonly Color Green = Color.FromRgb(66, 132, 82);

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
                "<Border x:Name='b' Background='#FFFFFF' BorderBrush='#E0DACF' BorderThickness='1' CornerRadius='7' Padding='16,0'>" +
                "<ContentPresenter VerticalAlignment='Center' HorizontalAlignment='Left'/>" +
                "</Border>" +
                "<ControlTemplate.Triggers>" +
                "<Trigger Property='IsMouseOver' Value='True'>" +
                "<Setter TargetName='b' Property='Background' Value='#FAF6EE'/>" +
                "<Setter TargetName='b' Property='BorderBrush' Value='#B88426'/>" +
                "</Trigger>" +
                "<Trigger Property='IsPressed' Value='True'>" +
                "<Setter TargetName='b' Property='Background' Value='#F1E8D8'/>" +
                "</Trigger>" +
                "</ControlTemplate.Triggers>" +
                "</ControlTemplate>";
            return (ControlTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
        }
    }
}
