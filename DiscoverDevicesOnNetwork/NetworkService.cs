using System.Net.NetworkInformation;
using System.Net;
using System.Runtime.InteropServices;

namespace DiscoverDevicesOnNetwork
{
    public class NetworkService
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        static extern int SendARP(int destIp, int srcIp, byte[] macAddr, ref uint physicalAddrLen);

        public static void Discover()
        {
            string baseIpAddress = "192.168.48"; // Replace with the base IP address of your LAN
            int startRange = 1;
            int endRange = 254; // Adjust the range as needed for your network

            for (int i = startRange; i <= endRange; i++)
            {
                string ipAddress = $"{baseIpAddress}.{i}";
                _ = Task.Run(() => CheckDevice(ipAddress));
            }

            Console.WriteLine("Scanning for devices on the LAN. Press Enter to exit.");
        }

        static async Task CheckDevice(string ipAddress)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync(ipAddress, 1000); // Adjust the timeout as needed

                if (reply.Status == IPStatus.Success)
                {

                    string macAddress = NetworkService.GetMacAddress(IPAddress.Parse(ipAddress))?.ToString() ?? "notfound";

                    Console.WriteLine($"Device found at IP address: \t {ipAddress} \t macAddress: {macAddress} \t hostName: {DiscoverDeviceName(ipAddress)}");
                    // You can perform further checks or actions for the discovered device here.
                }
            }
            catch (PingException ex)
            {
                Console.WriteLine(ex.Message);
                // Device not reachable or timeout occurred
            }
        }

        static PhysicalAddress GetMacAddress(IPAddress ipAddress)
        {
            byte[] macAddr = new byte[6];
            uint macAddrLen = (uint)macAddr.Length;

            byte[] ipBytes = ipAddress.GetAddressBytes();
            int destIp = BitConverter.ToInt32(ipBytes, 0);

            int result = SendARP(destIp, 0, macAddr, ref macAddrLen);

            if (result == 0 && macAddrLen >= 6)
            {
                string macAddress = string.Join(":", macAddr.Take(6).Select(b => b.ToString("X2")));
                return PhysicalAddress.Parse(macAddress);
            }
            else
            {
                return null!;
            }
        }

        static string DiscoverDeviceName(string ipAddress)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ipAddress);

                string deviceName = hostEntry.HostName;
                return deviceName;
            }
            catch (Exception)
            {
                return "not found";
            }
        }
    }
}
