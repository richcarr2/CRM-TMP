using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace VRMRest
{

    /// <summary>
    /// 
    /// Instructions on adding to your plugin,
    /// 1) Add a an existing Item to your project, navigate to this cs file, down in the bottom right corner where it says "Add", change to "Add as Link"
    /// 2) Add a Reference to System.Net.Http to the plugin project this is needed by HttpClient
    /// 3) Add a Reference to System.Xml to the plugin project this is needed by the Json Deserializer.
    /// 
    /// </summary>
    public class Utility
    {
        public enum LogLevel { Debug = 935950000, Info = 935950001, Warn = 935950002, Error = 935950003, Fatal = 935950004, Timing = 935950005 };

        public const string OneWayPassTest = "TestMessages#OneWayPassTest";
        public const string TwoWayPassTest = "TestMessages#TwoWayPassTest";
        public const string TwoMinuteTest = "TestMessages#TwoMinuteTest";
        public const string OneWayTimedTest = "TestMessages#OneWayTimedTest";

        public const string CreateCRMLogEntryRequest = "CRMe#CreateCRMLogEntryRequest";

        const string _urlRestPath = "/Servicebus/Rest/{0}";
        const string _urlParams = "?messageId={0}&messageType=text%2Fjson&isQueued=false";

        const string SEND = "Send";
        const string SEND_RECEIVE = "SendReceive";

        const string _vimtExceptionMessage = "The Query of the Legacy system timed out, click on refresh to try again";
        const int DEFAULT_TIMEOUT = 60;

        /// <summary>
        /// Send a Request Message to VIMT.  Response is always null.
        /// VIMT Message Handler should be a RequestHandler.
        /// </summary>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="messageId">Request Message</param>
        /// <param name="requestObj">Request Object</param>
        /// <param name="logSettings">Log Settings</param>
        /// <returns>HttpResponseMessage: null</returns>
        public static HttpResponseMessage Send(Uri baseUri, string messageId, object requestObj, LogSettings logSettings)
        {
            return Send(baseUri, messageId, requestObj, logSettings, 0);
        }

        /// <summary>
        /// Send a Request Message to VIMT.  
        /// getResposne specifies whether or not a response is expected.  The default is false.
        /// 
        /// If getResponse is true, then a VIMT RequestResponseHandler is expected
        /// If getResponse is false, then a VIMT RequestHandler is expected
        /// </summary>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="messageId">Request Message</param>
        /// <param name="requestObj">Request Object</param>
        /// <param name="logSettings">Log Settings</param>
        /// <param name="timeoutSeconds">Timeout in Seconds</param>
        /// <param name="getResponse">Get a Response (if true, uses SEND_RECEIVE, if false, uses SEND)</param>
        /// <returns>HttpResponseMessage: null if getResponse is false</returns>
        public static HttpResponseMessage Send(Uri baseUri, string messageId, object requestObj, LogSettings logSettings, int timeoutSeconds, bool getResponse=false)
        {
            HttpResponseMessage responseMessage = null;

            Uri uri = FormatUri(baseUri, getResponse?SEND_RECEIVE:SEND, messageId);

            try
            {
                var request = WebRequest.Create(uri);
                request.Method = WebRequestMethods.Http.Post;

                request.Timeout = DEFAULT_TIMEOUT * 1000;
                if (timeoutSeconds > 0) request.Timeout = timeoutSeconds * 1000;

                var requestStream = request.GetRequestStream();

                requestStream = ObjectToJSonStream(requestObj, requestStream);

                if (getResponse)
                {
                    using (var response = request.GetResponse())
                    {
                        using (var responseReader = new StreamReader(response.GetResponseStream()))
                        {
                            var text = responseReader.ReadToEnd();
                            responseMessage = new HttpResponseMessage();
                            responseMessage.Content = new StringContent(text, Encoding.UTF8, "application/json");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                if (ex.GetType() == typeof(WebException))
                {
                    if (((WebException)ex).Status == WebExceptionStatus.Timeout)
                    {
                        throw new VIMTTimeOutExeption(_vimtExceptionMessage);
                    }
                }

                if (logSettings != null)
                {
                    LogError(baseUri, logSettings, "Send", ex);
                }

                throw;
            }

            return responseMessage;
        }

        /// <summary>
        /// Send an Async Request Message to VIMT.  
        /// If there is a callback, the VIMT message handler should be a RequestResponseHandler.
        /// If there is no callback, the VIMT message handler should be RequestHandler. 
        /// </summary>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="messageId">Request Message</param>
        /// <param name="obj">Request Object</param>
        /// <param name="logSettings">Log Settings</param>
        /// <param name="callBack">Callback Method (if null, a SEND is used, if not null, a SEND_RECEIVE is used)</param>
        public static void SendAsync(Uri baseUri, string messageId, object obj, LogSettings logSettings, Action<HttpResponseMessage> callBack)
        {
            SendAsync(baseUri, messageId, obj, logSettings, callBack, 0);
        }

        /// <summary>
        /// Send an Async Request Message to VIMT.  
        /// If there is a callback, the VIMT message handler should be a RequestResponseHandler.
        /// If there is no callback, the VIMT message handler should be RequestHandler. 
        /// </summary>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="messageId">Request Message</param>
        /// <param name="obj">Request Object</param>
        /// <param name="logSettings">Log Settings</param>
        /// <param name="callBack">Callback Method (if null, a SEND is used, if not null, a SEND_RECEIVE is used)</param>
        /// <param name="timeoutSeconds">Timeout in Seconds</param>
        public static void SendAsync(Uri baseUri, string messageId, object obj, LogSettings logSettings, Action<HttpResponseMessage> callBack, int timeoutSeconds)
        {
            new Thread(() =>
            {
                try
                {
                    Thread.CurrentThread.IsBackground = false;
                    if (callBack != null)
                    {
                        HttpResponseMessage response = Send(baseUri, messageId, obj, logSettings, timeoutSeconds, true);
                        callBack(response);
                    }
                    else
                    {
                        Send(baseUri, messageId, obj, logSettings, timeoutSeconds, false);
                    }
                }
                catch (Exception ex)
                {
                    if (logSettings != null)
                    {
                        LogError(baseUri, logSettings, "SendAsync", ex);
                    }
                }
            }).Start();

        }

        /// <summary>
        /// Send a Request Message to VIMT and get the response of type T.
        /// The VIMT Message Handler should be a RequestResponseHandler
        /// </summary>
        /// <typeparam name="T">Response Object Type</typeparam>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="messageId">Request Message</param>
        /// <param name="obj">Request Object</param>
        /// <param name="logSettings">Log Settings</param>
        public static T SendReceive<T>(Uri baseUri, string messageId, object obj, LogSettings logSettings)
        {
            return SendReceive<T>(baseUri, messageId, obj, logSettings, 0);
        }

        /// <summary>
        /// Send a Request Message to VIMT and get the response of type T.
        /// The VIMT Message Handler should be a RequestResponseHandler
        /// </summary>
        /// <typeparam name="T">Response Object Type</typeparam>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="messageId">Request Message</param>
        /// <param name="obj">Request Object</param>
        /// <param name="logSettings">Log Settings</param>
        /// <param name="timeoutSeconds">Timeout in Seconds</param>
        public static T SendReceive<T>(Uri baseUri, string messageId, object obj, LogSettings logSettings, int timeoutSeconds)
        {
            string message = string.Empty;
            Uri uri = FormatUri(baseUri, SEND_RECEIVE, messageId);

            try
            {
                var request = WebRequest.Create(uri);
                request.Method = WebRequestMethods.Http.Post;

                request.Timeout = DEFAULT_TIMEOUT * 1000;
                if (timeoutSeconds > 0) request.Timeout = timeoutSeconds * 1000;


                var requestStream = request.GetRequestStream();

                requestStream = ObjectToJSonStream(obj, requestStream);

                using (var response = request.GetResponse())
                {
                    using (var responseReader = new StreamReader(response.GetResponseStream()))
                    {

                        string responseValue = responseReader.ReadToEnd();

                        //Parse message from VIMT, if there are valid tags.
                        int start = responseValue.IndexOf("<Message>") + 9;
                        int end = responseValue.IndexOf("</Message>");

                        if (end != -1)
                        {

                            message = responseValue.Substring(start, end - start);
                        }
                    }
                }
            }

            catch (Exception ex)
            {

                if (ex.GetType() == typeof(WebException))
                {
                    if (((WebException)ex).Status == WebExceptionStatus.Timeout)
                    {
                        throw new VIMTTimeOutExeption(_vimtExceptionMessage);
                    }
                }
                if (logSettings != null)
                {
                    LogError(baseUri, logSettings, "SendReceive", ex);
                }
                throw;
            }
            //Debug.WriteLine("Thread ID {0} Ending", Thread.CurrentThread.ManagedThreadId);

            T retObj = DeserializeResponse<T>(message);
            return retObj;
        }


        /// <summary>
        /// Format the URI with method and Message type.
        /// </summary>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="method">SEND or SEND_RECEIVE</param>
        /// <param name="messageId">Request Message</param>
        /// <returns>Destination Uri</returns>
        public static Uri FormatUri(Uri baseUri, string method, string messageId)
        {
            string urlRestPath = string.Format(_urlRestPath, method);
            string urlParams = string.Format(_urlParams, Uri.EscapeDataString(messageId));
            Uri relativeUri = new Uri(urlRestPath + urlParams, UriKind.Relative);
            Uri retUri = new Uri(baseUri, relativeUri);
            return retUri;
        }

        /// <summary>
        /// Deserialize Message to type T
        /// </summary>
        /// <typeparam name="T">Response Object Type</typeparam>
        /// <param name="message">String message to deserialize</param>
        /// <returns>Response Object</returns>
        public static T DeserializeResponse<T>(string message)
        {
            T retObj;
            byte[] b = Convert.FromBase64String(message);
            UTF8Encoding enc = new UTF8Encoding();
            string mess = enc.GetString(b);

            //REplace out the NewtonSoft specific dates with datacontract dates.
            string fixedDates = Regex.Replace(mess, @"new Date\(([-+0-9]*)\)", "\"\\/Date($1+0000)\\/\"");

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(enc.GetBytes(fixedDates), 0, fixedDates.Length);

                ms.Position = 0;

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                retObj = (T)ser.ReadObject(ms);
            }
            return retObj;
        }
        
        /// <summary>
        /// Convert object to MemoryStream in Json.
        /// </summary>
        /// <param name="obj">Object to convert to a json stream</param>
        /// <returns>MemoryStream</returns>
        private static Stream ObjectToJSonStream(object obj, Stream stream)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());
            ser.WriteObject(stream, obj);
            //stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Log Error
        /// </summary>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="org">CRM Organization</param>
        /// <param name="configFieldName">Not Used: Future On/Off switch</param>
        /// <param name="userId">Calling UserId</param>
        /// <param name="method">Child Calling Method or Sub Procedure</param>
        /// <param name="message">Error Message</param>
        /// <param name="callingMethod">Parent Calling Method</param>
        public static void LogError(Uri baseUri, string org, string configFieldName, Guid userId, string method, string message, string callingMethod=null)
        {
            string crme_method;
            if (!string.IsNullOrEmpty(callingMethod))
            {
                crme_method = callingMethod + ": " + method;
            }
            else
            {
                crme_method = method;
            }

            try
            {
                CreateCRMLogEntryRequest logRequestStart = new CreateCRMLogEntryRequest()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    OrganizationName = org,
                    UserId = userId,
                    crme_Name = string.Format("Exception: {0}:{1}", "Error in ", method),
                    crme_ErrorMessage = message,
                    crme_Debug = false,
                    crme_GranularTiming = false,
                    crme_TransactionTiming = false,
                    crme_Method = crme_method,
                    crme_LogLevel = (int)LogLevel.Error,
                    crme_Sequence = 1,
                    NameofDebugSettingsField = configFieldName
                };

                CreateCRMLogEntryResponse logResponse = SendReceive<CreateCRMLogEntryResponse>(baseUri, Utility.CreateCRMLogEntryRequest, logRequestStart, null);
            }
            catch (Exception) { }

        }

        /// <summary>
        /// Log Error
        /// </summary>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="org">CRM Organization</param>
        /// <param name="configFieldName">Not Used: Future On/Off switch</param>
        /// <param name="userId">Calling UserId</param>
        /// <param name="method">Child Calling Method or Sub Procedure</param>
        /// <param name="ex">Exception to log</param>
        /// <param name="callingMethod">Parent Calling Method</param>
        public static void LogError(Uri baseUri, string org, string configFieldName, Guid userId, string method, Exception ex, string callingMethod=null)
        {
            string stackTrace = StackTraceToString(ex);
            LogError(baseUri, org, configFieldName, userId, method, stackTrace, callingMethod);
        }

        /// <summary>
        /// Log Error
        /// </summary>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="logSettings">LogSettings Object</param>
        /// <param name="method">Calling Method</param>
        /// <param name="message">Error Message</param>
        public static void LogError(Uri baseUri, LogSettings logSettings, string method, string message)
        {
            LogError(baseUri, logSettings.Org, logSettings.ConfigFieldName, logSettings.UserId, method, message, logSettings.callingMethod);
        }

        /// <summary>
        /// Log Error
        /// </summary>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="logSettings">LogSettings Object</param>
        /// <param name="method">Calling Method</param>
        /// <param name="ex">Exception to log</param>
        public static void LogError(Uri baseUri, LogSettings logSettings, string method, Exception ex)
        {
            LogError(baseUri, logSettings.Org, logSettings.ConfigFieldName, logSettings.UserId, method, ex, logSettings.callingMethod);
        }

        /// <summary>
        /// Log Debug
        /// </summary>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="org">CRM Organization</param>
        /// <param name="configFieldName">Not Used: Future On/Off switch</param>
        /// <param name="userId">Calling UserId</param>
        /// <param name="method">Calling Method</param>
        /// <param name="message">Debug Message</param>
        public static void LogDebug(Uri baseUri, string org, string configFieldName, Guid userId, string method, string message, string callingMethod=null)
        {
            string crme_method;
            if (!string.IsNullOrEmpty(callingMethod))
            {
                crme_method = callingMethod + ": " + method;
            }
            else
            {
                crme_method = method;
            }

            try
            {
                CreateCRMLogEntryRequest logRequestStart = new CreateCRMLogEntryRequest()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    OrganizationName = org,
                    UserId = userId,
                    crme_Name = string.Format("Debug: {0}", method),
                    crme_ErrorMessage = message,
                    crme_Debug = true,
                    crme_GranularTiming = false,
                    crme_TransactionTiming = false,
                    crme_Method = crme_method,
                    crme_LogLevel = (int)LogLevel.Debug,
                    crme_Sequence = 1,
                    NameofDebugSettingsField = configFieldName
                };

                CreateCRMLogEntryResponse logResponse = SendReceive<CreateCRMLogEntryResponse>(baseUri, Utility.CreateCRMLogEntryRequest, logRequestStart, null);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Log Debug
        /// </summary>
        /// <param name="baseUri">REST URI to the VIMT Server</param>
        /// <param name="logSettings">LogSettings Object</param>
        /// <param name="method">Calling Method</param>
        /// <param name="message">Message</param>
        public static void LogDebug(Uri baseUri, LogSettings logSettings, string method, string message)
        {
            LogDebug(baseUri, logSettings.Org, logSettings.ConfigFieldName, logSettings.UserId, method, message, logSettings.callingMethod);
        }

        /// <summary>
        /// concatentate message and stack traces for exceptions and subsequent innerexceptions.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string StackTraceToString(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            BuildStackTrace(ex, sb);
            return sb.ToString();
        }

        /// <summary>
        /// Recursive call to concatentate message and stack traces for exceptions and subsequent innerexceptions.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="sb">StringBuilder to append to.</param>
        private static void BuildStackTrace(Exception ex, StringBuilder sb)
        {
            sb.AppendLine("***************************");
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                BuildStackTrace(ex.InnerException, sb);
            }
        }
    }

    /// <summary>
    /// Log Settings: Contains the settings to log the information/errors to
    /// </summary>
    public class LogSettings
    {
        /// <summary>
        /// CRM Organization
        /// </summary>
        public string Org { get; set; }
        /// <summary>
        /// Future Use On/Off Switch
        /// </summary>
        public string ConfigFieldName { get; set; }
        /// <summary>
        /// Calling User
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Calling Method
        /// </summary>
        public string callingMethod { get; set; }
    }

    #region Log Messages

    public class CreateCRMLogEntryRequest
    {
        public string MessageId { get; set; }

        public string OrganizationName { get; set; }

        public Guid UserId { get; set; }

        public int crme_Sequence { get; set; }

        public string crme_Name { get; set; }

        public string NameofDebugSettingsField { get; set; }

        public string crme_ErrorMessage { get; set; }

        public string crme_Method { get; set; }

        public bool crme_GranularTiming { get; set; }

        public bool crme_TransactionTiming { get; set; }

        public bool crme_Debug { get; set; }

        public int crme_LogLevel { get; set; }

        public Guid crme_RelatedParentId { get; set; }

        public string crme_RelatedParentEntityName { get; set; }

        public string crme_RelatedParentFieldName { get; set; }

        public string crme_RelatedWebMethodName { get; set; }

        public string crme_TimeStart { get; set; }

        public string crme_TimeEnd { get; set; }

        public Decimal crme_Duration { get; set; }
    }

    public class CreateCRMLogEntryResponse
    {
        public string MessageId { get; set; }
        public Guid crme_loggingId { get; set; }
    }



    #endregion

    #region VRMRest Excptions
    public class VIMTTimeOutExeption : System.Exception
    {
        public VIMTTimeOutExeption()
            : base()
        {
        }

        public VIMTTimeOutExeption(string message)
            : base(message)
        {
        }

        public VIMTTimeOutExeption(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    #endregion


}
