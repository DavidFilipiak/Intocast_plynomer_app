using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Velopack;
using Velopack.Sources;
using System.Windows;

namespace IntocastGasMeterApp.services
{
    internal static class UpdateService
    {
        public static async Task CheckForUpdatesAsync()
        {
            // Use your GitHub owner & repo here
            //var source = new GithubSource("your-github-username", "your-repo", prerelease: false);
            var mgr = new UpdateManager("https://pub-3669bb5d525c491baf9ebceff1857194.r2.dev/");

            var update = await mgr.CheckForUpdatesAsync();
            Console.WriteLine("Checked for updates.");
            Console.WriteLine(update);
            if (update is null) return;

            var newVersion = update.TargetFullRelease.Version;
            string newVersionNotes = update.TargetFullRelease.NotesMarkdown;
            long newVersionSize = update.TargetFullRelease.Size;
            double newVersionSizeMB = Math.Round((double)newVersionSize / 1024 / 1024, 2);

            if (MessageBox.Show(
                    $"Nová verzia {newVersion.ToString()} je dostupná ({newVersionSize} MB).\n\n{newVersionNotes}\n\nChcete ju stiahnuť teraz?",
                    "Dostupný update", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                await mgr.DownloadUpdatesAsync(update);
                mgr.ApplyUpdatesAndRestart(update); // app restarts into the new version
            }
        }
    }
}
