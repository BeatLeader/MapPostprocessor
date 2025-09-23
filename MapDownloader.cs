using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MapPostprocessor
{
    public class MapDownloader
    {
        private HttpClient httpClient = new();
        private string _mapsDirectory = "/home/maps";

        public MapDownloader(string mapsDirectory)
        {
            _mapsDirectory = mapsDirectory;
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; BeatSaverDownloader/1.0)");
        }

        public bool MapExists(string hash)
        {
            string lowerCaseDir = Path.Combine(_mapsDirectory, hash.ToLower());
            if (Directory.Exists(lowerCaseDir))
            {
                return true;
            }

            string mapDir = Path.Combine(_mapsDirectory, hash.ToUpper());

            if (Directory.Exists(mapDir))
            {
                return true;
            }

            return false;
        }

        public async Task<string?> Map(string hash)
        {
            string lowerCaseDir = Path.Combine(_mapsDirectory, hash.ToLower());
            if (Directory.Exists(lowerCaseDir))
            {
                return lowerCaseDir;
            }

            string mapDir = Path.Combine(_mapsDirectory, hash.ToUpper());

            if (Directory.Exists(mapDir))
            {
                return mapDir;
            }

            await Task.Delay(500); // Be nice to the API

            string beatsaverUrl = $"https://beatsaver.com/api/maps/hash/{hash}";
            JsonNode? beatsaverData = null;
            string? downloadURL = null;
            try {
                var response = await httpClient.GetStringAsync(beatsaverUrl);
                beatsaverData = response != null ? JsonSerializer.Deserialize<JsonNode>(response) : null;
                downloadURL = string.Empty;
            } catch (Exception e) {
                return null;
            }

            if (beatsaverData == null) {
                return null;
            }

            foreach (var version in beatsaverData["versions"]?.AsArray() ?? [])
            {
                if (version?["hash"]?.AsValue().ToString().ToLower() == hash.ToLower())
                {
                    downloadURL = version?["downloadURL"]?.AsValue().ToString() ?? "";
                    break;
                }
            }

            if (string.IsNullOrEmpty(downloadURL))
            {
                return null;
            }

            var data = await httpClient.GetByteArrayAsync(downloadURL);

            using var zipStream = new MemoryStream(data);
            using var zipArchive = new ZipArchive(zipStream);
            Directory.CreateDirectory(mapDir);
            zipArchive.ExtractToDirectory(mapDir);

            return mapDir;
        }
    }
}
