using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SuperMediaSuites.Services;

/// <summary>
/// Optional CD metadata lookup against the MusicBrainz web service. Computes the
/// MusicBrainz disc ID from a CD table of contents and queries the server for artist/album/track
/// titles. Network use is confined to this class; nothing is sent but the disc id.
/// </summary>
public sealed class MusicBrainzClient : IDisposable
{
    private static readonly HttpClient Http = new HttpClient();
    private readonly bool _ownsClient;

    static MusicBrainzClient()
    {
        // MusicBrainz asks for a contactable User-Agent.
        Http.DefaultRequestHeaders.UserAgent.ParseAdd("SuperMediaSuites/1.0 (https://github.com/uponatime2019/SuperMediaSuites)");
        Http.Timeout = TimeSpan.FromSeconds(15);
    }

    public MusicBrainzClient()
    {
        _ownsClient = false; // use the static, shared client
    }

    /// <summary>Compute the MusicBrainz disc ID for a CD (first/last track + sector offsets incl. lead-out).</summary>
    public static string ComputeDiscId(int firstTrack, int lastTrack, int leadOutSector, IReadOnlyList<int> trackOffsets)
    {
        var sb = new StringBuilder();
        sb.Append(firstTrack.ToString("X2", CultureInfo.InvariantCulture));
        sb.Append(lastTrack.ToString("X2", CultureInfo.InvariantCulture));

        var offsets = new int[100];
        for (var i = 0; i < trackOffsets.Count; i++)
        {
            offsets[i] = trackOffsets[i];
        }
        offsets[lastTrack - firstTrack + 1] = leadOutSector;

        for (var i = 0; i < 100; i++)
        {
            sb.Append(offsets[i].ToString("X8", CultureInfo.InvariantCulture));
        }

        var hash = SHA1.HashData(Encoding.ASCII.GetBytes(sb.ToString()));
        return Convert.ToBase64String(hash).Replace('+', '.').Replace('/', '_').Replace('=', '-');
    }

    /// <summary>Look up release metadata for the disc ID; returns null on any failure.</summary>
    public async Task<CdMetadata?> LookupAsync(string discId, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://musicbrainz.org/ws/2/discid/{Uri.EscapeDataString(discId)}?inc=recordings+artist-credits+releases&fmt=json";
            using var response = await Http.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
            var root = doc.RootElement;

            if (!root.TryGetProperty("releases", out var releases) || releases.GetArrayLength() == 0)
            {
                return null;
            }

            var release = releases[0];
            var artist = release.TryGetProperty("artist-credit", out var credit) && credit.GetArrayLength() > 0
                ? credit[0].GetProperty("name").GetString() ?? "Unknown Artist"
                : "Unknown Artist";
            var album = release.GetProperty("title").GetString() ?? "Unknown Album";

            var tracks = new Dictionary<int, string>();
            if (release.TryGetProperty("media", out var media) && media.GetArrayLength() > 0)
            {
                var disc = media[0];
                if (disc.TryGetProperty("tracks", out var trackList))
                {
                    var index = 1;
                    foreach (var t in trackList.EnumerateArray())
                    {
                        if (t.TryGetProperty("title", out var title))
                        {
                            tracks[index] = title.GetString() ?? $"Track {index:D2}";
                        }
                        index++;
                    }
                }
            }

            return new CdMetadata(artist, album, tracks);
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (_ownsClient) Http.Dispose();
    }
}

/// <summary>Resolved CD metadata (artist/album + per-track titles by 1-based number).</summary>
public sealed record CdMetadata(string Artist, string Album, IReadOnlyDictionary<int, string> TrackTitles);
