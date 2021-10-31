﻿using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BlogTags.Manage.Widgets
{
    public class BlogTagsSettingsModel : PageModel
    {
        protected readonly IWidgetService widgetService;
        public BlogTagsSettingsModel(IWidgetService widgetService)
        {
            this.widgetService = widgetService;
        }

        public string WidgetJson { get; set; }

        public async Task OnGet(int widgetId)
        {
            var widget = (BlogTagsWidget)await widgetService.GetExtensionAsync(widgetId);
            WidgetJson = JsonConvert.SerializeObject(widget);
        }

        public async Task<IActionResult> OnPostAsync([FromBody]BlogTagsWidget widget)
        {
            if (ModelState.IsValid)
            {
                await widgetService.UpdateWidgetAsync(widget.Id, widget);
                return new JsonResult("Widget settings updated.");
            }

            return BadRequest("Invalid form values submitted.");
        }
    }
}