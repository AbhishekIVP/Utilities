// See https://aka.ms/new-console-template for more information
using Medallion.Threading.Redis;
using StackExchange.Redis;

Console.WriteLine("Hello, World!");


ConfigurationOptions _options = new ConfigurationOptions()
{
    Password = "cGClMe4Srb+yZHpQMSYNfasQDoYTRoyTfqey8gNxOFMvJonwO6hCajUA7AfTDj8+V4zYeCupYF3ptz+w"
};
_options.EndPoints.Add("192.168.77.115:6379");

var connection = await ConnectionMultiplexer.ConnectAsync(_options); // uses StackExchange.Redis


var @lock = new RedisDistributedLock("MyLockName", connection.GetDatabase());
using (var handle = await @lock.TryAcquireAsync())
{
    if (handle != null)
    {
        Console.WriteLine("/* I have the lock */");
        Thread.Sleep(10000);
    }
    else
    {
        Console.WriteLine("// someone else has it :-(");
    }
}

await using (var handle = await @lock.AcquireAsync())
{
    if (handle != null)
    {
        Console.WriteLine("/* I2 have the lock */");
    }
    else
    {
        Console.WriteLine("// I2 someone else has it :-(");
    }
}