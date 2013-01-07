using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Xml;

namespace OpenTokApi.Core
{
    public class OpenTok
    {
        private readonly Random _random = new Random();

        public string ApiKey { get; protected set; }
        public string Server { get; protected set; }
        public string Secret { get; protected set; }
        public string TokenSentinel { get; protected set; }
        public string SdkVersion { get; protected set; }

        public OpenTok()
            : this(
                ConfigurationManager.AppSettings["opentok.key"],
                ConfigurationManager.AppSettings["opentok.secret"],
                ConfigurationManager.AppSettings["opentok.server"],
                ConfigurationManager.AppSettings["opentok.token.sentinel"],
                ConfigurationManager.AppSettings["opentok.sdk.version"]
            )
        {}

        public OpenTok(string apiKey, string secret, string server, string tokenSentinel = "T1==", string sdkVersion = "tbdotnet")
        {
            ApiKey = apiKey;
            Secret = secret;
            Server = server;
            TokenSentinel = tokenSentinel;
            SdkVersion = sdkVersion;

            ValidateSettings();

            Secret = Secret.Trim();
            Server = Server.TrimEnd(new[] {'/'});
        }

        /// <summary>
        /// The create_session() method of the OpenTokSDK object to create a new OpenTok session and obtain a session ID.
        /// </summary>
        /// <param name="location">n IP address that TokBox will use to situate the session in its global network. In general, you should not pass in a location hint; if no location hint is passed in, the session uses a media server based on the location of the first client connecting to the session. Pass a location hint in only if you know the general geographic region (and a representative IP address) and you think the first client connecting may not be in that region.</param>
        /// <param name="options">
        /// Optional. An object used to define peer-to-peer preferences for the session.
        /// 
        /// - p2p_preference (p2p.preference) : "disabled" or "enabled"
        /// - multiplexer_switchType (multiplexer.switchType)
        /// - multiplexer_switchTimeout (multiplexer.switchTimeout)
        /// - multiplexer_numOuputStreams (multiplexer.numOutputStreams)
        /// - echoSuppression_enabled (echoSuppression.enabled)
        /// 
        /// </param>
        /// <returns>sessionId</returns>
        public string CreateSession(string location, object options = null)
        {
            var paremeters = new RouteValueDictionary(options ?? new object()) {
                {"location", location},
                {"partner_id", ApiKey}
            };

            var dictionary = paremeters.ToDictionary(x => CleanupKey(x.Key), v => v.Value);
            var xmlDoc = CreateSessionId(string.Format("{0}/session/create", Server), dictionary);
            var sessionId = xmlDoc.GetElementsByTagName("session_id")[0].ChildNodes[0].Value;
            return sessionId;
        }

        /// <summary>
        /// In order to authenticate a user connecting to a OpenTok session, a user must pass an authentication token along with the API key.
        /// </summary>
        /// <param name="sessionId">
        /// Optional. An object used to define preferences for the token.
        /// 
        /// - role : "subscriber", "publisher", "moderator"
        /// - expire_time : DateTime for when the token should expire
        /// - connection_data : any metadata you want about the connection limited to 1000 characters
        /// </param>
        /// <param name="options"></param>
        /// <returns></returns>
        public string GenerateToken(string sessionId, object options = null)
        {

            var paremeters = new RouteValueDictionary(options ?? new object()) {
                {"session_id", sessionId},
                {"create_time",(int) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds},
                {"nonce", _random.Next(0, 999999)}
            };

            if (!paremeters.ContainsKey(TokenProperties.Role))
                paremeters.Add(TokenProperties.Role, Roles.Publisher);

            // Convert expire time to Unix Timestamp
            if (paremeters.ContainsKey(TokenProperties.ExpireTime))
            {
                var origin = new DateTime(1970, 1, 1, 0, 0, 0);
                var expireTime = (DateTime)paremeters[TokenProperties.ExpireTime];
                var diff = expireTime - origin;
                paremeters[TokenProperties.ExpireTime] = Math.Floor(diff.TotalSeconds);
            }

            var data = HttpUtility.ParseQueryString(string.Empty);
            foreach (var pair in paremeters)
                data.Add(CleanupKey(pair.Key), pair.Value.ToString());

            var sig = SignString(data.ToString(), Secret);
            var token = string.Format("{0}{1}", TokenSentinel, EncodeTo64(string.Format("partner_id={0}&sdk_version={1}&sig={2}:{3}", ApiKey, SdkVersion, sig, data)));

            return token;
        }

        static private string EncodeTo64(string data)
        {
            var encDataByte = Encoding.UTF8.GetBytes(data);
            var encodedData = Convert.ToBase64String(encDataByte);
            return encodedData;
        }

        private static string SignString(string message, string key)
        {
            var encoding = new ASCIIEncoding();

            var keyByte = encoding.GetBytes(key);

            var hmacsha1 = new HMACSHA1(keyByte);

            var messageBytes = encoding.GetBytes(message);
            var hashmessage = hmacsha1.ComputeHash(messageBytes);

            // Make sure to utilize ToLower() method, else an exception willl be thrown
            // Exception: 1006::Connecting to server to fetch session info failed.
            string result = ByteToString(hashmessage).ToLower();

            return result;
        }

        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                // Hex format
                sbinary += buff[i].ToString("X2");
            }
            return (sbinary);
        }

        private XmlDocument CreateSessionId(string uri, Dictionary<string, object> dict)
        {
            var xmlDoc = new XmlDocument();

            var postData = HttpUtility.ParseQueryString(string.Empty);

            foreach (var pair in dict)
                postData.Add(CleanupKey(pair.Key), pair.Value.ToString());

            byte[] postBytes = Encoding.UTF8.GetBytes(postData.ToString());

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            request.Headers.Add("X-TB-PARTNER-AUTH", string.Format("{0}:{1}", ApiKey, Secret));

            var requestStream = request.GetRequestStream();

            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            using (var response = (HttpWebResponse)request.GetResponse())
                if (response.StatusCode == HttpStatusCode.OK)
                    using (var reader = XmlReader.Create(response.GetResponseStream(), new XmlReaderSettings { CloseInput = true }))
                        xmlDoc.Load(reader);

            return xmlDoc;
        }

        private static string CleanupKey(string key)
        {
            var startsWith = new[] { "echoSuppression", "multiplexer", "p2p" };
            return startsWith.Any(key.StartsWith) ? key.Replace("_", ".") : key;
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(ApiKey)) throw new ArgumentException("api key is required");
            if (string.IsNullOrWhiteSpace(Secret)) throw new ArgumentException("secret key is required");
            if (string.IsNullOrWhiteSpace(Server)) throw new ArgumentException("server is required");
            if (string.IsNullOrWhiteSpace(TokenSentinel)) throw new ArgumentException("token sentinel is required");
            if (string.IsNullOrWhiteSpace(SdkVersion)) throw new ArgumentException("sdk version is required");
        }

    }
}