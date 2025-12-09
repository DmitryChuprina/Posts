namespace Posts.Migrations
{
    public static class Migrations
    {
        public static async Task RunAsync(string connectionString)
        {
            var migrator = new Migrator(connectionString);
            await migrator.MigrateAsync();

            Console.WriteLine("Migration complete!");
        }
    }
}
