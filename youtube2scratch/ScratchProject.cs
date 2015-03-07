using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace youtube2scratch
{
    public class ScratchProject
    {

        private Dictionary<int, string> m_dictionary = null;
        private string m_directory = null;

        public ScratchProject(Dictionary<int, string> dict, string dir)
        {
            m_dictionary = dict;
            m_directory = dir;
        }

        public void createScratchJson()
        {
            

        }

        private string getCostumes()
        {
            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<int, string> pair in m_directory)
            {
                builder.Append("{\"costumeName\": \"youtube2Scratch").Append(pair.Key).Append("\",");
                builder.Append("\"baseLayerID\": ").Append(pair.Key).Append(",");
                builder.Append("\"baseLayerMD5\": \"").Append(pair.Value).Append("\",");
                builder.Append("\"bitmapResolution\": 2,");
                builder.Append("\"rotationCenterX\": 478,");
                builder.Append("\"rotationCenterY\": 268},");
            }
            builder.Length--;

            return builder.ToString();
        }

        private string getSounds(string md5, int sampleCount)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{\"soundName\": \"youtube2Scratch\"");
            builder.Append("\"soundID\": 0,");
            builder.Append("\"md5\": \"").Append(md5).Append(".wv\",");
            builder.Append("\"sampleCount\":").Append(sampleCount).Append(",");
            builder.Append("\"rate\": 22050,");
            builder.Append("\"format\": \"adpcm\"}");

            return builder.ToString();

        }

    }
}
