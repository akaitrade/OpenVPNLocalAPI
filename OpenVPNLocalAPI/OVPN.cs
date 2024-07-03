using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

public class OpenVPNLog
{
    public Title Title { get; set; }
    public Time Time { get; set; }
    public List<ClientList> ClientLists { get; set; } = new List<ClientList>();
    public List<RoutingTable> RoutingTables { get; set; } = new List<RoutingTable>();
    public GlobalStats GlobalStats { get; set; }
}

public class Title
{
    public string Value { get; set; }
}

public class Time
{
    public DateTime Timestamp { get; set; }
    public long TimeInSeconds { get; set; }
}

public class ClientList
{
    public string CommonName { get; set; }
    public string RealAddress { get; set; }
    public string VirtualAddress { get; set; }
    public string VirtualIPv6Address { get; set; }
    public long BytesReceived { get; set; }
    public long BytesSent { get; set; }
    public DateTime ConnectedSince { get; set; }
    public long ConnectedSinceTime { get; set; }
    public string Username { get; set; }
    public int ClientID { get; set; }
    public int PeerID { get; set; }
    public string DataChannelCipher { get; set; }
}

public class RoutingTable
{
    public string VirtualAddress { get; set; }
    public string CommonName { get; set; }
    public string RealAddress { get; set; }
    public DateTime LastRef { get; set; }
    public long LastRefTime { get; set; }
}

public class GlobalStats
{
    public int MaxBcastMcastQueueLength { get; set; }
}

public class OVPN
{
    public static string testlog = @"TITLE	OpenVPN 2.5.9 x86_64-pc-linux-gnu [SSL (OpenSSL)] [LZO] [LZ4] [EPOLL] [PKCS11] [MH/PKTINFO] [AEAD] built on Sep 29 2023
TIME	2024-07-03 11:07:29	1720004849
HEADER	CLIENT_LIST	Common Name	Real Address	Virtual Address	Virtual IPv6 Address	Bytes Received	Bytes Sent	Connected Since	Connected Since (time_t)	Username	Client ID	Peer ID	Data Channel Cipher
CLIENT_LIST	ovpn	188.90.40.58:58239	10.8.0.2		520170	818166	2024-07-03 11:07:14	1720004834	UNDEF	3	0	AES-256-GCM
HEADER	ROUTING_TABLE	Virtual Address	Common Name	Real Address	Last Ref	Last Ref (time_t)
ROUTING_TABLE	10.8.0.2	ovpn	188.90.40.58:58239	2024-07-03 11:07:28	1720004848
GLOBAL_STATS	Max bcast/mcast queue length	0
END";
    public static string ReadLog()
    {
        string logFilePath = "/var/log/openvpn/openvpn-status.log";

        try
        {
            string logContents = System.IO.File.ReadAllText(logFilePath);
            return logContents;
        }
        catch (FileNotFoundException)
        {
            Console.Error.WriteLine($"The log file at path {logFilePath} was not found.");
            return testlog;
        }
        catch (UnauthorizedAccessException)
        {
            Console.Error.WriteLine("You do not have permission to access this file.");
            return testlog;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred while reading the log file: {ex.Message}");
            return testlog;
        }
    }
    public static string init(string log)
    {
        

        OpenVPNLog openVPNLog = ParseLog(log);

        // Output parsed data for demonstration
        Console.WriteLine("Title: " + openVPNLog.Title.Value);
        Console.WriteLine("Time: " + openVPNLog.Time.Timestamp + " (" + openVPNLog.Time.TimeInSeconds + ")");

        foreach (var client in openVPNLog.ClientLists)
        {
            Console.WriteLine($"ClientList: {client.CommonName}, {client.RealAddress}, {client.VirtualAddress}, {client.BytesReceived}, {client.BytesSent}");
        }

        foreach (var route in openVPNLog.RoutingTables)
        {
            Console.WriteLine($"RoutingTable: {route.VirtualAddress}, {route.CommonName}, {route.RealAddress}, {route.LastRef}");
        }

        Console.WriteLine("GlobalStats: " + openVPNLog.GlobalStats.MaxBcastMcastQueueLength);
        // Serialize the OpenVPNLog object to a JSON string
        string jsonString = JsonSerializer.Serialize(openVPNLog, new JsonSerializerOptions
        {
            WriteIndented = true // This is optional, for pretty printing the JSON
        });

        return jsonString;
    }

    static OpenVPNLog ParseLog(string log)
    {
        OpenVPNLog openVPNLog = new OpenVPNLog();
        string[] lines = log.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string[] parts = line.Split('\t');
            try
            {
                switch (parts[0])
                {
                    case "TITLE":
                        openVPNLog.Title = new Title { Value = parts[1] };
                        break;
                    case "TIME":
                        openVPNLog.Time = new Time
                        {
                            Timestamp = DateTime.Parse(parts[1], CultureInfo.InvariantCulture),
                            TimeInSeconds = long.Parse(parts[2])
                        };
                        break;
                    case "CLIENT_LIST":
                        openVPNLog.ClientLists.Add(new ClientList
                        {
                            CommonName = parts[1],
                            RealAddress = parts[2],
                            VirtualAddress = parts[3],
                            VirtualIPv6Address = parts[4],
                            BytesReceived = long.Parse(parts[5]),
                            BytesSent = long.Parse(parts[6]),
                            ConnectedSince = DateTime.Parse(parts[7], CultureInfo.InvariantCulture),
                            ConnectedSinceTime = long.Parse(parts[8]),
                            Username = parts[9],
                            ClientID = int.Parse(parts[10]),
                            PeerID = int.Parse(parts[11]),
                            DataChannelCipher = parts[12]
                        });
                        break;
                    case "ROUTING_TABLE":
                        openVPNLog.RoutingTables.Add(new RoutingTable
                        {
                            VirtualAddress = parts[1],
                            CommonName = parts[2],
                            RealAddress = parts[3],
                            LastRef = DateTime.Parse(parts[4], CultureInfo.InvariantCulture),
                            LastRefTime = long.Parse(parts[5])
                        });
                        break;
                    case "GLOBAL_STATS":
                        if (parts.Length > 2 && parts[1] == "Max bcast/mcast queue length")
                        {
                            openVPNLog.GlobalStats = new GlobalStats
                            {
                                MaxBcastMcastQueueLength = int.Parse(parts[2])
                            };
                        }
                        break;
                    case "END":
                        // End of log
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing line: {line}");
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        return openVPNLog;
    }
}
