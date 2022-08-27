using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dogpixels_viewer.Model
{
    /// <summary>
    /// The following class is used to parse an API response.
    /// Sources: https://e621.net/help/api#posts
    /// Example: https://e621.net/posts.json?md5=e3b5bc2ab9a7765d55edbab5b5dc67ce&limit=1
    /// Note: The following property names must match the named structure of an API response as seen in the example linked above.
    /// </summary>
    public class ApiResponse
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool? Success { get; set; }
        public string? Reason { get; set; }
        public string? Location { get; set; }
        public DanbooruApiPostData? Post { get; set; }
        public DanbooruApiPostData[]? Posts { get; set; }

        internal string getAllTags()
        {
            List<string> ret = new List<string>();

            List<DanbooruApiPostData> posts = new List<DanbooruApiPostData>();

            if (Posts != null)
                posts.AddRange(Posts);

            if (Post != null)
                posts.Add(Post);

            try
            {
                // search ApiResponse.Tags for tags
                foreach (DanbooruApiPostData post in posts)
                {
                    foreach (PropertyInfo prop in post.Tags.GetType().GetProperties())
                    {
                        if ((Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType) == typeof(string[]))
                        {
                            ret.AddRange((string[])prop.GetValue(post.Tags, null));
                        }
                        else
                        {
                            log.Warn($"Encountered a non string array property in model ApiResponse.Post.Tags and didn't know how to handle it. Property type: '{prop.PropertyType}'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("General error while converting ApiResponse.Post.Tags to string[]:");
                log.Error(ex);
            }

            return string.Join(" ", ret);
        }
    }

    public class DanbooruApiPostData
    {
        public int? Id { get; set; }
        public string? Created_at { get; set; }
        public string? Updated_at { get; set; }
        public ApiFile? File { get; set; }
        public ApiPreview? Preview { get; set; }
        public ApiTags? Tags { get; set; }
    }

    public class ApiFile
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Url { get; set; }
    }
    public class ApiPreview
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Url { get; set; }
    }
    public class ApiTags
    {
        public string[]? General { get; set; }
        public string[]? Species { get; set; }
        public string[]? Character { get; set; }
        public string[]? Copyright { get; set; }
        public string[]? Artist { get; set; }
        public string[]? Invalid { get; set; }
        public string[]? Lore { get; set; }
        public string[]? Meta { get; set; }
    }
}
