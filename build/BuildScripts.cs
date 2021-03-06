using System;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
public class BuildScripts : NukeBuild
{
	/*
	/// Support plugins are available for:
	///   - JetBrains ReSharper        https://nuke.build/resharper
	///   - JetBrains Rider            https://nuke.build/rider
	///   - Microsoft VisualStudio     https://nuke.build/visualstudio
	///   - Microsoft VSCode           https://nuke.build/vscode
	*/

	public static int Main() => Execute<BuildScripts>(x => x.Build);

	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
	readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

	[Solution] readonly Solution Solution;
	[GitRepository] readonly GitRepository GitRepository;
	[GitVersion] readonly GitVersion GitVersion;

	AbsolutePath SourceDirectory => RootDirectory / "src";
	AbsolutePath TestsDirectory => RootDirectory / "tests";
	AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
	AbsolutePath TestArtifactsDirectory => ArtifactsDirectory / "tests";
	AbsolutePath NugetDirectory => ArtifactsDirectory / "nuget";
	AbsolutePath CodeCoverageReportOutput => TestArtifactsDirectory / "Reports";
	AbsolutePath CodeCoverageFile => TestArtifactsDirectory / "coverage.cobertura.xml";

	AbsolutePath PerformanceProject => TestsDirectory / "Beffyman.UdpServer.Performance";
	AbsolutePath DemoClientProject => TestsDirectory / "Beffyman.UdpServer.Demo.Client";
	AbsolutePath DemoServerProject => TestsDirectory / "Beffyman.UdpServer.Demo";

	Target Clean => _ => _
		.Before(Restore)
		.Executes(() =>
		{
			SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
			TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
			EnsureCleanDirectory(ArtifactsDirectory);
		});

	Target Restore => _ => _
		.Executes(() =>
		{
			DotNetRestore(s => s
				.SetProjectFile(Solution));
		});

	Target Build => _ => _
		.DependsOn(Restore)
		.Executes(() =>
		{
			DotNetBuild(s => s
				.SetProjectFile(Solution)
				.SetConfiguration(Configuration)
				.SetAssemblyVersion(GitVersion.AssemblySemVer)
				.SetFileVersion(GitVersion.AssemblySemFileVer)
				.SetInformationalVersion(GitVersion.InformationalVersion)
				.AddProperty("TreatWarningsAsErrors", "true")
				.EnableNoRestore());
		});


	Target Pack => _ => _
		.DependsOn(Build)
		.Executes(() =>
		{

			DotNetPack(s => s.SetProject(Solution)
					.SetVersion(GitVersion.NuGetVersionV2)
					.EnableNoBuild()
					.EnableIncludeSource()
					.EnableIncludeSymbols()
					.SetConfiguration(Configuration)
					.SetAssemblyVersion(GitVersion.AssemblySemVer)
					.SetFileVersion(GitVersion.AssemblySemFileVer)
					.SetInformationalVersion(GitVersion.InformationalVersion)
					.SetOutputDirectory(NugetDirectory));
		});

	Target Test => _ => _
		.DependsOn(Build)
		.Executes(() =>
		{
			DotNetTest(s => s.EnableNoBuild()
				.SetConfiguration(Configuration)
				.EnableNoBuild()
				.EnableNoRestore()
				.SetLogger("trx")
				.SetResultsDirectory(TestArtifactsDirectory)
				.SetLogOutput(true)
				.SetArgumentConfigurator(arguments => arguments.Add("/p:CollectCoverage={0}", "true")
					.Add("/p:CoverletOutput={0}/", TestArtifactsDirectory)
					//.Add("/p:Threshold={0}", 90)
					.Add("/p:Exclude=\"[xunit*]*%2c[*.Tests]*\"")
					.Add("/p:UseSourceLink={0}", "true")
					.Add("/p:CoverletOutputFormat={0}", "cobertura"))
				.SetProjectFile(Solution));

			FileExists(CodeCoverageFile);
		});

	Target PerfTest => _ => _
		.DependsOn(Build)
		.Executes(() =>
		{
			DotNetRun(s => s.SetConfiguration(Configuration.Release)
				.SetWorkingDirectory(PerformanceProject));
		});

	Target Local_Benchmark => _ => _
		.DependsOn(Build)
		.Executes(async () =>
		{

			var server = Task.Run(() => DotNetRun(s => s.SetConfiguration(Configuration.Release)
				.SetWorkingDirectory(DemoServerProject))).ConfigureAwait(false);

			await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

			var client = Task.Run(() => DotNetRun(s => s.SetConfiguration(Configuration.Release)
				  .SetWorkingDirectory(DemoClientProject))).ConfigureAwait(false);

			await client;
			await server;
		});


	Target Report => _ => _
		.DependsOn(Test)
		.Executes(() =>
		{
			ReportGenerator(s => s.SetReports(CodeCoverageFile)
								.SetTargetDirectory(CodeCoverageReportOutput)
								.SetTag(GitVersion.NuGetVersionV2)
								.SetReportTypes(ReportTypes.HtmlInline_AzurePipelines_Dark));
		});


	Target CI => _ => _
		.DependsOn(Pack)
		.DependsOn(Report)
		.Executes(() =>
		{
			Nuke.Common.CI.AzurePipelines.AzurePipelines.Instance?.UpdateBuildNumber(GitVersion.NuGetVersionV2);
		});
}
