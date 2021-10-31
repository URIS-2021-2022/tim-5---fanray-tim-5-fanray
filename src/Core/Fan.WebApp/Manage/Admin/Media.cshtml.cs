﻿using Fan.Blog.Enums;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Medias;
using Fan.Membership;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Admin
{
    public class MediaModel : PageModel
    {
        private readonly IBlogPostService _blogSvc;
        private readonly IMediaService _mediaSvc;
        private readonly IImageService _imgSvc;
        private readonly UserManager<User> _userManager;

        public MediaModel(
            IBlogPostService blogSvc,
            IMediaService mediaSvc,
            IImageService imgService,
            UserManager<User> userManager)
        {
            _blogSvc = blogSvc;
            _mediaSvc = mediaSvc;
            _imgSvc = imgService;
            _userManager = userManager;
        }

        // -------------------------------------------------------------------- Helper Classes

        public class ImageVM : Media
        {
            public string FileType { get; set; }
            public string UploadDate { get; set; }
            public string UploadVia { get; set; }
            public string UploadBy { get; set; }
            /// <summary>
            /// The gallery image dialog shows small image as thumbs.
            /// </summary>
            public string UrlSmall { get; set; }
            /// <summary>
            /// The composer inserts medium image.
            /// </summary>
            public string UrlMedium { get; set; }
            /// <summary>
            /// The gallery image dialog preview shows the large image.
            /// </summary>
            public string UrlLarge { get; set; }
            /// <summary>
            /// The gallery image dialog sidebar shows the original url.
            /// </summary>
            public string UrlOriginal { get; set; }

            public bool Selected { get; set; }
        }

        public class ImageData
        {
            public IEnumerable<ImageVM> Images { get; set; }
            public IEnumerable<string> ErrorMessages { get; set; }
            public string ImagesJson =>
                (Images == null || Images.Count() <= 0) ? "[]" :
                JsonConvert.SerializeObject(Images);
        }

        // -------------------------------------------------------------------- consts & properties

        /// <summary>
        /// Display 96 images at a time.
        /// </summary>
        public const int PAGE_SIZE = 96;

        /// <summary>
        /// Total number of images.
        /// </summary>
        public int ImageCount { get; set; }

        /// <summary>
        /// The json data to bootstrap page initially.
        /// </summary>
        public ImageData Data { get; private set; }

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// GET bootstrap page with json data.
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            var (medias, count) = await GetImageVMsAsync(1);
            Data = new ImageData
            {
                Images = medias,
            };
            ImageCount = count;
        }

        /// <summary>
        /// Ajax GET to return first page of images in editor mode.
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> OnGetImagesAsync()
        {
            var (medias, count) = await GetImageVMsAsync(1);
            return new JsonResult(new { medias, count });
        }

        /// <summary>
        /// Ajax GET when clicks on Show More button to get more medias by page number.
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public async Task<JsonResult> OnGetMoreAsync(int pageNumber = 1)
        {
            var (medias, count) = await GetImageVMsAsync(pageNumber);
            return new JsonResult(medias);
        }

        /// <summary>
        /// Uploads images and returns urls to optimized or original if optimized is not available.
        /// </summary>
        /// <param name="images"></param>
        public async Task<JsonResult> OnPostImageAsync(IList<IFormFile> images)
        {
            var userId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User));
            var imageVMs = new List<ImageVM>();
            var errMsgs = new List<string>();

            foreach (var image in images)
            {
                try
                {
                    using (Stream stream = image.OpenReadStream())
                    {
                        var media = await _imgSvc.UploadAsync(stream, userId, image.FileName, image.ContentType, EUploadedFrom.Browser);
                        imageVMs.Add(await MapImageVMAsync(media));
                    }
                }
                catch (NotSupportedException ex)
                {
                    errMsgs.Add(ex.Message);
                }
                catch (FanException ex) // todo consider errcode
                {
                    errMsgs.Add(ex.Message);
                }
            }

            var imageData = new ImageData
            {
                Images = imageVMs,
                ErrorMessages = errMsgs.Distinct(),
            };

            return new JsonResult(imageData);
        }

        /// <summary>
        /// POST to delete images by their ids.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JsonResult> OnPostDeleteAsync([FromBody]int[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                await _imgSvc.DeleteAsync(ids[i]);
            }
            return new JsonResult(true);
        }

        /// <summary>
        /// POST to update an image.
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public async Task<JsonResult> OnPostUpdateAsync([FromBody]ImageVM media)
        {
            await _mediaSvc.UpdateMediaAsync(media.Id, media.Title, media.Caption, media.Alt, media.Description);
            return new JsonResult(true);
        }

        // -------------------------------------------------------------------- private

        /// <summary>
        /// Returns 
        /// </summary>
        /// <remarks>
        /// TODO check each media AppType to decide which GetImageUrl to call
        /// </remarks>
        private async Task<(IEnumerable<ImageVM> medias, int count)> GetImageVMsAsync(int pageNumber)
        {
            var (medias, count) = await _mediaSvc.GetMediasAsync(EMediaType.Image, pageNumber, PAGE_SIZE);
            List<ImageVM> imageVMs = new List<ImageVM>();
            foreach (var media in medias)
            {
                imageVMs.Add(await MapImageVMAsync(media));
            }

            return (medias: imageVMs, count: count);
        }

        /// <summary>
        /// Maps an Media object to ImageVM.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private async Task<ImageVM> MapImageVMAsync(Media m)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return new ImageVM
            {
                Id = m.Id,
                FileName = m.FileName,
                Title = m.Title,
                Caption = m.Caption,
                Alt = m.Alt,
                FileType = m.ContentType,
                UploadDate = m.UploadedOn.ToString("yyyy-MM-dd"),
                UploadVia = m.UploadedFrom.ToString(),
                UploadBy = user.DisplayName,
                Width = m.Width,
                Height = m.Height,
                UrlSmall = _imgSvc.GetAbsoluteUrl(m, EImageSize.Small),
                UrlMedium = _imgSvc.GetAbsoluteUrl(m, EImageSize.Medium),
                UrlLarge = _imgSvc.GetAbsoluteUrl(m, EImageSize.Large),
                UrlOriginal = _imgSvc.GetAbsoluteUrl(m, EImageSize.Original),
            };
        }
    }
}