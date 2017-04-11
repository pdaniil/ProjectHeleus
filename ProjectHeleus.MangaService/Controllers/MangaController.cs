﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectHeleus.MangaService.Extensions;
using ProjectHeleus.MangaService.Models.Interfaces;
using ProjectHeleus.MangaService.Providers.Contracts;

namespace ProjectHeleus.MangaService.Controllers
{
    public class MangaController : Controller
    {
        #region Private Members

        private readonly IMangaProvider _mangaProvider;

        #endregion

        public MangaController(IMangaProvider mangaProvider)
        {
            _mangaProvider = mangaProvider;
        }

        [Route("api/[controller]/{catalog}/{manga}")]
        public async Task<IManga> GetMangaContent(string catalog, string manga)
        {
            return await _mangaProvider.GetMangaContentAsync(catalog.GetCatalogType(), manga);
        }


        [Route("api/[controller]/{catalog}/{manga}/{volume}/{chapter}")]
        public async Task<IChapterImages> GetMangaChapterContent(string catalog, string manga, string volume, string chapter)
        {
            return await _mangaProvider.GetMangaChapterContentAsync(catalog.GetCatalogType(), $"/{manga}/{volume}/{chapter}");
        }


        [Route("api/[controller]/{catalog}/{manga}/{volume}/{chapter}/{page}")]
        public async Task<IChapterImages> GetMangaChapterContent(string catalog, string manga, string volume, string chapter, string page)
        {
            return await _mangaProvider.GetMangaChapterContentAsync(catalog.GetCatalogType(), $"/{manga}/{volume}/{chapter}/{page}");
        }
    }
}