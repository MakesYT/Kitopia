using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Build.Construction;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Project = Nuke.Common.ProjectModel.Project;

[GitHubActions(
    "continuous",
    GitHubActionsImage.WindowsLatest,
    On = new[] { GitHubActionsTrigger.Push },
    ImportSecrets = new[] { nameof(GitHubToken) },
    InvokedTargets = new[] { nameof(Clean) })]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);
    [Solution]
    readonly Solution Solution;
    
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [Parameter][Secret]readonly string GitHubToken;
    Project AvaloniaProject => Solution.GetProject("KitopiaAvalonia");
    

    Target Restore => _ => _
        .Executes(() =>
        {
            Log.Debug( "Restoring solution {0}", Solution);
            Log.Debug("Restoring project {0}", AvaloniaProject);
           
            DotNetRestore(c => new DotNetRestoreSettings()
               .SetProjectFile(AvaloniaProject.Path));
            DotNetRestore(c => new DotNetRestoreSettings()
               .SetProjectFile("KitopiaEx"));

        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            // DotNetBuild(c =>
            // {
            //     return new DotNetBuildSettings()
            //           .SetProjectFile(AvaloniaProject.Path)
            //           .SetOutputDirectory(RootDirectory / "output");
            // });
        });

    
    
   

    Target Pack => _ => _
                       .OnlyWhenDynamic(() =>
                        {
                            var gitRepository = GitRepository.FromUrl("https://github.com/MakesYT/Kitopia");
                            var result = GitHubTasks.GetLatestRelease(gitRepository,true).Result;
                            Log.Debug("Packing project {0}", AvaloniaProject);
                            Log.Debug("GitHubName {0}",  gitRepository.GetGitHubName());
                            var _gitHubClient = new GitHubClient(new ProductHeaderValue("Kitopia"))
                            {
                                Credentials = new Credentials(GitHubToken)
                            };
                            var readOnlyList = _gitHubClient.Repository.GetAllTags(gitRepository.GetGitHubOwner(), gitRepository.GetGitHubName()).Result;
                            if (readOnlyList.Any(e=>e.Name==AvaloniaProject.GetProperty("Version")))
                            {
                                    return false;
                            }
                            return true;
                        })
        .DependsOn(Compile)
        .Executes(() =>
                            {
                                var rootDirectory = RootDirectory / "Publish";
                                rootDirectory.DeleteDirectory();
                                DotNetPublish(c => new DotNetPublishSettings()
                                                  .SetProject("KitopiaEx")
                                                  .SetOutput(RootDirectory / "Publish" / "plugins" / "KitopiaEx")
                                                  .SetRuntime( "win-x64")
                                                  .SetFramework("net8.0")
                                                  .SetConfiguration("Release")
                                                  .SetSelfContained(false)
                                );
                                DotNetPublish(c => new DotNetPublishSettings()
                                                  .SetProject(AvaloniaProject.Name)
                                                  .SetOutput(RootDirectory / "Publish")
                                                  .SetPublishSingleFile(true)
                                                  
                                                  .SetRuntime( "win-x64")
                                                  .SetFramework("net8.0-windows10.0.17763.0")
                                                  .SetConfiguration("Release")
                                                  .SetSelfContained(false)
                                                  );
                                
                                var gitRepository = GitRepository.FromUrl("https://github.com/MakesYT/Kitopia");
                                var result = GitHubTasks.GetLatestRelease(gitRepository,true).Result;
                                Log.Debug("Packing project {0}", AvaloniaProject);
                                Log.Debug("GitHubName {0}",  gitRepository.GetGitHubName());
                                var _gitHubClient = new GitHubClient(new ProductHeaderValue("Kitopia"))
                                {
                                    Credentials = new Credentials(GitHubToken)
                                };
                                foreach (var absolutePath in rootDirectory.GetFiles())
                                {
                                    if (absolutePath.Extension is ".pdb" or ".xml")
                                    {
                                        absolutePath.DeleteFile();
                                    }
                                }

                                var archiveFile = RootDirectory / "Kitopia"+AvaloniaProject.GetProperty("Version")+".zip";
                                archiveFile.DeleteFile();
                                rootDirectory.ZipTo(archiveFile);
                                var newRelease = new NewRelease(AvaloniaProject.GetProperty("Version"))
                                {
                                    Name = "Kitopia",
                                    Prerelease = true,
                                    Draft = true,
                                    
                                };

                                var release = _gitHubClient.Repository.Release.Create(gitRepository.GetGitHubOwner(), gitRepository.GetGitHubName(),
                                    newRelease).Result;
                                using var artifactStream = File.OpenRead(archiveFile);
                                var fileName = Path.GetFileName( "Kitopia"+AvaloniaProject.GetProperty("Version")+".zip");
                                var assetUpload = new ReleaseAssetUpload
                                {
                                    FileName = fileName,
                                    ContentType =  "application/octet-stream",
                                    RawData = artifactStream,
                                };
                                _gitHubClient.Repository.Release.UploadAsset(release,assetUpload).Wait();
                                Log.Debug(result);
                            }
                        );
    Target Clean => _ => _
                        .DependsOn(Pack)
                        .Executes(() =>
                         {
                             
                         });
}
