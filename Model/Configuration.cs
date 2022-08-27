using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dogpixels_viewer
{
    [Serializable]
    public class Configuration
    {
        /// <summary>
        /// Configuration file name.
        /// </summary>
        public string ProfileName { get; set; }

        /// <summary>
        /// Configuration file version.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Root directory of your gallery to scan.
        /// </summary>
        public string RootDirectory { get; set; }

        /// <summary>
        /// Username as used on an image board to be included in the User Agent, as it may be requested by API guidelines.
        /// </summary>
        public string ImageBoardAccount { get; set; }

        /// <summary>
        /// Image board API uri to fetch data from. Include {md5} in the string to be replaced by the corresponding file md5.
        /// </summary>
        public string ImageBoardApi { get; set; }

        /// <summary>
        /// Attempt to store files in EXIF meta data, if applicable (e.g. JPEG)
        /// </summary>
        public bool StoreTagsInFiles { get; set; }

        /// <summary>
        /// Timer interval (in seconds) for rate limited API calls.
        /// </summary>
        public int ApiCallInterval { get; set; }

        public Configuration(string profilename)
        {
            ProfileName = profilename;
            Version = 1;
            ApiCallInterval = 2;
        }
    }
}
