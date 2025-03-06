using Microsoft.Extensions.Configuration;
using System.Diagnostics;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var list = configuration.GetSection("exeFilesPath")!.GetChildren();
foreach (var item in list)
{
    var path = item.Value;
    ProcessStartInfo processInfo = new ProcessStartInfo
    {
        FileName = item.Value,
        UseShellExecute = false,
        WorkingDirectory = Path.GetDirectoryName(path)
    };
    Process pro = new Process();
    pro.StartInfo = processInfo;
    pro.Start();
}