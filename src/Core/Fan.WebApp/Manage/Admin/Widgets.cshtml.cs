﻿using Fan.Widgets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Admin
{
    public class WidgetsModel : PageModel
    {
        private readonly IWidgetService widgetService;

        public WidgetsModel(IWidgetService widgetService)
        {
            this.widgetService = widgetService;
        }

        public string WidgetManifestsJson { get; private set; }
        public string WidgetAreasJson { get; private set; }
        public string WidgetAreaPanelsJson { get; private set; }

        public async Task OnGetAsync()
        {
            // manifests
            var widgetManifests = await widgetService.GetManifestsAsync();
            WidgetManifestsJson = JsonConvert.SerializeObject(widgetManifests);

            // areas
            var widgetAreas = await widgetService.GetCurrentThemeAreasAsync();
            WidgetAreasJson = JsonConvert.SerializeObject(widgetAreas);

            // areas panels
            var panels = new bool[widgetAreas.ToArray().Length];
            for (int i = 0; i < panels.Length; i++)
            {
                panels[i] = true;
            }
            WidgetAreaPanelsJson = JsonConvert.SerializeObject(panels);
        }

        /// <summary>
        /// When user drags a widget from info to an area or from an area to anther area.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAddAsync([FromBody]AddWidgetDto dto)
        {
            WidgetInstance widgetInst = null;

            // user drags a widget from infos to an area
            if (dto.AreaFromId.IsNullOrEmpty())
            {
                var widgetId = await widgetService.CreateWidgetAsync(dto.Folder);
                widgetInst = await widgetService.AddWidgetToAreaAsync(widgetId, dto.AreaToId, dto.Index);
            }
            else // user drags a widget from area to another
            {
                await widgetService.RemoveWidgetFromAreaAsync(dto.WidgetId, dto.AreaFromId);
                widgetInst = await widgetService.AddWidgetToAreaAsync(dto.WidgetId, dto.AreaToId, dto.Index);
            }

            return new JsonResult(widgetInst);
        }

        public async Task OnPostReorderAsync([FromBody]OrderWidgetDto dto) =>
            await widgetService.OrderWidgetInAreaAsync(dto.WidgetId, dto.AreaId, dto.Index);

        /// <summary>
        /// DELETE a widget instance from an area.
        /// </summary>
        /// <param name="widgetId"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public async Task OnDeleteAsync(int widgetId, string areaId)
        {
            await widgetService.RemoveWidgetFromAreaAsync(widgetId, areaId);
            await widgetService.DeleteWidgetAsync(widgetId);
        }
    }

    public class AddWidgetDto
    {
        /// <summary>
        /// The new index in the area to insert a widget.
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// The folder name of the widget user drags.
        /// </summary>
        public string Folder { get; set; }
        /// <summary>
        /// Id of the area user drags widget to.
        /// </summary>
        public string AreaToId { get; set; }
        /// <summary>
        /// Id of the widget user drags from an area, 0 if user drags widget from infos.
        /// </summary>
        public int WidgetId { get; set; }
        /// <summary>
        /// Id of the area user drags widget from, null if user drags widget from infos. 
        /// </summary>
        public string AreaFromId { get; set; }

        public string Name { get; set; }
        public string Title { get; set; }
    }

    public class OrderWidgetDto
    {
        /// <summary>
        /// The new index to insert the widget.
        /// </summary>
        public int Index { get; set; }
        public int WidgetId { get; set; }
        public string AreaId { get; set; }
    }
}