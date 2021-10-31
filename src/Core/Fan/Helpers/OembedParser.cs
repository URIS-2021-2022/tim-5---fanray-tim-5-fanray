﻿using HtmlAgilityPack;
using System;
using System.Web;

namespace Fan.Helpers
{
    /// <summary>
    /// Parses oembeds in a post body and replace with html to display the embeds.
    /// </summary>
    public class OembedParser
    {
        /// <summary>
        /// Given a post body of html, parses any oembeds in it and returns new body with proper embed html.
        /// </summary>
        /// <param name="body">A post body.</param>
        /// <returns></returns>
        public static string Parse(string body)
        {
            if (body.IsNullOrEmpty()) return body;

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(body);

                var nodes = doc.DocumentNode.SelectNodes("//oembed");
                if (nodes == null || nodes.Count <= 0) return body;

                bool changed = false;
                foreach (var node in nodes)
                {
                    var url = node.Attributes["url"]?.Value; if (url.IsNullOrEmpty()) continue;
                    var type = GetOembedType(url); if (type == EEmbedType.Unknown) continue;

                    if (type == EEmbedType.YouTube)
                    {
                        var embHtml = GetYouTubeEmbed(url);
                        var newNode = HtmlNode.CreateNode(embHtml);
                        node.ParentNode.ReplaceChild(newNode, node);
                        changed = true;
                    }

                    if (type == EEmbedType.Vimeo)
                    {
                        var embHtml = GetVimeoEmbed(url);
                        var newNode = HtmlNode.CreateNode(embHtml);
                        node.ParentNode.ReplaceChild(newNode, node);
                        changed = true;
                    }
                }

                return changed ? doc.DocumentNode.OuterHtml : body;
            }
            catch (Exception)
            {
                return body;
            }
        }

        /// <summary>
        /// Returns proper embed html to display for a YouTube video
        /// </summary>
        /// <param name="url">
        /// https://youtu.be/MNor4dYXa6U or https://www.youtube.com/watch?v=MNor4dYXa6U
        /// </param>
        /// <remarks>
        /// https://developers.google.com/youtube/youtube_player_demo
        /// </remarks>
        /// <returns></returns>
        public static string GetYouTubeEmbed(string url)
        {
            var key = GetYouTubeVideoKey(url);
            var urlEmbed = $"https://www.youtube.com/embed/{key}";

            var widthSeg = "width=\"800\"";
            var heightSeg = "height=\"450\"";
            if (urlEmbed.ToLower().Contains("&amp;w=") || urlEmbed.ToLower().Contains("&amp;start=") ||
                urlEmbed.ToLower().Contains("&w=") || urlEmbed.ToLower().Contains("&start=")) //  height is optional
            {
                urlEmbed = urlEmbed.Replace("&amp;", "&");
                string queryStr = new Uri(urlEmbed).PathAndQuery;
                var dict = HttpUtility.ParseQueryString(queryStr);

                // get all possible query params
                Int32.TryParse(dict["w"], out int width);
                Int32.TryParse(dict["h"], out int height);
                Int32.TryParse(dict["start"], out int start);

                // get rid of query string
                urlEmbed = urlEmbed.Substring(0, urlEmbed.IndexOf("&"));

                widthSeg = width > 0 ? $"width=\"{width}\"" : widthSeg;
                heightSeg = height > 0 ? $"height=\"{height}\"" : heightSeg;

                if (start > 0)
                {
                    urlEmbed += "?start=" + start;
                }
            }

            return $"<iframe {widthSeg} {heightSeg} src =\"{urlEmbed}\" frameborder =\"0\" allow=\"autoplay; encrypted - media\" allowfullscreen></iframe>";
        }

        /// <summary>
        /// Returns embed html for an vimeo video.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetVimeoEmbed(string url)
        {
            const string VIMEO_URL_SEG = "vimeo.com/";

            var key = url.Substring(url.LastIndexOf(VIMEO_URL_SEG) + VIMEO_URL_SEG.Length);
            var urlEmbed = $"https://player.vimeo.com/video/{key}";

            var widthSeg = "width=\"800\"";
            var heightSeg = "height=\"450\"";
            return $"<iframe {widthSeg} {heightSeg} src =\"{urlEmbed}\" frameborder =\"0\" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>";
        }

        /// <summary>
        /// Returns the type of url whether it's youtube, twitter or other.
        /// </summary>
        /// <returns></returns>
        public static EEmbedType GetOembedType(string url)
        {
            if (url.Contains("youtu.be/") || url.Contains("youtube.com/")) return EEmbedType.YouTube;
            if (url.Contains("vimeo.com/")) return EEmbedType.Vimeo;
            //if (url.Contains("twitter.com/")) return EEmbedType.Twitter;

            return EEmbedType.Unknown;
        }

        /// <summary>
        /// Returns the portion of the url that starts with video key. Returns empty string if url
        /// is not a qualified youtube video url.
        /// </summary>
        /// <param name="url">A full youtube video url.</param>
        /// <returns></returns>
        public static string GetYouTubeVideoKey(string url)
        {
            const string SHORTENED_URL = "youtu.be/";
            const string NAVIGATION_URL = "youtube.com/watch?v=";
            const string EMBED_URL = "youtube.com/embed/";

            string segment = "";
            if (url.Contains(SHORTENED_URL)) segment = SHORTENED_URL;
            else if (url.Contains(NAVIGATION_URL)) segment = NAVIGATION_URL;
            else if (url.Contains(EMBED_URL)) segment = EMBED_URL;
            if (segment.IsNullOrEmpty()) return "";

            string key = url.Substring(url.LastIndexOf(segment) + segment.Length);
            // the "t" param does not work, it has to change to "start"
            if (key.Contains("&t=")) key = key.Replace("&t=", "&start=");
            else if (key.Contains("&amp;t=")) key = key.Replace("&amp;t=", "&amp;start=");

            return key;
        }
    }

    /// <summary>
    /// The different embeds CkEditor 5 provides.
    /// </summary>
    /// <remarks>
    /// https://ckeditor.com/docs/ckeditor5/latest/features/media-embed.html
    /// </remarks>
    public enum EEmbedType
    {
        Unknown,
        YouTube,
        Vimeo,
        Twitter,
        Instagram,
        Spotify,
        GoogleMap,
    }
}
