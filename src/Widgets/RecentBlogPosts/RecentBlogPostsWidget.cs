﻿using Fan.Widgets;
using System.ComponentModel.DataAnnotations;

namespace RecentBlogPosts
{
    public class RecentBlogPostsWidget : Widget
    {
        public const int MAX_RECENT_POSTS = 6;

        public RecentBlogPostsWidget()
        {
            Title = "Recent Posts";
        }

        /// <summary>
        /// Number of recent blog posts to display. Default 3, range must be between 1 and 6.
        /// </summary>
        [Range(1, MAX_RECENT_POSTS, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int NumberOfPostsToShow { get; set; } = 3;

        /// <summary>
        /// Whether to display post author.
        /// </summary>
        public bool ShowPostAuthor { get; set; } = true;

        /// <summary>
        /// Whether to display post date.
        /// </summary>
        public bool ShowPostDate { get; set; } = true;

        /// <summary>
        /// Whether to display post excerpt.
        /// </summary>
        public bool ShowPostExcerpt { get; set; } = true;
    }
}
