﻿namespace ProjectHeleus.MangaService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using Extensions;
    using Shared.Types;
    using Providers.Interfaces;
    using Shared.Models.Interfaces;

    public class CatalogsController 
        : Controller
    {
        #region Private Members

        private readonly ICatalogsProvider _catalogsProvider;

        #endregion

        public CatalogsController(ICatalogsProvider catalogsProvider)
        {
            _catalogsProvider = catalogsProvider;
        }

        [Route("api/[controller]")]
        public async Task<IEnumerable<ICatalog>> GetAllCatalogs()
        {
            return await _catalogsProvider.GetCatalogsAsync();
        }

        [Route("api/[controller]/{catalog}")]
        public async Task<IEnumerable<ISort>> GetCatalogContent(string catalog)
        {
            return await _catalogsProvider.GetCatalogSorts(catalog.GetCatalogType());
        }

        [Route("api/[controller]/{catalog}/{page:int}")]
        public async Task<IEnumerable<IManga>> GetCatalogContent(string catalog, int page)
        {
            return await GetCatalogContent(catalog, SortType.Popular.ToString(), page);
        }

        [Route("api/[controller]/{catalog}/{sort}")]
        public async Task<IEnumerable<IManga>> GetCatalogContent(string catalog, string sort)
        {
            return await GetCatalogContent(catalog, sort, 0);
        }

        [Route("api/[controller]/{catalog}/{sort}/{page:int}")]
        public async Task<IEnumerable<IManga>> GetCatalogContent(string catalog, string sort, int page)
        {
            var sortType = (SortType)Enum.Parse(typeof(SortType), sort, true);
            var catalogType = catalog.GetCatalogType();

            return await _catalogsProvider.GetAllFromCatalogAsync(catalogType, sortType, page);
        }
    }
}
