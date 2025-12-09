using Posts.Migrations;

if (args.Length == 0)
{
    Console.WriteLine("Pass connection string as an argument");
    return;
}

var connectionString = args[0];
await Migrations.RunAsync(connectionString);