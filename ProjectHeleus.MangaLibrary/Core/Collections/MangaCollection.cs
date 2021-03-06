﻿namespace ProjectHeleus.MangaLibrary.Core.Collections
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    using Caliburn.Micro;
    using Microsoft.Toolkit.Uwp;

    using Messages;
    using Shared.Models;
    using Providers.Interfaces;

    public class MangaCollection 
        : IIncrementalSource<MangaPreviewModel>
    {
        #region Private Members

        private readonly ICatalogsProvider _catalogsProvider;
        private readonly IEventAggregator _eventAggregator;

        private CatalogModel _catalog;
        private SortModel _sort;
        private GenreModel _genre;

        #endregion

        public MangaCollection(ICatalogsProvider catalogsProvider, IEventAggregator eventAggregator)
        {
            _catalogsProvider = catalogsProvider;
            _eventAggregator = eventAggregator;
        }

        public void SetCatalog(CatalogModel catalog)
        {
            _catalog = catalog;
        }

        public void SetSort(SortModel sort)
        {
            _sort = sort;
        }

        public void SetGenre(GenreModel genre)
        {
            _genre = genre;
        }

        #region Implementation of IIncrementalSource<MangaModel>

        public async Task<IEnumerable<MangaPreviewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new CancellationToken())
        {
            _eventAggregator.PublishOnUIThread(new BeginIncrementalLoading());

            IEnumerable<MangaPreviewModel> fetchedResult = null;

            if (_genre is null)
            {
                if (_sort is null)
                    fetchedResult = (await _catalogsProvider.GetCatalogContent(_catalog, pageIndex)).Value;
                else
                    fetchedResult = (await _catalogsProvider.GetCatalogContent(_catalog, _sort, pageIndex)).Value;
            }
            else
            {
                if (_sort is null)
                    fetchedResult = (await _catalogsProvider.GetCatalogContent(_catalog, _genre, pageIndex)).Value;
                else
                    fetchedResult = (await _catalogsProvider.GetCatalogContent(_catalog, _genre, _sort, pageIndex)).Value;
            }

            _eventAggregator.PublishOnUIThread(new EndIncrementalLoading());

            return fetchedResult;
        }

        #endregion
    }
}