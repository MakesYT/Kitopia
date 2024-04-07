using System;
using System.Linq;
using Microsoft.Build.Construction;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
[GitHubActions(
    "continuous",
    GitHubActionsImage.WindowsLatest,
    On = new[] { GitHubActionsTrigger.Push },
    
    InvokedTargets = new[] { nameof(Compile) })]
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
    Project AvaloniaProject => Solution.GetProject("KitopiaAvalonia");
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            Log.Debug( "Restoring solution {0}", Solution);
            Log.Debug("Restoring project {0}", AvaloniaProject);
            DotNetRestore(c => new DotNetRestoreSettings()
               .SetProjectFile(AvaloniaProject.Path));


        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(c => new DotNetBuildSettings()
               .SetProjectFile(AvaloniaProject.Path));
        });

}
