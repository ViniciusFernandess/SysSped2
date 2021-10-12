using SysSped.Domain.Core.Services;
using System;
using System.IO;
using System.Net;

namespace SysSped.Domain.Services
{
    public class ValidadorKeyService
    {
        public bool VerificaSeKeyEhValida()
        {
            string path = System.IO.Directory.GetCurrentDirectory();

            string pat2 = Environment.CurrentDirectory;

            var keyFromApi = ObterKeyDaApi();
            var keyLocal = ObterKeyLocal();

            if (keyFromApi == keyLocal)
            {
                File.WriteAllText(path + "/System.Memory.txt", DateTime.Now.Date.ToString());
                return true;
            }

            return false;
        }


        private string ObterKeyLocal()
        {
            var geradorKeyServ = new GeradorKeysService();

            var macLocal = geradorKeyServ.ObterEndercoMacLocal();
            var keyLocal = geradorKeyServ.Gerar(macLocal);

            return keyLocal;
        }

        private string ObterKeyDaApi()
        {
            string keyFromApi;

            var geradorKeyServ = new GeradorKeysService();
            var macLocal = geradorKeyServ.ObterEndercoMacLocal();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($@"https://localhost:44325/api/gerar/{macLocal}");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                keyFromApi = reader.ReadToEnd();
            }

            return keyFromApi;
        }
    }
}
