using System.Security.Cryptography;

namespace BandHub.AuthService.Features
{
    public class TesteSeguranca
    {
        private const string Password = "Admin123456";
        public string BuscarUsuario(string nome)
        {
            return $"SELECT * FROM Users WHERE Name = '{nome}'";
        }

        private const string ApiKey = "sk_live_123456789";

        private const string Key = "MinhaChaveSuperSecreta123456789";

        private static readonly MD5 md5 = MD5.Create();

        private static readonly Random random = new Random();


    }
}
