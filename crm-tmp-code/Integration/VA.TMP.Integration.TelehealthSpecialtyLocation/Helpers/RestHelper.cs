using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Helpers
{
    public class RestHelper
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private IOptions<ApplicationSettings> _settings;

        public RestHelper(ILogger logger, IOptions<ApplicationSettings> settings, IConfiguration config)
        {
            _config = config;
            _logger = logger;
            //_logger = logger.CreateLogger<RestHelper>();
            _settings = settings;
        }

        public string GetToken(string appId, string secret, string scope, string tenantId)
        {
            using (var client = new HttpClient())
            {
                var uri = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                client.DefaultRequestHeaders.ConnectionClose = true;

                HttpResponseMessage response;

                var content = new FormUrlEncodedContent(new[] {
                     new KeyValuePair<string, string>("client_id", $"{appId}"),
                     new KeyValuePair<string, string>("grant_type", "client_credentials"),
                     new KeyValuePair<string, string>("client_secret", $"{secret}"),
                     new KeyValuePair<string, string>("scope", $"{scope}/.default")
                });

                try
                {
                    _logger.LogDebug("Get Token");
                    response = client.PostAsync(uri, content).ConfigureAwait(false).GetAwaiter().GetResult();

                    var respContent = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                    _logger.LogDebug($"Get Token Response: {respContent}");

                    var token = JsonSerializer.Deserialize<TokenResponse>(respContent);

                    return token.AccessToken;
                }
                catch (System.Exception e)
                {
                    _logger.LogError($"Get Toke failed with error: {e}");
                    throw;
                }
            }
        }

        public async Task<string> GetTokenAsync(string appId, string secret, string scope, string tenantId)
        {
            using (var client = new HttpClient())
            {
                var uri = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                client.DefaultRequestHeaders.ConnectionClose = true;

                HttpResponseMessage response;

                var content = new FormUrlEncodedContent(new[] {
                     new KeyValuePair<string, string>("client_id", $"{appId}"),
                     new KeyValuePair<string, string>("grant_type", "client_credentials"),
                     new KeyValuePair<string, string>("client_secret", $"{secret}"),
                     new KeyValuePair<string, string>("scope", $"{scope}/.default")
                });

                try
                {
                    _logger.LogDebug("Get Token");
                    response = await client.PostAsync(uri, content);

                    var respContent = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                    _logger.LogDebug($"Get Token Response: {respContent}");

                    var token = JsonSerializer.Deserialize<TokenResponse>(respContent);

                    return token.AccessToken;
                }
                catch (System.Exception e)
                {
                    _logger.LogError($"Get Toke failed with error: {e}");
                    throw;
                }
            }
        }

        public string Get(string uri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;

                var token = GetToken(_settings.Value.AppId, _config["TMP_Client_Secret"], _settings.Value.Scope, _settings.Value.TenantId);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = client.GetStringAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult();

                return response;
            }
        }

        public T Get<T>(string uri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;

                var token = GetToken(_settings.Value.AppId, _config[_settings.Value.KeyVaultSecretName], _settings.Value.Scope, _settings.Value.TenantId);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    //T response = client.GetFromJsonAsync<T>(uri).ConfigureAwait(false).GetAwaiter().GetResult();
                    var jsonResponse = client.GetAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (jsonResponse.IsSuccessStatusCode)
                    {
                        var jsonContent = jsonResponse.Content.ReadAsStringAsync().Result;
                        _logger.LogDebug($"Response: {jsonContent}");

                        var response = JsonSerializer.Deserialize<T>(jsonContent);
                        return response;
                    }
                    else
                    {
                        return default;
                    }
                }
                catch (System.Exception e)
                {
                    _logger.LogError($"Post failed with error: {e}");
                    throw;
                }
            }
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;
                client.Timeout = TimeSpan.FromSeconds(_settings.Value.TimeOut);

                var token = await GetTokenAsync(_settings.Value.AppId, _config[_settings.Value.KeyVaultSecretName], _settings.Value.Scope, _settings.Value.TenantId);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var jsonResponse = await client.GetAsync(uri);

                    if (jsonResponse.IsSuccessStatusCode)
                    {
                        var jsonContent = jsonResponse.Content.ReadAsStringAsync().Result;
                        _logger.LogDebug($"Response: {jsonContent}");

                        var response = JsonSerializer.Deserialize<T>(jsonContent);
                        return response;
                    }
                    else
                    {
                        return default;
                    }
                }
                catch (System.Exception e)
                {
                    _logger.LogError($"Get failed with error: {e}");
                    throw;
                }
            }
        }

        public async Task<T> GetAsync<T>(string uri, string token) where T : TmpBaseResponseMessage
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;
                client.Timeout = TimeSpan.FromSeconds(_settings.Value.TimeOut);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var jsonResponse = await client.GetAsync(uri);

                    if (jsonResponse.IsSuccessStatusCode)
                    {
                        var jsonContent = jsonResponse.Content.ReadAsStringAsync().Result;
                        _logger.LogDebug($"Response: {jsonContent}");

                        var response = JsonSerializer.Deserialize<T>(jsonContent);
                        return response;
                    }
                    else
                    {
                        return default;
                    }
                }
                catch (System.Exception e)
                {
                    var resp = default(T);
                    resp.ExceptionMessage = e.ToString();
                    resp.ExceptionOccured = true;
                    _logger.LogError($"Get failed for Message Id {resp.MessageId} with error: {JsonSerializer.Serialize(resp)}");
                    throw;
                }
            }
        }

        public async Task<T> GetAsync<T>(string uri, string messageId, string token) where T : TmpBaseResponseMessage, new()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;
                client.Timeout = TimeSpan.FromSeconds(_settings.Value.TimeOut);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var jsonResponse = await client.GetAsync(uri);

                    if (jsonResponse.IsSuccessStatusCode)
                    {
                        var jsonContent = jsonResponse.Content.ReadAsStringAsync().Result;
                        _logger.LogDebug($"CorrelationId: {messageId} - Response: {jsonContent}");

                        var response = JsonSerializer.Deserialize<T>(jsonContent);
                        response.MessageId = messageId;
                        return response;
                    }
                    else
                    {
                        var resp = new T
                        {
                            MessageId = messageId
                        };
                        return resp;
                    }
                }
                catch (System.Exception e)
                {
                    var resp = new T
                    {
                        ExceptionMessage = e.ToString(),
                        ExceptionOccured = true,
                        MessageId = messageId
                    };
                    _logger.LogError($"CorrelationId: {messageId} - Get failed with error: {JsonSerializer.Serialize(resp)}");
                    throw;
                }
            }
        }

        public string Post(string uri, string content)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;

                var token = GetToken(_settings.Value.AppId, _config["TMP_Client_Secret"], _settings.Value.Scope, _settings.Value.TenantId);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = client.PostAsJsonAsync(uri, content, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public T Post<T, R>(string uri, R content)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;

                var token = GetToken(_settings.Value.AppId, _config[_settings.Value.KeyVaultSecretName], _settings.Value.Scope, _settings.Value.TenantId);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var serializedContent = JsonSerializer.Serialize(content);
                    _logger.LogDebug($"Content: {serializedContent}");

                    HttpResponseMessage response;

                    using (var ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(serializedContent)))
                    using (var sc = new StreamContent(ms))
                    {
                        sc.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = client.PostAsync(uri, sc, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        _logger.LogDebug($"Response: {responseContent}");

                        return JsonSerializer.Deserialize<T>(responseContent, null);
                    }
                    else
                    {
                        return default(T);
                    }
                }
                catch (System.Exception e)
                {
                    _logger.LogError($"Post failed with error: {e}");
                    throw;
                }
            }
        }

        public async Task<T> PostAsync<T, R>(string uri, R content)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;
                client.Timeout = TimeSpan.FromSeconds(_settings.Value.TimeOut);

                var token = await GetTokenAsync(_settings.Value.AppId, _config[_settings.Value.KeyVaultSecretName], _settings.Value.Scope, _settings.Value.TenantId);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var serializedContent = JsonSerializer.Serialize(content);
                    _logger.LogDebug($"Content: {serializedContent}");

                    HttpResponseMessage response;

                    using (var ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(serializedContent)))
                    using (var sc = new StreamContent(ms))
                    {
                        sc.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = await client.PostAsync(uri, sc, CancellationToken.None);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        _logger.LogDebug($"Response: {responseContent}");

                        return JsonSerializer.Deserialize<T>(responseContent, null);
                    }
                    else
                    {
                        return default;
                    }
                }
                catch (System.Exception e)
                {
                    _logger.LogError($"Post failed with error: {e}");
                    return default;
                    //throw;
                }
            }
        }

        public async Task<T> PostAsync<T, R>(string uri, R content, string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;
                client.Timeout = TimeSpan.FromSeconds(_settings.Value.TimeOut);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var serializedContent = JsonSerializer.Serialize(content);
                    //_logger.LogDebug($"Content: {serializedContent}");

                    HttpResponseMessage response;

                    using (var ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(serializedContent)))
                    using (var sc = new StreamContent(ms))
                    {
                        sc.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = await client.PostAsync(uri, sc, CancellationToken.None);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        //_logger.LogDebug($"Response: {responseContent}");

                        return JsonSerializer.Deserialize<T>(responseContent, null);
                    }
                    else
                    {
                        return default;
                    }
                }
                catch (System.Exception e)
                {
                    _logger.LogError($"Post failed with error: {e}");
                    return default;
                    //throw;
                }
            }
        }

        public async Task<T> PostAsync<T, R>(string uri, R content, string token, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;
                client.Timeout = TimeSpan.FromSeconds(_settings.Value.TimeOut);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var serializedContent = JsonSerializer.Serialize(content);
                    //_logger.LogDebug($"Content: {serializedContent}");

                    HttpResponseMessage response;

                    using (var ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(serializedContent)))
                    using (var sc = new StreamContent(ms))
                    {
                        sc.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = await client.PostAsync(uri, sc, cancellationToken);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        _logger.LogDebug($"Response: {responseContent}");

                        return JsonSerializer.Deserialize<T>(responseContent, null);
                    }
                    else
                    {
                        return default;
                    }
                }
                catch (System.Exception e)
                {
                    _logger.LogError($"Post failed with error: {e}");
                    return default;
                    //throw;
                }
            }
        }

        public async Task<T> PostAsync<T, R>(string uri, R content, string messageId, string token, CancellationToken cancellationToken) where T : TmpBaseResponseMessage, new()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = true;
                client.Timeout = TimeSpan.FromSeconds(_settings.Value.TimeOut);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var serializedContent = JsonSerializer.Serialize(content);
                    //_logger.LogDebug($"Content: {serializedContent}");

                    HttpResponseMessage response;

                    using (var ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(serializedContent)))
                    using (var sc = new StreamContent(ms))
                    {
                        sc.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = await client.PostAsync(uri, sc, cancellationToken);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        _logger.LogDebug($"CorrelationId: {messageId} - Response: {responseContent}");

                        return JsonSerializer.Deserialize<T>(responseContent, null);
                    }
                    else
                    {
                        var respContent = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        var resp = new T
                        {
                            ExceptionOccured = true,
                            ExceptionMessage = respContent,
                            MessageId = messageId
                        };

                        return resp;
                    }
                }
                catch (System.Exception e)
                {
                    var resp = new T
                    {
                        ExceptionMessage = e.ToString(),
                        ExceptionOccured = true,
                        MessageId = messageId
                    };
                    _logger.LogError($"CorrelationId: {messageId} - Post failed with error: {JsonSerializer.Serialize(resp)}");
                    return resp;
                }
            }
        }
    }
}
