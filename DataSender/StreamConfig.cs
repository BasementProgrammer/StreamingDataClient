using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Oci.Common.Auth;
using Oci.SecretsService.Models;
using Oci.SecretsService.Requests;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace DataSender
{
    public class StreamConfig
    {
        IBasicAuthenticationDetailsProvider _config;
        public enum AuthType
        {
            ResourcePrincipal,
            ConfigFile
        }
        AuthType _authType;

        public StreamConfig(IConfiguration _appConfig)
        {
            try
            {
                _config = ResourcePrincipalAuthenticationDetailsProvider.GetProvider();
                _authType = AuthType.ResourcePrincipal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Instance Principals authentication: {ex.Message}");
                string profileName = _appConfig["ProfileName"];
                // Fallback to config file authentication
                _config = new ConfigFileAuthenticationDetailsProvider(profileName);
                _authType = AuthType.ConfigFile;
            }

            if (_config == null)
            {
                throw new Exception("Authentication provider could not be initialized.");
            }
            // get the configuration vault name. This will either be an environment variable, or an entry in appSettings.json
            ConfigVaultId = Environment.GetEnvironmentVariable("VaultID");
            // If the environment variable is not set, try to get it from appsettings.json
            if (string.IsNullOrEmpty(ConfigVaultId))
            {
                ConfigVaultId = _appConfig["VaultID"];
            }

            if (string.IsNullOrEmpty(ConfigVaultId))
            {
                // We need to get the config parameters from the appSettings.json file
                StreamId = _appConfig["StreamId"];
                EndpointConfiguration = _appConfig["EndpointConfiguration"];
            }
            else
            {
                // We will get the config parameters from the OCI vault
                StreamId = populateConfigFromVault("StreamId", ConfigVaultId);
                EndpointConfiguration = populateConfigFromVault("EndpointConfiguration", ConfigVaultId);
            }
        }

        private string populateConfigFromVault(string secretName, string vaultId)
        {
            var client = new Oci.SecretsService.SecretsClient(_config);
            var vaultClient = new Oci.VaultService.VaultsClient(_config);


            var getSecretBundleByNameRequest = new GetSecretBundleByNameRequest
            {
                SecretName = secretName,
                VaultId = vaultId,
            };

            var secretResponce = client.GetSecretBundleByName(getSecretBundleByNameRequest).Result;
            var secretBundle = secretResponce.SecretBundle;
            Base64SecretBundleContentDetails secretBundleContent = (Base64SecretBundleContentDetails)secretBundle.SecretBundleContent;
            var content = secretBundleContent.Content;
            var decodedBytes = Convert.FromBase64String(content);

            return Encoding.UTF8.GetString(decodedBytes);
        }

        public AuthType AuthenticationType
        {
            get { return _authType; }
        }

        public IBasicAuthenticationDetailsProvider AuthenticationProvider
        {
            get { return _config; }
        }

        public string ConfigVaultId { get; set; }

        /// <summary>
        /// The name of the cloud profile to use for authentication.
        /// </summary>
        public string ProfileName { get; set; }
        /// <summary>
        /// For OCI this is the endpointURL to referance your stream.
        /// </summary>
        public string EndpointConfiguration { get; set; }
        /// <summary>
        /// For OCI this is the OCID of the stream.
        /// </summary>
        public string StreamId { get; set; }
    }
}
