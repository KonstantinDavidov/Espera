﻿using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espera.Core
{
    public class SoundCloudSongFinder : INetworkSongFinder<SoundCloudSong>
    {
        private const string ClientId = "0367b2f7000481e0d1e0815e70c81379";

        public async Task<IReadOnlyList<SoundCloudSong>> GetSongsAsync(string searchTerm)
        {
            IReadOnlyList<SoundCloudSong> songs;

            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    var api = RestService.For<ISoundCloudApi>("http://api-v2.soundcloud.com");

                    songs = (await api.GetPopularTracks(50)).Tracks;
                }

                else
                {
                    var api = RestService.For<ISoundCloudApi>("http://api.soundcloud.com");

                    songs = await api.Search(searchTerm, ClientId);
                }
            }

            catch (Exception ex)
            {
                throw new NetworkSongFinderException("SoundCloud search failed", ex);
            }

            songs = songs.Where(x => x.IsStreamable || x.IsDownloadable).ToList();

            foreach (SoundCloudSong song in songs)
            {
                if (song.IsStreamable)
                {
                    song.StreamUrl = new Uri(song.StreamUrl + "?client_id=" + ClientId);
                }

                if (song.IsDownloadable)
                {
                    song.DownloadUrl = new Uri(song.DownloadUrl + "?client_id=" + ClientId);
                }
            }

            return songs;
        }
    }
}