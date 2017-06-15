﻿namespace ProjectHeleus.MangaService.Parsers
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;

    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Dom.Html;
    using AngleSharp.Extensions;
    using AngleSharp.Parser.Html;
    using Microsoft.Extensions.Logging;

    using Interfaces;
    using Shared.Types;
    using Shared.Models;
    using Shared.Models.Interfaces;

    public class ReadMangaParser 
        : IParser
    {
        #region Private Members

        private readonly ILogger<ReadMangaParser> _logger;

        #endregion

        public ReadMangaParser(ILogger<ReadMangaParser> logger)
        {
            _logger = logger;
        }

        #region Implementation of IParser

        public virtual string Url { get; set; } = "http://readmanga.me";

        #endregion

        #region Implementation of ICatalogParser
        public async Task<IEnumerable<ISort>> GetCatalogSorts()
        {
            var sorts = new List<SortModel>
            {
                new SortModel() {Id = SortType.Update.ToString().ToLower(), Title = "Дата обновления"},
                new SortModel() {Id = SortType.Rating.ToString().ToLower(), Title = "Рейтинг"},
                new SortModel() {Id = SortType.Popular.ToString().ToLower(), Title = "Популярность"},
                new SortModel() {Id = SortType.New.ToString().ToLower(), Title = "Новинки"},
            };

            return await Task.FromResult(sorts);
        }
        public async Task<IEnumerable<IManga>> GetAllFromCatalogAsync(SortType sortType, int page)
        {
            try
            {
                switch (sortType)
                {
                    case SortType.New:
                        return await GetCatalogContentAsync($"{Url}/list?sortType=created", page);
                    case SortType.Popular:
                        return await GetCatalogContentAsync($"{Url}/list?sortType=rate", page);
                    case SortType.Rating:
                        return await GetCatalogContentAsync($"{Url}/list?sortType=votes", page);
                    case SortType.Update:
                        return await GetCatalogContentAsync($"{Url}/list?sortType=updated", page);
                    default:
                        throw new NotSupportedException();
                }
            }
            finally
            {
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private async Task<IEnumerable<IManga>> GetCatalogContentAsync(string url, int page)
        {
            var parser = new HtmlParser();

            #region Build URL

            if (page > 0)
                url = $"{url}&offset={70 * page}&max=70";

            #endregion

            var formattedMangas = new List<MangaPreviewModel>();

            try
            {
                using (var client = new WebClient())
                {

                    client.Encoding = Encoding.UTF8;
                    
                    using (var htmlDocument = parser.Parse(await client.DownloadStringTaskAsync(url)))
                    {
                        var htmlMangas = htmlDocument.QuerySelectorAll(".tile.col-sm-6");

                        if (!htmlMangas.Any())
                        {
                            _logger.LogInformation($"There were no content for catalog: {url}");
                            return null;
                        }

                        foreach (var htmlManga in htmlMangas)
                        {
                            #region Header

                            var formattedManga = new MangaPreviewModel
                            {
                                Id = htmlManga.QuerySelector(".img a")?.GetAttribute("href").Replace("/", ""),
                                Title =
                                    htmlManga.QuerySelector("h4")?
                                        .TextContent.Replace("\n", "")
                                        .TrimStart(' ')
                                        .TrimEnd(' '),
                                Url = $@"{Url}{htmlManga.QuerySelector(".img a")?.GetAttribute("href")}",
                                Cover = htmlManga.QuerySelector(".img a img")?.GetAttribute("src")
                            };

                            #endregion

                            #region Rating

                            var htmlRatings = htmlManga.QuerySelector(".desc .rating")?.GetAttribute("title").Split(' ');

                            if (htmlRatings != null)
                            {
                                formattedManga.Rating = float.Parse(htmlRatings[0]);
                                formattedManga.RatingLimit = float.Parse(htmlRatings[2]);
                            }

                            #endregion

                            formattedMangas.Add(formattedManga);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot get calatog content from: {url}");
                _logger.LogError(e.Message);

                throw new HttpRequestException(e.Message);
            }
            finally
            {
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            return formattedMangas;
        }

        #endregion

        #region Implementation of IMangaParser

        public async Task<IManga> GetMangaAsync(string url)
        {
            var parser = new HtmlParser();

            #region Build URL

            url = $"{Url}/{url}";

            #endregion

            var parsedManga = new MangaModel();

            try
            {
                using (var client = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    using (var htmlDocument = parser.Parse(await client.DownloadStringTaskAsync(url)))
                    {
                        var htmlManga = htmlDocument.QuerySelector(".leftContent");

                        if (htmlManga == null)
                            return null;

                        GetInformation(parsedManga, htmlManga);

                        var information = htmlManga.QuerySelectorAll(".subject-meta.col-sm-7 p");

                        if (information.Any())
                        {
                            GetVolume(information, parsedManga);
                            GetViews(information, parsedManga);

                            foreach (var informationLine in information.Skip(2))
                            {
                                if (GetGenres(informationLine, parsedManga))
                                    continue;

                                if (GetAuthors(informationLine, parsedManga))
                                    continue;

                                if (GetTranslators(informationLine, parsedManga))
                                    continue;

                                GetPublishedYear(informationLine, parsedManga);
                            }
                        }

                        GetChapters(htmlManga, parsedManga);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot get manga content from: {url}");
                _logger.LogError(e.Message);

                throw new HttpRequestException(e.Message);
            }
            finally
            {
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            return parsedManga;
        }
        public virtual async Task<IChapterImages> GetMangaChapterAsync(string url)
        {
            var parser = new HtmlParser();

            #region Build URL

            url = $"{Url}/{url}";

            #endregion

            try
            {
                using (var client = new WebClient())
                {
                    using (var htmlDocument = parser.Parse(await client.DownloadStringTaskAsync(url)))
                    {

                        var htmlChapterImages =
                            htmlDocument.QuerySelectorAll("script")
                                .FirstOrDefault(x => x.TextContent.Contains("rm_h.init"));
                        if (htmlChapterImages == null)
                            return null;

                        string formattedImagesSource;

                        #region Preparing images source

                        var jsImagesSource =
                            htmlChapterImages.TextContent.Substring(htmlChapterImages.TextContent.IndexOf("rm_h.init"));

                        var preFormattedImagesSource = jsImagesSource.Substring(jsImagesSource.IndexOf("(")).TrimEnd();

                        var startBracket = preFormattedImagesSource.IndexOf("[");
                        var endBracket = preFormattedImagesSource.LastIndexOf("]");

                        formattedImagesSource = preFormattedImagesSource.Substring(startBracket + 1,
                            endBracket - startBracket - 1);

                        #endregion

                        if (string.IsNullOrEmpty(formattedImagesSource))
                        {
                            _logger.LogInformation(
                                $"Cannot retrive images for chapter: Url - {url}; jsImagesSource: {jsImagesSource}; preFormattedImagesSource: {preFormattedImagesSource};");
                            return null;
                        }

                        var localImagesHrefs = new List<string>();

                        foreach (Match match in new Regex(@"\[(.*?)\]").Matches(formattedImagesSource))
                        {
                            var image = match.Groups[1].Value;

                            if (string.IsNullOrEmpty(image))
                                continue;

                            var imageAttributes = image.Split(',');
                            localImagesHrefs.Add(
                                $@"{imageAttributes[1].Replace("\'", "")}{imageAttributes[0].Replace("\'", "")}{imageAttributes
                                    [2].Replace("\"", "")}");
                        }

                        return new ChapterImagesModel() {Images = localImagesHrefs};
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot get manga chapter content from: {url}");
                _logger.LogError(e.Message);

                throw new HttpRequestException(e.Message);
            }
            finally
            {
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private void GetInformation(MangaModel formattedManga, IElement mangaRaw)
        {
            formattedManga.Id = mangaRaw.QuerySelector("meta[itemprop=url]")?.GetAttribute("content").Replace(Url + @"/", "");
            formattedManga.Name = mangaRaw.QuerySelector(".names .name")?.TextContent;
            formattedManga.AlternateNames = new[]
            {
                mangaRaw.QuerySelector(".names .eng-name")?.TextContent,
                mangaRaw.QuerySelector(".names .original-name")?.TextContent
            }.Where(x => x != null);
            formattedManga.Description = mangaRaw.QuerySelector("meta[itemprop=description]")?.GetAttribute("content");
            formattedManga.Covers =
                mangaRaw.QuerySelectorAll(".picture-fotorama img")?.Select(x => x.GetAttribute("data-full"));
            formattedManga.Rating =
                float.Parse(mangaRaw.QuerySelector(".user-rating meta[itemprop=ratingValue]")?.GetAttribute("content"));
        }
        private void GetVolume(IHtmlCollection<IElement> information, MangaModel formattedManga)
        {
            var volumeLineBeforeIndex = information[0].TextContent.IndexOf(":", StringComparison.Ordinal);
            var volumeLineAfterIndex = information[0].TextContent.IndexOf(",", StringComparison.Ordinal);

            formattedManga.Volumes =
                volumeLineAfterIndex > 0
                    ? information[0].TextContent.Substring(volumeLineBeforeIndex + 1,
                        volumeLineAfterIndex - volumeLineBeforeIndex - 1)
                    : information[0].TextContent.Substring(volumeLineBeforeIndex + 1);
            formattedManga.Volumes = formattedManga.Volumes.Trim();
        }
        private void GetViews(IHtmlCollection<IElement> information, MangaModel formattedManga)
        {
            if (information.Length > 6)
            {
                var viewsIndex = information[2].TextContent.IndexOf(":", StringComparison.Ordinal);

                if (viewsIndex > 0)
                    formattedManga.Views = int.Parse(information[2].TextContent.Substring(viewsIndex + 1));
            }
            else
                formattedManga.Views = 0;
        }
        private bool GetGenres(IElement informationLine, MangaModel formattedManga)
        {
            if (informationLine.QuerySelector(".elem_genre") != null)
            {
                formattedManga.Genres =
                    informationLine.QuerySelectorAll(".elem_genre a")
                        .Select(x => new GenreModel() { Id = x.GetAttribute("href").Substring(x.GetAttribute("href").LastIndexOf('/') + 1), Title = x.TextContent, Url = $@"{Url}{x.GetAttribute("href")}" });
                return true;
            }
            return false;
        }
        private bool GetAuthors(IElement informationLine, MangaModel formattedManga)
        {
            if (informationLine.QuerySelector(".elem_author") != null)
            {
                formattedManga.Authors =
                    informationLine.QuerySelectorAll(".elem_author a")
                        .Select(x => new AuthorModel() { Id = x.GetAttribute("href")?.Substring(x.GetAttribute("href").LastIndexOf('/') + 1), Name = x.TextContent, Url = $@"{Url}{x.GetAttribute("href")}" });
                return true;
            }
            return false;
        }
        private bool GetTranslators(IElement informationLine, MangaModel formattedManga)
        {
            if (informationLine.QuerySelector(".elem_translator") != null)
            {
                formattedManga.Translators =
                    informationLine.QuerySelectorAll(".elem_translator a")
                        .Where(x => !string.IsNullOrEmpty(x.TextContent))
                        .Select(x => new TranslatorModel() { Id = x.GetAttribute("href").Substring(x.GetAttribute("href").LastIndexOf('/') + 1), Name = x.TextContent, Url = $@"{Url}{x.GetAttribute("href")}" });
                return true;
            }
            return false;
        }
        private void GetPublishedYear(IElement informationLine, MangaModel formattedManga)
        {
            if (informationLine.QuerySelector(".elem_year") != null)
                formattedManga.Published = int.Parse(informationLine.QuerySelector(".elem_year a")?.TextContent);
        }
        private void GetChapters(IElement mangaRaw, MangaModel formattedManga)
        {
            var formattedChapters = new List<ChapterModel>();
            var htmlChapters = mangaRaw.QuerySelectorAll(".expandable.chapters-link tbody tr");

            foreach (var chapter in htmlChapters)
            {
                var chapterInfo = chapter.QuerySelectorAll("td");

                if (chapterInfo.Any())
                {
                    formattedChapters.Add(new ChapterModel()
                    {
                        Id = chapterInfo[0].QuerySelector("a")?.GetAttribute("href").Substring(1),
                        Name = Regex.Replace(chapterInfo[0].QuerySelector("a")?.TextContent, @"\s{2,}", " ").TrimStart().TrimEnd(),
                        Url = $@"{Url}{chapterInfo[0].QuerySelector("a")?.GetAttribute("href")}",
                        Date = chapterInfo[1].TextContent.Replace("\n", "").Replace(" ", "")
                    });
                }
            }

            formattedManga.Chapters = formattedChapters;
        }

        #endregion

        #region Implementation of IGenreParser

        public async Task<IEnumerable<IGenre>> GetAllGenresAsync()
        {
            var parser = new HtmlParser();

            try
            {
                using (var client = new WebClient())
                {
                    using (
                        var htmlDocument =
                            parser.Parse(await client.DownloadStringTaskAsync($"{Url}/list/genres/sort_name")))
                    {
                        return
                            htmlDocument.QuerySelectorAll(".table.table-hover tbody tr td a")?
                                .Select(
                                    x =>
                                        new GenreModel()
                                        {
                                            Id =
                                                x.GetAttribute("href")?
                                                    .TrimEnd('/')
                                                    .Substring(x.GetAttribute("href").TrimEnd('/').LastIndexOf("/") + 1),
                                            Title = x.TextContent,
                                            Url = $"{Url}{x.GetAttribute("href")}"
                                        });
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot get genres content from: {Url}/list/genres/sort_name");
                _logger.LogError(e.Message);

                throw new HttpRequestException(e.Message);
            }
        }
        public async Task<IEnumerable<IManga>> GetAllFromGenreGenreAsync(SortType sortType, string url, int page)
        {
            return await GetCatalogContentAsync(GetGenreContentUrl(sortType, url, page), 0);
        }

        private string GetGenreContentUrl(SortType sortType, string url, int page)
        {
            string formattedUrl = $"{Url}/list/genre/{url}{{0}}{{1}}";;

            switch (sortType)
            {
                case SortType.New:
                   formattedUrl = string.Format(formattedUrl, "?sortType=created", page > 0 ? $"{url}&offset={70 * page}&max=70" : "");
                    break;
                case SortType.Popular:
                    formattedUrl = string.Format(formattedUrl, "?sortType=rate", page > 0 ? $"{url}&offset={70 * page}&max=70" : "");
                    break;
                case SortType.Rating:
                    formattedUrl = string.Format(formattedUrl, "?sortType=votes", page > 0 ? $"{url}&offset={70 * page}&max=70" : "");
                    break;
                case SortType.Update:
                    formattedUrl = string.Format(formattedUrl, "?sortType=updated", page > 0 ? $"{url}&offset={70 * page}&max=70" : "");
                    break;
            }

            return formattedUrl;
        }

        #endregion
    }
}