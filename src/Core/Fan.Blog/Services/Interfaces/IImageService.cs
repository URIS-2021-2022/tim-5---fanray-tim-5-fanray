﻿using Fan.Blog.Enums;
using Fan.Medias;
using System.IO;
using System.Threading.Tasks;

namespace Fan.Blog.Services.Interfaces
{
    /// <summary>
    /// The interface for blog image service.
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Deletes an image by id.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        Task DeleteAsync(int mediaId);

        /// <summary>
        /// Returns absolute URL to an image.
        /// </summary>
        /// <param name="media">The media record representing the image.</param>
        /// <param name="size">The image size.</param>
        /// <returns></returns>
        string GetAbsoluteUrl(Media media, EImageSize size);

        /// <summary>
        /// Uploads image.
        /// </summary>
        /// <param name="source">File stream.</param>
        /// <param name="userId">User who uploads the file.</param>
        /// <param name="fileName">Original filename.</param>
        /// <param name="contentType">File content type e.g. "image/jpeg".</param>
        /// <param name="uploadFrom">Whether the image is uploaded from browser or OLW.</param>
        /// <returns>
        /// <see cref="Media"/> that represents the image.
        /// </returns>
        Task<Media> UploadAsync(Stream source, int userId, string fileName, string contentType, EUploadedFrom uploadFrom);

        /// <summary>
        /// Given a blog post's body html, it replaces all img tags with one that is updated for Repsonsive Images.
        /// </summary>
        /// <param name="body">A blog post's body html.</param>
        /// <returns></returns>
        Task<string> ProcessResponsiveImageAsync(string body);
    }
}
