using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using YoutubeExtractor;
using System.Drawing;

using AForge.Video.FFMPEG;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.IO;
using System.Text;

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

        private Dictionary<int, string> m_dictionary = null;

        public YoutubeLink()
        {
            m_dictionary = new Dictionary<int, string>();

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
            /*
             * Select the first .mp4 video with 360p resolution
             */
            VideoInfo video = m_videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 480);

            /*
             * If the video has a decrypted signature, decipher it
             */
            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            m_filename = System.IO.Path.Combine("C:/temp", video.Title);

            var videoDownloader = new VideoDownloader(video, m_filename + video.VideoExtension);

            videoDownloader.DownloadProgressChanged += (s, args) => this.progress = args.ProgressPercentage;

            videoDownloader.Execute();

            cutvideo(video.VideoExtension);
        }

        private void cutvideo(string extension)
        {
            try
            {
                VideoFileReader reader = new VideoFileReader();
                reader.Open(m_filename + extension);
                Bitmap videoFrame = null;
                int frame = 0;
                int scratchfile = 0;
                while((videoFrame = reader.ReadVideoFrame()) != null)
                {
                    if( frame % reader.FrameRate == 0)
                    {
                        Console.WriteLine("Frame: " + frame);

                        byte[] buffer = imageToByteArray(videoFrame);
                        m_dictionary.Add(scratchfile, getMd5Hash(buffer));
                        videoFrame.Save("C:/temp/" + scratchfile.ToString() + ".png", ImageFormat.Png);
                        videoFrame.Dispose();
                        videoFrame = null;
                        ++scratchfile;
                    }
                    ++frame;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static byte[] imageToByteArray(Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        private static string getMd5Hash(byte[] buffer)
        {
            MD5 md5Hasher = MD5.Create();

            byte[] data = md5Hasher.ComputeHash(buffer);

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        void m_audioWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                /*
                 * We want the first extractable video with the highest audio quality.
                 */
                VideoInfo video = m_videoInfos
                    .Where(info => info.CanExtractAudio)
                    .OrderByDescending(info => info.AudioBitrate)
                    .First();

                /*
                 * If the video has a decrypted signature, decipher it
                 */
                if (video.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                }
                /*
                 * Create the audio downloader.
                 * The first argument is the video where the audio should be extracted from.
                 * The second argument is the path to save the audio file.
                 */
                var audioDownloader = new AudioDownloader(video, System.IO.Path.Combine("C:/temp", video.Title + video.AudioExtension));

                // Register the progress events. We treat the download progress as 85% of the progress and the extraction progress only as 15% of the progress,
                // because the download will take much longer than the audio extraction.
            
            
                // audioDownloader.DownloadProgressChanged += (s, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
                // audioDownloader.AudioExtractionProgressChanged += (s, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);

                /*
                 * Execute the audio downloader.
                 * For GUI applications note, that this method runs synchronously.
                 */
                audioDownloader.Execute();
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
                // m_audioWorker.RunWorkerAsync();
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
