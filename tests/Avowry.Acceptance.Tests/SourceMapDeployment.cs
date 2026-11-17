using System.Diagnostics;
using System.Net;
using System.Text;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: Source maps stay out of deployed applications
//   As Morgan, I want proof that source maps are neither published nor served,
//   so that Phrase file names and lines never reach a deployment.
[Collection("Isolated timing")]
public class SourceMapDeployment
{
    private static readonly TimeSpan PublishTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    [Trait("Slice", "167")]
    public async Task Publish_output_never_contains_the_source_map()
    {
        // Given Morgan has built the Tasks app with the CLI into a temporary workspace
        using var workspace = TempWorkspace.Create();
        using var publishOutput = TempWorkspace.Create();
        var source = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);

        // When the generated API project is published into a separate temporary folder
        var project = workspace.FindSingle("Tasks.Api.csproj");
        var publish = await PublishAsync(project, publishOutput.Root);
        Assert.True(publish.ExitCode == 0, publish.Text);

        // Then the deployment contains the API assembly but no source map anywhere
        Assert.NotEmpty(Directory.GetFiles(publishOutput.Root, "Tasks.Api.dll", SearchOption.AllDirectories));
        var maps = Directory.GetFiles(publishOutput.Root, "*", SearchOption.AllDirectories)
            .Where(path => Path.GetFileName(path).EndsWith(".avowrymap.json", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(maps);
    }

    [Fact]
    [Trait("Slice", "167")]
    public async Task Running_api_answers_404_for_the_source_map_paths()
    {
        // Given Morgan has built and started the Tasks API
        using var workspace = TempWorkspace.Create();
        var source = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);
        await using var api = await GeneratedApi.StartAsync(workspace.FindSingle("Tasks.Api.csproj"));
        using var http = new HttpClient { BaseAddress = new Uri(api.BaseUrl) };

        // When Morgan calls a real endpoint and the obvious source-map paths
        using var body = new StringContent("""{"id":"task-167","title":"Keep maps private"}""", Encoding.UTF8, "application/json");
        var endpointResponse = await http.PostAsync("/tasks", body);
        var mapPaths = new[]
        {
            "/Tasks.avowrymap.json",
            "/avowrymap.json",
            "/Tasks.avowrymap.json",
            "/Tasks.Api/Tasks.avowrymap.json",
            "/.well-known/Tasks.avowrymap.json",
            "/wwwroot/Tasks.avowrymap.json",
            "/tasks.avowrymap.json",
            "/Tasks.avowrymap.json/",
        };
        var mapResponses = await Task.WhenAll(mapPaths.Select(http.GetAsync));
        using var head = await http.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/Tasks.avowrymap.json"));

        // Then the endpoint behaves as specified while every source-map path is absent
        Assert.Equal(HttpStatusCode.OK, endpointResponse.StatusCode);
        Assert.All(mapResponses, response => Assert.Equal(HttpStatusCode.NotFound, response.StatusCode));
        Assert.Equal(HttpStatusCode.NotFound, head.StatusCode);
    }

    private static async Task<CliRun> PublishAsync(string project, string output)
    {
        var info = PublishStartInfo(project, output);
        var result = await NestedDotnet.RunAsync(info, PublishTimeout);
        if (result.TimedOut)
            throw new TimeoutException($"dotnet publish exceeded {PublishTimeout.TotalMinutes:0} minutes.");

        return new CliRun(result.ExitCode, result.Stdout, result.Stderr);
    }

    private static ProcessStartInfo PublishStartInfo(string project, string output)
    {
        var info = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(project)!,
        };
        foreach (var argument in new[] { "publish", project, "--nologo", "--disable-build-servers", "-o", output })
            info.ArgumentList.Add(argument);
        NestedDotnet.Apply(info);
        return info;
    }
}
