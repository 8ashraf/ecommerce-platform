/*using Shop.Identity;
using Shop.Identity.Services;

await using var db = new AppDbContext();
var identity = new IdentityService(db);

Console.Write("Phone: ");
var phone = Console.ReadLine()!;
await identity.StartRegistrationAsync(phone);

Console.Write("OTP: ");
var code = Console.ReadLine()!;
var account = await identity.VerifyAsync(phone, code);

Console.WriteLine(account is null
    ? "فشل التحقق"
    : $"نجح. AccountId = {account.Id}, Type = {account.Type}");


*/

using Microsoft.Extensions.Configuration;
using Shop.Identity;
using Shop.Identity.Services;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

await using var db = new AppDbContext();
var identity = new IdentityService(db, new TokenService(config));

Console.Write("Phone: ");
var phone = Console.ReadLine()!;
await identity.StartRegistrationAsync(phone);

Console.Write("OTP: ");
var code = Console.ReadLine()!;
var result = await identity.VerifyAsync(phone, code);

if (result is null)
{
    Console.WriteLine("فشل التحقق");
}
else
{
    Console.WriteLine($"نجح. AccountId = {result.Account.Id}, Type = {result.Account.Type}");
    Console.WriteLine($"TOKEN: {result.AccessToken}");
}