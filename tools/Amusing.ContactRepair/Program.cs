using System.Text.Json;
using Amusing.ContactRepair;
using MySqlConnector;

try
{
    string Value(string key)
    {
        var index = Array.IndexOf(args, key);
        if (index < 0 || index + 1 >= args.Length) throw new InvalidOperationException("Ontbrekend argument: " + key);
        return args[index + 1];
    }
    var execute = args.Contains("--execute");
    var path = Path.GetFullPath(Value(execute ? "--execute" : "--plan"));
    var database = Value("--database");
    if (database is not ("amusing" or "amusingdev")) throw new InvalidOperationException("Onbekende database.");
    if (execute && database == "amusing" && !args.Contains("--allow-production")) throw new InvalidOperationException("Productiecorrectie is niet expliciet ingeschakeld.");
    var connection = Environment.GetEnvironmentVariable("AMUSING_ROLE_REPAIR_CONNECTION") ?? throw new InvalidOperationException("Verbindingsinstelling ontbreekt.");
    if (new MySqlConnectionStringBuilder(connection).Database != database) throw new InvalidOperationException("Verbinding wijkt af van gekozen database.");
    var store = new ContactRepairStore(connection);
    ContactRepairPlan plan;
    var changed = 0;
    if (execute)
    {
        plan = JsonSerializer.Deserialize<ContactRepairPlan>(await File.ReadAllTextAsync(path)) ?? throw new InvalidOperationException("Ongeldig herstelplan.");
        changed = await store.ApplyAsync(plan, Value("--actor"));
    }
    else
    {
        for (var parent = new DirectoryInfo(Path.GetDirectoryName(path)!); parent is not null; parent = parent.Parent)
            if (File.Exists(Path.Combine(parent.FullName, ".git")) || Directory.Exists(Path.Combine(parent.FullName, ".git")))
                throw new InvalidOperationException("Bewaar persoonsgegevens buiten Git.");
        plan = await store.PlanAsync();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(output, plan, new JsonSerializerOptions { WriteIndented = true });
    }
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        Database = database,
        Plan = path,
        Groups = plan.Groups.Count,
        ChangedGroups = changed,
        RemovedLinks = plan.Groups.Sum(g => g.Before.Except(g.After).Count()),
        AddedLinks = plan.Groups.Sum(g => g.After.Except(g.Before).Count()),
        UniqueEmailsBefore = plan.Groups.Sum(g => g.Before.Where(l => ContactPolicy.IsContact(l.Role)).Select(ContactPolicy.EmailKey).Distinct().Count()),
        UniqueEmailsAfter = plan.Groups.Sum(g => g.After.Where(l => ContactPolicy.IsContact(l.Role)).Select(ContactPolicy.EmailKey).Distinct().Count())
    }));
}
catch (Exception error)
{
    Console.Error.WriteLine(error is InvalidOperationException or UnauthorizedAccessException ? error.Message : "Contactcorrectie afgebroken: " + error.GetType().Name);
    Environment.ExitCode = 1;
}
