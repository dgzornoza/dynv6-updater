using Dynv6Updater.Services.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dynv6Updater.Services
{
    public class UpdaterService : IUpdaterService
    {
        private const string CHECK_IP_ADDRESS = "http://checkip.dyndns.com/";
        private const string DYNV6_UPDATE_ADDRESS = "http://dynv6.com/api/update?hostname={0}&token={1}&ipv4={2} ";
        private const string REGEX_IP = @"(?<First>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Second>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Third>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Fourth>2[0-4]\d|25[0-5]|[01]?\d\d?)";

        private static IPAddress _lastIp = IPAddress.None;

        private readonly HttpClient _httpClient;
        private readonly AppSettingsModel _appSettingsModel;

        public UpdaterService(HttpClient httpClient, IOptions<AppSettingsModel> options)
        {
            _httpClient = httpClient;
            _appSettingsModel = options.Value;
        }

        public async Task UpdateIp()
        {
            // obtener ip actual
            string chekIpResult = await _httpClient.GetStringAsync(CHECK_IP_ADDRESS);
            Match match = Regex.Match(chekIpResult, REGEX_IP);

            // actualizar la ip en el servicio dynv6 si ha cambiado
            if (match.Success && IPAddress.TryParse(match.Value, out IPAddress newAddress) && !_lastIp.Equals(newAddress))
            {
                string url = string.Format(DYNV6_UPDATE_ADDRESS, _appSettingsModel.Dynv6HostName, _appSettingsModel.Dynv6HttpToken, newAddress);
                string dynv6UpdateResult = await _httpClient.GetStringAsync(url);
            }
        }
    }
}
