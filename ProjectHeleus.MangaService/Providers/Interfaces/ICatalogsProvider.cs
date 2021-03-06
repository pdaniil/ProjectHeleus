﻿namespace ProjectHeleus.MangaService.Providers.Interfaces
{
    using System.Threading.Tasks;
    using System.Collections.Generic;

    using Shared.Types;
    using Shared.Models.Interfaces;

    public interface ICatalogsProvider
    {
        Task<IEnumerable<ICatalog>> GetCatalogsAsync();

        Task<IEnumerable<ISort>> GetCatalogSorts(CatalogType catalogType);
        Task<IEnumerable<IManga>> GetAllFromCatalogAsync(CatalogType catalog, SortType sort, int page);
    }
}