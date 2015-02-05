using AForge.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using YoutubeExtractor;

namespace youtube2scratch
{
    public class ImageExporter
    {
        private string m_mp4File = null;
        private Dictionary<int, string> m_dictionary = new Dictionary<int,string>();

        public void export(IEnumerable<VideoInfo> videoInfos)
        {
            download(videoInfos);
            convert();
            cleanup();
        }

        private void download(IEnumerable<VideoInfo> videoInfos)
        {
            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 480);

            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            m_mp4File = System.IO.Path.Combine("C:/temp", video.Title + video.VideoExtension);

            var videoDownloader = new VideoDownloader(video, m_mp4File);

            videoDownloader.Execute();
        }

        private void convert()
        {
            try
            {
                VideoFileReader reader = new VideoFileReader();
                reader.Open(m_mp4File);
                Bitmap videoFrame = null;
                int frame = 0;
                int scratchfile = 0;
                while ((videoFrame = reader.ReadVideoFrame()) != null)
                {
                    if (frame % reader.FrameRate == 0)
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

        private void cleanup()
        {
            if (System.IO.File.Exists(m_mp4File))
            {
                System.IO.File.Delete(m_mp4File);
            }
        }

        // Helper Methods

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
    }
}
