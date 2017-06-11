﻿namespace ProjectHeleus.Shared.Models
{
    using System.Collections.Generic;

    using Interfaces;

    public class MangaShortModel 
        : IManga
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Cover { get; set; }
        public int Views { get; set; }
        public float Rating { get; set; }
        public float RatingLimit { get; set; }
        public IEnumerable<AuthorModel> Authors { get; set; }
        public IEnumerable<GenreModel> Genres { get; set; }
    }
}