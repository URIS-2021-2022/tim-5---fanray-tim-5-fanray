﻿using Fan.WebApp;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Web.Tests
{
    public class BlogRoutesTest : IClassFixture<FanWebApplicationFactory<Startup>>
    {
        private readonly FanWebApplicationFactory<Startup> factory;

        public BlogRoutesTest(FanWebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// The blog theme is implemented with MVC as such all routes should return success status
        /// code 200 - 299.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO the preview links are not working due to TempData needs to get a hold of the post
        /// to be previewed, this is done in composer, need to figure out a way to simulate that.
        /// </remarks>
        [Theory]
        [InlineData("/")]
        [InlineData("/blog")]
        [InlineData("/rsd")]
        [InlineData("/blog/post/1")]
        [InlineData("/post/2017/01/01/test-post")]
        [InlineData("/blog/technology")]
        [InlineData("/posts/tagged/aspnet")]
        [InlineData("/posts/2017/01")]
        [InlineData("/feed")]
        [InlineData("/blog/technology/feed")]
        [InlineData("/about")]
        [InlineData("/about/ray")]
        //[InlineData("/preview/post/2017/01/01/test-post")]
        //[InlineData("/preview/page/about")]
        //[InlineData("/preview/page/about/ray")]
        public async Task All_blog_routes_should_return_suscess_status_code(string url)
        {
            // Arrange: a http client
            var client = factory.CreateClient();

            // Act: hit a url
            var response = await client.GetAsync(url);

            // Assert: the status code should be 200-299
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}
