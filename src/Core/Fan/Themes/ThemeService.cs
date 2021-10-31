﻿using Fan.Data;
using Fan.Exceptions;
using Fan.Extensibility;
using Fan.Widgets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MoreLinq.Extensions.DistinctByExtension;

namespace Fan.Themes
{
    /// <summary>
    /// The theme service.
    /// </summary>
    public class ThemeService : ExtensibleService<ThemeManifest, Theme>, IThemeService
    {
        /// <summary>
        /// The manifest file name for themes "theme.json".
        /// </summary>
        public const string THEME_MANIFEST = "theme.json";
        /// <summary>
        /// The directory that contains themes "Themes".
        /// </summary>
        public const string THEME_DIR = "Themes";

        private const string CACHE_KEY_INSTALLED_THEMES_MANIFESTS = "installed-theme-manifests";
        private TimeSpan Cache_Time_Installed_Theme_Manifests = new TimeSpan(0, 10, 0);

        public ThemeService(IWebHostEnvironment hostingEnvironment,
            IDistributedCache distributedCache,
            IMetaRepository metaRepository,
            ILogger<ThemeService> logger)
            : base(metaRepository, distributedCache, hostingEnvironment, logger)
        {
        }

        public override string ManifestName { get; } = THEME_MANIFEST;
        public override string ManifestDirectory { get; } = THEME_DIR;

        /// <summary>
        /// Activates a theme.
        /// </summary>
        /// <param name="folderName">Theme's folder name.</param>
        /// <returns></returns>
        /// <remarks>
        /// It registers theme and the widget areas used by the theme.
        /// </remarks>
        public async Task ActivateThemeAsync(string folderName)
        {
            // verify folderName 
            if (!IsValidExtensionFolder(folderName))
                throw new FanException($"Theme {folderName} contains invalid characters.");

            // register theme if not exist
            folderName = folderName.ToLower(); // lower case
            if (await metaRepository.GetAsync(folderName, EMetaType.Theme) == null)
            {
                await metaRepository.CreateAsync(new Meta
                {
                    Key = folderName, 
                    Value = "", // empty for now
                    Type = EMetaType.Theme,
                });
            }

            // register theme-defined widget areas
            var installedThemes = await GetManifestsAsync();
            var themeToActivate = installedThemes.Single(t => t.Folder.Equals(folderName, StringComparison.OrdinalIgnoreCase));

            // check if there is any empty area ids
            if (themeToActivate.WidgetAreas.Any(a => a.Id.IsNullOrEmpty()))
                throw new FanException("Widget area id cannot be empty.");

            var themeDefinedAreas = themeToActivate.WidgetAreas.Where(ta => 
                                    !WidgetService.SystemDefinedWidgetAreaInfos.Any(sa => sa.Id == ta.Id));
            foreach (var area in themeDefinedAreas)
            {
                var key = string.Format($"{folderName}-{area.Id}").ToLower();

                // register only if not exist
                if (await metaRepository.GetAsync(key, EMetaType.WidgetAreaByTheme) == null)
                {
                    var widgetArea = new WidgetArea { Id = area.Id };
                    await metaRepository.CreateAsync(new Meta
                    {
                        Key = key,
                        Value = JsonConvert.SerializeObject(widgetArea),
                        Type = EMetaType.WidgetAreaByTheme,
                    });
                }
            }
        }

        /// <summary>
        /// Returns a list of <see cref="ThemeManifest"/> of the installed themes and their <see cref="WidgetAreaInfo"/>.
        /// </summary>
        /// <remarks>
        /// The ids of the widget area infos are distinct and lower case.
        /// </remarks>
        public override async Task<IEnumerable<ThemeManifest>> GetManifestsAsync()
        {
            return await distributedCache.GetAsync(CACHE_KEY_INSTALLED_THEMES_MANIFESTS, Cache_Time_Installed_Theme_Manifests, async () =>
            {
                var list = new List<ThemeManifest>();
                var themesDir = Path.Combine(hostingEnvironment.ContentRootPath, THEME_DIR);

                foreach (var dir in Directory.GetDirectories(themesDir))
                {
                    var folder = dir.Substring(dir.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                    // load only valid folder name
                    if (!IsValidExtensionFolder(folder)) continue;

                    var file = Path.Combine(dir, THEME_MANIFEST);
                    var themeManifest = JsonConvert.DeserializeObject<ThemeManifest>(await File.ReadAllTextAsync(file));
                    themeManifest.Folder = folder;

                    // make sure no duplicate areas based on id
                    themeManifest.WidgetAreas = themeManifest.WidgetAreas.DistinctBy(a => a.Id).ToArray();

                    // make sure all area ids are lower case
                    foreach (var area in themeManifest.WidgetAreas) area.Id = area.Id.ToLower();

                    list.Add(themeManifest);
                }

                return list;
            });
        }

        public static string GetLayoutName(EPageLayout layout) => $"_Page{layout}";
    }
}
