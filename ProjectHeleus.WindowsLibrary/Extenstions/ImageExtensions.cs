﻿namespace ProjectHeleus.WindowsLibrary.Extenstions
{
    using System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media.Imaging;

    public class ImageExtensions
    {
        public static readonly DependencyProperty DecodableUriSourceProperty = DependencyProperty.RegisterAttached(
            "DecodableUriSource", typeof(object), typeof(Image), new PropertyMetadata(null));

        public static readonly DependencyProperty DecodePixelHeightProperty = DependencyProperty.RegisterAttached(
            "DecodePixelHeight", typeof(int), typeof(Image), new PropertyMetadata(0));

        public static readonly DependencyProperty DecodePixelTypeProperty = DependencyProperty.RegisterAttached(
            "DecodePixelType", typeof(DecodePixelType), typeof(Image), new PropertyMetadata(DecodePixelType.Physical));

        public static readonly DependencyProperty DecodePixelWidthProperty = DependencyProperty.RegisterAttached(
            "DecodePixelWidth", typeof(int), typeof(Image), new PropertyMetadata(0));

        public static object GetDecodableUriSource(UIElement element)
        {
            return (object)element.GetValue(DecodableUriSourceProperty);
        }

        public static void SetDecodableUriSource(UIElement element, object value)
        {
            Image img = (Image)element;

            element.SetValue(DecodableUriSourceProperty, value);

            img.Source = new BitmapImage(new Uri(value as string))
            {
                DecodePixelHeight = GetDecodePixelHeight(img),
                DecodePixelWidth = GetDecodePixelWidth(img),
                DecodePixelType = GetDecodePixelType(img)
            };
        }

        public static int GetDecodePixelHeight(UIElement element)
        {
            return (int)element.GetValue(DecodePixelHeightProperty);
        }

        public static void SetDecodePixelHeight(UIElement element, int value)
        {
            element.SetValue(DecodePixelHeightProperty, value);
        }

        public static DecodePixelType GetDecodePixelType(UIElement element)
        {
            return (DecodePixelType)element.GetValue(DecodePixelTypeProperty);
        }

        public static void SetDecodePixelType(UIElement element, DecodePixelType value)
        {
            element.SetValue(DecodePixelTypeProperty, value);
        }

        public static int GetDecodePixelWidth(UIElement element)
        {
            return (int)element.GetValue(DecodePixelWidthProperty);
        }

        public static void SetDecodePixelWidth(UIElement element, int value)
        {
            element.SetValue(DecodePixelWidthProperty, value);
        }

    }
}