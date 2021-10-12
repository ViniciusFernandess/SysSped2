using System.Net.NetworkInformation;
using System.Text;

namespace SysSped.Domain.Core.Services
{
    public class GeradorKeysService
    {
        public string Gerar(string macAdressSource)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(macAdressSource);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                    sb.Append(hashBytes[i].ToString("X2"));

                var key = new StringBuilder();

                for (int i = 0; i < sb.Length; i++)
                {
                    key.Append(sb[i]);

                    if (i == 5 || i == 11 || i == 17 || i == 23 || i == 29)
                        key.Append("-");
                }

                return key.ToString() + " - " + sb.ToString().Length;
            }
        }

        public string ObterEndercoMacLocal()
        {
            var enderecoMac = string.Empty;
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var adapter in nics)
            {
                if (enderecoMac == string.Empty)
                {
                    var properties = adapter.GetIPProperties();
                    enderecoMac = adapter.GetPhysicalAddress().ToString();
                }
            }

            return enderecoMac;
        }
    }
}
