// A child process for harness tests. It stays alive and silent unless told otherwise.
// Commands run in order: --print LINE (stdout), --eprint LINE (stderr), --exit N,
// --wait-file PATH (continue once the file exists). It never listens on a port.
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--print":
            Console.Out.WriteLine(args[++i]);
            Console.Out.Flush();
            break;
        case "--eprint":
            Console.Error.WriteLine(args[++i]);
            Console.Error.Flush();
            break;
        case "--exit":
            return int.Parse(args[++i]);
        case "--wait-file":
            var path = args[++i];
            while (!File.Exists(path))
                Thread.Sleep(20);
            break;
        default:
            Console.Error.WriteLine($"QuietChild: unknown argument {args[i]}");
            return 64;
    }
}

Thread.Sleep(Timeout.Infinite);
return 0;
