using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExtractor;
using NAudio;
using NAudio.Wave;

namespace youtube2scratch
{
    public class WAVexporter
    {
        private string m_mp3File = null;

        public void export(IEnumerable<VideoInfo> videoInfos)
        {
            download(videoInfos);
            convert();
            cleanup();
        }

        private void download(IEnumerable<VideoInfo> videoInfos)
        {
            VideoInfo video = videoInfos
                    .Where(info => info.CanExtractAudio)
                    .OrderByDescending(info => info.AudioBitrate)
                    .First();

            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            m_mp3File = System.IO.Path.Combine("C:/temp", video.Title + video.AudioExtension);

            var audioDownloader = new AudioDownloader(video, m_mp3File);
            audioDownloader.Execute();
        }

        private void convert()
        {
            using (Mp3FileReader reader = new Mp3FileReader(m_mp3File))
            {
                WaveFileWriter.CreateWaveFile("C:/temp/1.wav", reader);
            }
        }

        private void cleanup()
        {
            if(System.IO.File.Exists(m_mp3File))
            {
                System.IO.File.Delete(m_mp3File);
            }
        }
    }
}
