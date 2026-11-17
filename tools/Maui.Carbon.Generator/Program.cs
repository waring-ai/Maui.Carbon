using Maui.Carbon.Generator;

// Usage: dotnet run --project tools/Maui.Carbon.Generator -- [--design <dir>] [--out <dir>] [--check]
//   --design  folder holding tokens.json and icons/ (default: <repo>/design/carbon)
//   --out     folder receiving the *.g.cs files (default: <repo>/src/Maui.Carbon/Generated)
//   --check   do not write; exit 1 when the committed files differ from what would be generated

string? design = null, output = null;
var check = false;
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--design": design = args[++i]; break;
        case "--out": output = args[++i]; break;
        case "--check": check = true; break;
        default:
            Console.Error.WriteLine($"Unknown argument '{args[i]}'.");
            return 2;
    }
}

var root = RepoPaths.FindRoot(Directory.GetCurrentDirectory());
design ??= Path.Combine(root, "design", "carbon");
output ??= Path.Combine(root, "src", "Maui.Carbon", "Generated");

var files = CarbonGenerator.Generate(design);

if (check)
{
    var drift = CarbonGenerator.FindDrift(files, output);
    foreach (var d in drift) Console.Error.WriteLine($"drift: {d}");
    if (drift.Count > 0)
    {
        Console.Error.WriteLine("Generated code is out of date. Run the generator without --check.");
        return 1;
    }
    Console.WriteLine($"Generated code is up to date ({files.Count} files).");
    return 0;
}

Directory.CreateDirectory(output);
foreach (var (name, content) in files)
    File.WriteAllText(Path.Combine(output, name), content);
foreach (var stale in Directory.GetFiles(output, "*.g.cs").Where(p => !files.ContainsKey(Path.GetFileName(p))))
    File.Delete(stale);
Console.WriteLine($"Wrote {files.Count} files to {output}.");
return 0;
