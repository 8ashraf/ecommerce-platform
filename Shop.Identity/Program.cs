using Shop.Identity;
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