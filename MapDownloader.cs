using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MapPostprocessor
{
    public class MapDownloader
    {
        private HttpClient httpClient = new();
        private string _mapsDirectory = "/home/maps";
        private bool _deleteSong = false;

        public MapDownloader(string mapsDirectory, bool deleteSong = false)
        {
            _mapsDirectory = mapsDirectory;
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; BeatSaverDownloader/1.0)");
            _deleteSong = deleteSong;
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

        public async Task<string?> Map(string hash, string? downloadUrl = null, bool skipDownload = false)
        {
            string lowerCaseDir = Path.Combine(_mapsDirectory, hash.ToLower());
            if (Directory.Exists(lowerCaseDir) && (File.Exists(Path.Combine(lowerCaseDir, "info.dat")) || File.Exists(Path.Combine(lowerCaseDir, "Info.dat"))))
            {
                return lowerCaseDir;
            }

            string mapDir = Path.Combine(_mapsDirectory, hash.ToUpper());

            if (Directory.Exists(mapDir) && (File.Exists(Path.Combine(mapDir, "info.dat")) || File.Exists(Path.Combine(mapDir, "Info.dat"))))
            {
                return mapDir;
            }

            if (skipDownload) {
                return null;
            }

            string? downloadURL = downloadUrl;

            if (downloadURL == null) {
                await Task.Delay(500); // Be nice to the API

                string beatsaverUrl = $"https://beatsaver.com/api/maps/hash/{hash}";
                JsonNode? beatsaverData = null;
            
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
            }

            if (string.IsNullOrEmpty(downloadURL))
            {
                return null;
            }

            var data = await httpClient.GetByteArrayAsync(downloadURL);

            using var zipStream = new MemoryStream(data);
            using var zipArchive = new ZipArchive(zipStream);
            Directory.CreateDirectory(mapDir);
            zipArchive.ExtractToDirectory(mapDir, true);

            if (_deleteSong) {
                foreach (var item in Directory.EnumerateFiles(mapDir)) {
                    if (item.EndsWith(".egg") || item.EndsWith(".ogg") || item.EndsWith(".wav")) {
                        File.Delete(item);
                    }
                }
            }
            

            return mapDir;
        }
    }
}
