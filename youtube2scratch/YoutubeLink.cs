using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using YoutubeExtractor;

namespace youtube2scratch
{
    public class YoutubeLink : INotifyPropertyChanged
    {

        private string m_link = "";
        private string m_title = "";
        private string m_image = "youtube.png";
        private double m_progress = 0;
        private string m_filename = "";

        private IEnumerable<VideoInfo> m_videoInfos = null;

        private readonly BackgroundWorker m_infoWorker = new BackgroundWorker();
        private readonly BackgroundWorker m_audioWorker = new BackgroundWorker();
        private readonly BackgroundWorker m_downloadWorker = new BackgroundWorker();

        public YoutubeLink()
        {
            m_infoWorker.DoWork += m_infoWorker_DoWork;
            m_audioWorker.DoWork += m_audioWorker_DoWork;
            m_downloadWorker.DoWork += m_downloadWorker_DoWork;
        }

        public string link {
            get
            {
                return m_link;
            }
            set
            {
                if(value != m_link)
                {
                    m_link = value;
                    OnPropertyChanged("link");
                    getVideoInfo();
                }
            }

        }

        public string title
        {
            get
            {
                return m_title;
            }
            set
            {
                if(value != m_title)
                {
                    m_title = value;
                    OnPropertyChanged("title");
                }
            }
        }

        public string image
        {
            get
            {
                return m_image;
            }
            set
            {
                if(value != m_image)
                {
                    m_image = value;
                    OnPropertyChanged("image");
                }
            }
        }

        public double progress
        {
            get
            {
                return m_progress;
            }
            set
            {
                if(value != m_progress)
                {
                    m_progress = value;
                    OnPropertyChanged("progress");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Methods

        private void getVideoInfo()
        {
            if(!m_infoWorker.IsBusy)
            {
                m_infoWorker.RunWorkerAsync();
            }
        }

        private void m_infoWorker_DoWork(object sender, DoWorkEventArgs e)
        {
 	        try
            {
                if (!isValidUrl(m_link) || !m_link.ToLower().Contains("www.youtube.com/watch?"))
                {
                    Console.WriteLine("Invalid youtube URL.");
                }
                else
                {
                    this.image = string.Format("http://i3.ytimg.com/vi/{0}/default.jpg", GetVideoIDFromUrl(m_link));
                    m_videoInfos = DownloadUrlResolver.GetDownloadUrls(m_link);
                    this.title = m_videoInfos.First().Title;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void m_downloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                ImageExporter exporter = new ImageExporter();
                exporter.export(m_videoInfos);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void m_audioWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                WAVexporter exporter = new WAVexporter();
                exporter.export(m_videoInfos);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void downloadVideo()
        {
            if(m_videoInfos != null)
            {
                m_downloadWorker.RunWorkerAsync();
                m_audioWorker.RunWorkerAsync();
            }
            else
            {
                getVideoInfo();
            }
        }

        // Helper Methods

        private bool isValidUrl(string url)
        {
            string pattern = @"^(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?$";
            Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return regex.IsMatch(url);
        }

        private string GetVideoIDFromUrl(string url)
        {
            url = url.Substring(url.IndexOf("?") + 1);
            string[] props = url.Split('&');

            string videoid = "";
            foreach (string prop in props)
            {
                if (prop.StartsWith("v="))
                    videoid = prop.Substring(prop.IndexOf("v=") + 2);
            }

            return videoid;
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
