using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using dotnetmtr.Tracert;


// Create some options:
var addressOption = new Option<string>(
        "--address",
        getDefaultValue: () => "willwh.com",
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
var rootCommand = new RootCommand();
rootCommand.Description = "dotnetmtr";

rootCommand.Add(addressOption);
rootCommand.Add(maxhopsOption);
rootCommand.Add(timeoutOption);



rootCommand.SetHandler((string address, int maxhops, int timeout) =>
{
    Tracert(address, maxhops, timeout);
}, addressOption, maxhopsOption, timeoutOption);

// Parse the incoming args and invoke the handler
return rootCommand.Invoke(args);

static IEnumerable<TracertEntry> Tracert(string Address, int MaxHops, int Timeout) {
    Ping pingSender = new Ping ();
    PingOptions options = new PingOptions ();

    Console.WriteLine($"The value for --address is: {Address}");
    Console.WriteLine($"The value for --max-hops is: {MaxHops}");
    Console.WriteLine($"The value for --timeout is: {Timeout}");
    // Use the default Ttl value which is 128,
    // but change the fragmentation behavior.
    options.DontFragment = true;

    // Create a buffer of 32 bytes of data to be transmitted.
    string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    byte[] buffer = Encoding.ASCII.GetBytes (data);
    // Need to figure out incorporating maxHops.... and hops in general :)
    // StopWatch to check timing on pings
    Stopwatch pingReplyTime = new Stopwatch();
    pingReplyTime.Start();
    PingReply reply = pingSender.Send (Address, Timeout, buffer, options);
    pingReplyTime.Stop();

    do {

        string hostname = string.Empty;
        if (reply.Address != null) {
            try {
                hostname = Dns.GetHostEntry(reply.Address).HostName;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        yield return new TracertEntry()
        {
            HopID = options.Ttl,
            Address = reply.Address == null ? "N/A" : reply.Address.ToString(),
            Hostname = hostname,
            ReplyTime = pingReplyTime.ElapsedMilliseconds,
            ReplyStatus = reply.Status
        };

        options.Ttl++;
        pingReplyTime.Reset();
        Console.WriteLine ("Address: {0}", reply.Address.ToString ());
        Console.WriteLine ("RoundTrip time: {0}", reply.RoundtripTime);
        Console.WriteLine ("Buffer size: {0}", reply.Buffer.Length);
    }
    while (reply.Status != IPStatus.Success && options.Ttl <= MaxHops);
}