﻿namespace ProjectHeleus.MangaApp.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Windows.System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;
    using WindowsLibrary.Extenstions;
    using Caliburn.Micro;
    using MangaLibrary.Providers.Interfaces;
    using Microsoft.Toolkit.Uwp.UI.Animations;
    using Microsoft.Toolkit.Uwp.UI.Controls;
    using Models;
    using Shared.Models;

    public class DetailPageViewModel : Screen
    {
        #region Private Members

        private readonly ICatalogsProvider _catalogsProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDetailProvider _detailProvider;

        private Tuple<MangaPreviewModel, CatalogModel> _parameter;
        private MangaModel _mangaModel;
        private bool _isMangaLoaded;
        private ChapterModel _selectedChapter;
        private IEnumerable<ImageModel> _chapterImages;
        private double _width;

        #endregion

        public Tuple<MangaPreviewModel, CatalogModel> Parameter
        {
            get { return _parameter; }
            set
            {
                _parameter = value;
                GetMangaContent();
            }
        }

        public bool IsMangaLoaded
        {
            get { return _isMangaLoaded; }
            set
            {
                _isMangaLoaded = value; 
                NotifyOfPropertyChange();
            }
        }

        public DetailPageViewModel(ICatalogsProvider catalogsProvider, IEventAggregator eventAggregator, IDetailProvider detailProvider)
        {
            _catalogsProvider = catalogsProvider;
            _eventAggregator = eventAggregator;
            _detailProvider = detailProvider;

            Initialize();
        }

        private void Initialize()
        {
            _eventAggregator.Subscribe(this);
        }


        public string Title => _mangaModel?.Name;
        public string Titles => _mangaModel == null ? string.Empty : string.Join(", ", _mangaModel?.AlternateNames);

        public IEnumerable<StringModel> CoverModels => _mangaModel?.Covers.Select(x => new StringModel() { Value = x }).ToBindableCollection();

        
        public string Authors => _mangaModel == null ? string.Empty : string.Join(", ", _mangaModel.Authors.Select(x => (x as AuthorModel).Name).Where(x=>!string.IsNullOrEmpty(x)));
        public bool AuthorsVisibility => !string.IsNullOrEmpty(Authors);

        public string Translators => _mangaModel?.Translators == null ? string.Empty : string.Join(", ", _mangaModel.Translators.Select(x => (x as TranslatorModel).Name).Where(x=>!string.IsNullOrEmpty(x)));
        public bool TranslatorsVisibility => !string.IsNullOrEmpty(Translators);

        public string Genres => _mangaModel?.Genres == null ? string.Empty : string.Join(", ", _mangaModel.Genres.Select(x => (x as GenreModel).Title).Where(x=>!string.IsNullOrEmpty(x)));
        public bool GenresVisibility => !string.IsNullOrEmpty(Genres);

        public float Rating => _mangaModel?.Rating ?? 0.0F;
        public int RatingLimit => Convert.ToInt32(_mangaModel?.RatingLimit);

        public string Description => _mangaModel?.Description;

        public IEnumerable<ChapterModel> Chapters => _mangaModel?.Chapters.Cast<ChapterModel>();

        public ChapterModel SelectedChapter
        {
            get { return _selectedChapter; }
            set
            {
                _selectedChapter = value;

                if (value != null)
                {
                    LoadImages();
                }

                NotifyOfPropertyChange();
            }
        }

        public IEnumerable<ImageModel> ChapterImages
        {
            get { return _chapterImages; }
            set
            {
                _chapterImages = value;
                NotifyOfPropertyChange();
            }
        }

        private async void GetMangaContent()
        {
            IsMangaLoaded = false;


            var manga = await _detailProvider.GetMangaContent(Parameter.Item2, Parameter.Item1);

            if (manga.HasResponse)
            {
                _mangaModel = manga.Value;

                NotifyOfPropertyChange(nameof(Title));
                NotifyOfPropertyChange(nameof(Titles));

                NotifyOfPropertyChange(nameof(CoverModels));

                NotifyOfPropertyChange(nameof(Authors));
                NotifyOfPropertyChange(nameof(AuthorsVisibility));

                NotifyOfPropertyChange(nameof(Translators));
                NotifyOfPropertyChange(nameof(TranslatorsVisibility));

                NotifyOfPropertyChange(nameof(Genres));
                NotifyOfPropertyChange(nameof(GenresVisibility));

                NotifyOfPropertyChange(nameof(Rating));
                NotifyOfPropertyChange(nameof(RatingLimit));

                NotifyOfPropertyChange(nameof(Description));

                NotifyOfPropertyChange(nameof(Chapters));
            }



            IsMangaLoaded = true;
        }

        private async void LoadImages()
        {
            var images = await _detailProvider.GetMangaChapterContent(Parameter.Item2, _mangaModel, SelectedChapter);
            if (images.HasResponse)
                ChapterImages = images.Value.Images.Select(x => new ImageModel() { Url = x });
        }

        private void OnImageOpened(Image source, ScrollViewer obj)
        {
            if (source.ActualHeight > obj.ActualHeight * 2)
            {
                source.Width = 500;
            }
            else
            {
                obj.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                obj.VerticalScrollMode = ScrollMode.Disabled;
            }
        }
    }
}