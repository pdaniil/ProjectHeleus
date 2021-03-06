﻿namespace ProjectHeleus.MangaService.Parsers
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Shared.Models.Interfaces;

    public partial class MintMangaParser 
        : ReadMangaParser
    {
        #region Hides of IParser

        public override string Url { get; set; } = "http://mintmanga.com";

        #endregion

        public MintMangaParser(ILogger<ReadMangaParser> logger) 
            : base(logger)
        {

        }
    }
}