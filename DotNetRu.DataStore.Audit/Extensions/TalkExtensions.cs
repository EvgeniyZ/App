﻿namespace DotNetRu.DataStore.Audit.Extensions
{
    using System.Linq;

    using DotNetRu.DataStore.Audit.Models;
    using DotNetRu.DataStore.Audit.RealmModels;

    public static class TalkExtensions
    {
        public static TalkModel ToModel(this Talk talkEntity)
        {
            return new TalkModel
            {
                TalkId = talkEntity.Id,
                Title = talkEntity.Title,
                Abstract = talkEntity.Description,
                PresentationUrl = talkEntity.SlidesUrl,
                VideoUrl = talkEntity.VideoUrl,
                CodeUrl = talkEntity.CodeUrl,
                ShortTitle = talkEntity.Title,
                Speakers = talkEntity.Speakers.Select(x => x.ToModel()),

                // Sessions = talkEntity.Session.Select(s => s.ToModel())
            };
        }
    }
}
