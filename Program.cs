using System.CommandLine;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;

// Create some options:
var addressOption = new Option<string>(
        "--address",
        getDefaultValue: () => "localhost",
        description: "An option whose argument is parsed as an int");
var maxhopsOption = new Option<int>(
        "--max-hops",
        getDefaultValue: () => 30,
        description: "Maximum number of hops");
var timeoutOption = new Option<int>(
        "--timeout",
        getDefaultValue: () => 5000,
        description: "Timeout value in milliseconds");

// Add the options to a root command:
var rootCommand = new RootCommand
{
    addressOption,
    maxhopsOption,
    timeoutOption
};

rootCommand.Description = "dotnetmtr";

rootCommand.SetHandler((string address, int maxhops, int timeout) =>
{
    Ping(address, maxhops, timeout);
    Console.WriteLine($"The value for --address is: {address}");
    Console.WriteLine($"The value for --max-hops is: {maxhops}");
    Console.WriteLine($"The value for --timeout is: {timeout}");
}, addressOption, maxhopsOption, timeoutOption);

// Parse the incoming args and invoke the handler
return rootCommand.Invoke(args);

static void Ping(string Address, int MaxHops, int Timeout) {
    Ping pingSender = new Ping ();
    PingOptions options = new PingOptions ();

    // Use the default Ttl value which is 128,
    // but change the fragmentation behavior.
    options.DontFragment = true;

    // Create a buffer of 32 bytes of data to be transmitted.
    string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    byte[] buffer = Encoding.ASCII.GetBytes (data);
    // Need to figure out incorporating maxHops.... and hops in general :)
    PingReply reply = pingSender.Send (Address, Timeout, buffer, options);
    if (reply.Status == IPStatus.Success)
    {
        Console.WriteLine ("Address: {0}", reply.Address.ToString ());
        Console.WriteLine ("RoundTrip time: {0}", reply.RoundtripTime);
        Console.WriteLine ("Buffer size: {0}", reply.Buffer.Length);
    }
}