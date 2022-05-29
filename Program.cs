using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace dotnetmtr {

    class Program {

        private static readonly byte[] _buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private const string _requestTimeOut = "Request timed out.";
        private const string _requestTimeNa = "*";

        static async Task Main(string[] args)
        {
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

            rootCommand.AddOption(addressOption);
            rootCommand.AddOption(maxhopsOption);
            rootCommand.AddOption(timeoutOption);

            rootCommand.SetHandler(async (string address, int maxhops, int timeout) =>
            {
               await traceRouteAsync(address, maxhops, timeout);
            }, addressOption, maxhopsOption, timeoutOption);

            // Parse the incoming args and invoke the handler
            await rootCommand.InvokeAsync(args);
        }

        public static async Task traceRouteAsync(string address, int maxHops, int timeout)
        {
            // Start parallel tasks for each hop
            var traceRouteTasks = new Task<TraceRouteResult>[maxHops];
            for (int baseHop = 0; baseHop < maxHops; baseHop++)
            {
                traceRouteTasks[baseHop] = traceRouteInteralAsync(address, baseHop, timeout);
            }

            await Task.WhenAll(traceRouteTasks);

            for (int hop = 0; hop < maxHops; hop++)
            {
                var traceTask = traceRouteTasks[hop];
                if (traceTask.Status == TaskStatus.RanToCompletion)
                {
                    var res = traceTask.Result;
                    writeToConsole(res.Message);

                    if (res.IsComplete)
                    {
                        //trace complete
                        break;
                    }
                }
                else
                {
                    writeToConsole($"Could not get result for hop #{hop + 1}");
                }
            }
        }

        public static async Task<TraceRouteResult> traceRouteInteralAsync(string address, int baseHop, int timeout)
        {
            using (Ping pingSender = new Ping())
            {
                var hop = baseHop + 1;

                PingOptions pingOptions = new PingOptions();
                Stopwatch stopWatch = new Stopwatch();
                pingOptions.DontFragment = true;
                pingOptions.Ttl = hop;

                stopWatch.Start();

                PingReply pingReply = await pingSender.SendPingAsync(
                    address,
                    timeout,
                    _buffer,
                    pingOptions
                );

                stopWatch.Stop();

                var elapsedMilliseconds = stopWatch.ElapsedMilliseconds;

                string pingReplyAddress;
                string strElapsedMilliseconds;

                if (pingReply.Status == IPStatus.TimedOut)
                {
                    pingReplyAddress = _requestTimeOut;
                    strElapsedMilliseconds = _requestTimeNa;
                }
                else
                {
                    pingReplyAddress = pingReply.Address.ToString();
                    strElapsedMilliseconds = $"{elapsedMilliseconds.ToString(CultureInfo.InvariantCulture)} ms";
                }

                var traceResults = new StringBuilder();
                traceResults.Append(hop.ToString(CultureInfo.InvariantCulture).PadRight(4, ' '));
                traceResults.Append(strElapsedMilliseconds.PadRight(10, ' '));
                traceResults.Append(pingReplyAddress);

                return new TraceRouteResult(traceResults.ToString(), pingReply.Status == IPStatus.Success);
            }
        }

        public static int writeToConsole(string message)
        {
            Console.WriteLine(message);
            return 0;
        }
    }
}