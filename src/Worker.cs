using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TerminalBackgroundRotator
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private string _filename;
        private string _originalFilename;
        private string _profileGuid;
        private string _wallpaperDirectory;
        private int _wallpaperIntervalInSeconds = 300;

        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _filename = Environment.ExpandEnvironmentVariables(_config.GetValue<string>("Terminal:ProfilePath"));
            _profileGuid = _config.GetValue<string>("Terminal:ProfileGuid");
            _wallpaperDirectory = Environment.ExpandEnvironmentVariables(_config.GetValue<string>("Terminal:WallpaperDirectory"));
            _wallpaperIntervalInSeconds = _config.GetValue<int>("Terminal:WallpaperIntervalInSeconds", _wallpaperIntervalInSeconds);

            // Backup terminal profile
            var timestamp = DateTime.Now.ToString("yyyyMMddhhmmss");
            _originalFilename = $"{_filename}.bak{timestamp}";
            File.Copy(_filename, _originalFilename);

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            // Restore original terminal profile
            File.Copy(_originalFilename, _filename, overwrite: true);

            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string backgroundImage = null;

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    // Read the terminal profiles
                    var json = File.ReadAllText(_filename);

                    // Get a background image from the provided location
                    var images = Directory.EnumerateFiles(_wallpaperDirectory);

                    if (backgroundImage == null)
                    {
                        backgroundImage = images.FirstOrDefault();
                    }
                    else
                    {
                        // Find the next background image after the current one
                        bool found = false;
                        string nextImage = null;

                        foreach (string image in images)
                        {
                            if (found)
                            {
                                nextImage = image;
                                break;
                            }

                            if (image == backgroundImage)
                            {
                                found = true;
                            }
                        }

                        backgroundImage = nextImage ?? images.FirstOrDefault() ?? backgroundImage;
                    }

                    // Replace the background image for the given profile
                    dynamic jobject = JsonConvert.DeserializeObject(json);
                    var profile = ((IEnumerable<dynamic>)jobject.profiles["list"]).Where(d => d.guid == _profileGuid).SingleOrDefault();

                    if (profile != null)
                    {
                        profile.backgroundImage = backgroundImage;
                    }

                    // Write the profiles back out
                    var output = JsonConvert.SerializeObject(jobject, Formatting.Indented);
                    File.WriteAllText(_filename, output);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "An exception was caught!");
                }

                await Task.Delay(_wallpaperIntervalInSeconds * 1000, stoppingToken);
            }
        }
    }
}
