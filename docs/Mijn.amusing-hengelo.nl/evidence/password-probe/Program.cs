using System.Text.Json;
var fixtures=JsonSerializer.Deserialize<List<Fixture>>(File.ReadAllText(args[0]));
var results=new List<object>();
foreach(var f in fixtures) {
 bool valid=BCrypt.Net.BCrypt.Verify(f.password,f.hash);
 bool wrong=BCrypt.Net.BCrypt.Verify("Wrong-2026!",f.hash);
 if(!valid || wrong) throw new Exception("PHP verification failed: "+f.name);
 var upgraded=BCrypt.Net.BCrypt.HashPassword(f.password);
 if(!BCrypt.Net.BCrypt.Verify(f.password,upgraded)) throw new Exception("Second login failed");
 results.Add(new {f.name,f.password,hash=upgraded,phpPrefix=f.hash.Substring(0,7),netPrefix=upgraded.Substring(0,7),valid,wrong});
}
File.WriteAllText(args[1],JsonSerializer.Serialize(results));
Console.WriteLine("PASS: PHP -> BCrypt.Net and second login, 4 cases; wrong passwords rejected.");
record Fixture(string name,string password,string hash);
