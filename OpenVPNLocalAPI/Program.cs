using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using System.Net;


namespace OpenVPNLocalAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //OVPN.GetAdapterSpeed();
            CreateHostBuilder(args).Build().Run();
            //OVPN.init(OVPN.ReadLog());

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    
                    webBuilder.UseKestrel(opts =>
                    {
                        // Bind directly to a socket handle or Unix socket
                        // opts.ListenHandle(123554);
                        // opts.ListenUnixSocket("/tmp/kestrel-test.sock");
                        opts.Listen(IPAddress.Loopback, port: 5002);
                        opts.ListenAnyIP(80);
                        opts.ListenLocalhost(5004);
                        opts.ListenLocalhost(5005);
                    });
                });
    }
}
