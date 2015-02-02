using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using System.IO;
using YoutubeExtractor;
using System.Windows.Controls;
using System.Windows.Data;

namespace youtube2scratch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private YoutubeLink youtubeLink = null;

        public MainWindow()
        {
            InitializeComponent();

            youtubeLink = new YoutubeLink();

            VideoLink.SetBinding(TextBox.TextProperty, new Binding("link")
            {
                Source = youtubeLink,
                Mode = BindingMode.OneWayToSource,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            VideoTitle.SetBinding(Label.ContentProperty, new Binding("title")
            {
                Source = youtubeLink,
                Mode = BindingMode.OneWay
            });

            VideoProgress.SetBinding(ProgressBar.ValueProperty, new Binding("progress")
            {
                Source = youtubeLink,
                Mode = BindingMode.OneWay
            });

            VideoImage.SetBinding(Image.SourceProperty, new Binding("image")
            {
                Source = youtubeLink,
                Mode = BindingMode.OneWay
            });

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            youtubeLink.downloadVideo();
        }
    }
}
