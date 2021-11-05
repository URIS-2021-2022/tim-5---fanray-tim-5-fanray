﻿using HtmlAgilityPack;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TimeZoneConverter;

namespace Fan.Helpers
{
    /// <summary>
    /// Utility helpers.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Returns the slug of a given string. 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="randomCharCountOnEmpty">
        /// The result slug could be empty if given title is non-english such as chinese, turn this 
        /// to false will return a random string of the specified number of chars instead of empty string.
        /// </param>
        /// <remarks>
        /// Produces optional, URL-friendly version of a title, "like-this-one",
        /// hand-tuned for speed, reflects performance refactoring contributed by John Gietzen (user otac0n) 
        /// http://stackoverflow.com/questions/25259/how-does-stackoverflow-generate-its-seo-friendly-urls
        /// </remarks>
        public static string Slugify(string title, int maxlen = 250, int randomCharCountOnEmpty = 0)
        {
            if (title == null)
                return randomCharCountOnEmpty <= 0 ? "" : Util.RandomString(randomCharCountOnEmpty);

            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (int i = 0; i < len; i++)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase            
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' || c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    { sb.Append('-'); prevdash = true; }
                }
                else if ((int)c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxlen - 1) break;
            }

            var slug = prevdash ? sb.ToString().Substring(0, sb.Length - 1) : sb.ToString();
            if (slug == string.Empty && randomCharCountOnEmpty > 0) slug = Util.RandomString(randomCharCountOnEmpty);

            return slug;
        }

        /// <summary>
        /// Returns a new slug by appending a counter to the given slug.  This method is called 
        /// when caller already determined the slug is a duplicate.
        /// </summary>
        /// <param name="slug">The current slug that runs into conflict.</param>
        /// <param name="i">The counter on what slug should be appended with.</param>
        /// <returns></returns>
        public static string UniquefySlug(string slug, IEnumerable<string> existingSlugs)
        {
            if (slug.IsNullOrEmpty() || existingSlugs.IsNullOrEmpty()) return slug;

            int i = 2;
            while (existingSlugs.Contains(slug))
            {
                var lookup = $"-{i}";
                if (slug.EndsWith(lookup))
                {
                    var idx = slug.LastIndexOf(lookup);
                    slug = slug.Remove(idx, lookup.Length).Insert(idx, $"-{++i}");
                }
                else
                {
                    slug = $"{slug}-{i}";
                }
            }

            return slug;
        }

        /// <summary>
        /// Returns a new slug by appending a counter to the given slug.
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string UniquefySlug(string slug, ref int i)
        {
            if (slug.IsNullOrEmpty()) return slug;

            var lookup = $"-{i}";
            if (slug.EndsWith(lookup))
            {
                var idx = slug.LastIndexOf(lookup);
                slug = slug.Remove(idx, lookup.Length).Insert(idx, $"-{++i}");
            }
            else
            {
                slug = $"{slug}-{i}";
            }

            return slug;
        }

        /// <summary>
        /// Returns an ascii char for an international char.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://meta.stackexchange.com/questions/7435/non-us-ascii-characters-dropped-from-full-profile-url/7696#7696
        /// </remarks>
        public static string RemapInternationalCharToAscii(char c)
        {
            var s = c.ToString().ToLowerInvariant();

        var mappings = new Dictionary<string, string>
        {
            { "a", "àåáâäãåą" },
            { "c", "çćčĉ" },
            { "d", "đ" },
            { "e", "èéêëę" },
            { "g", "ğĝ" },
            { "h", "ĥ" },
            { "i", "ìíîïı" },
            { "j", "ĵ" },
            { "l", "ł" },
            { "n", "ñń" },
            { "o", "òóôõöøőð" },
            { "r", "ř" },
            { "s", "śşšŝ" },
            { "ss", "ß" },
            { "th", "Þ" },
            { "u", "ùúûüŭů" },
            { "y", "ýÿ" },
            { "z", "żźž" }
        };

        foreach(var mapping in mappings)
        {
            if (mapping.Value.Contains(s))
                return mapping.Key;
        }

        return string.Empty;
        }

        /// <summary>
        /// Returns a random lowercase alpha + numeric chars of a certain length.
        /// </summary>
        /// <param name="length"></param>
        /// <remarks>
        /// https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings-in-c
        /// https://stackoverflow.com/a/9995910/32240
        /// https://stackoverflow.com/a/1344242/32240
        /// </remarks>
        public static string RandomString(int length)
        {
            Random random = new Random();
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";
            var chars = Enumerable.Repeat(0, length) // Range would give the same end result
                                   .Select(x => input[random.Next(0, input.Length)]);
            return new string(chars.ToArray());
        }

        /// <summary>
        /// Returns excerpt give body of a post. Returns empty string if body is null or operation
        /// fails. The returned string 
        /// </summary>
        /// <param name="body"></param>
        /// <param name="wordsLimit"></param>
        /// <returns></returns>
        /// <remarks>
        /// - I noticed flipboard on the web uses cleaned up exerpts
        /// - Stripping all html tags with Html Agility Pack http://stackoverflow.com/a/3140991/32240
        /// </remarks>
        public static string GetExcerpt(string body, int wordsLimit)
        {
            if (string.IsNullOrEmpty(body) || wordsLimit <= 0) return "";

            try
            {
                // decode body
                body = WebUtility.HtmlDecode(body); 

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(body);
                body = document.DocumentNode.InnerText?.Trim(); // should be clean text by now
                if (body.IsNullOrEmpty()) return "";

                return body.Truncate(wordsLimit, Truncator.FixedNumberOfWords);
            }
            catch (Exception)
            {
                return body;
            }
        }

        /// <summary>
        /// Removes all html tags from content.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string CleanHtml(string content)
        {
            if (content.IsNullOrEmpty()) return content;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(content);
            return document.DocumentNode.InnerText;
        }

        /// <summary>
        /// Returns true if running from a test project.
        /// </summary>
        /// <returns></returns>
        public static bool IsRunningFromTestHost() =>
            Assembly.GetEntryAssembly().FullName.StartsWith("testhost", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// A util that uses regular expression to verify if a string is in valid email format.
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
        /// </remarks>
        public static bool IsValidEmail(string strIn)
        {
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
                if (string.IsNullOrEmpty(strIn))
                    return false;
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            // Return true if strIn is in valid email format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                return null;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}
