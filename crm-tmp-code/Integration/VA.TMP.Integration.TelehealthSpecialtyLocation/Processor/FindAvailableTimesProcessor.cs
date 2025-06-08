using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Helpers;
using VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Models;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.TelehealthSpecialtyLocation;

namespace VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Processor
{
    public class FindAvailableTimesProcessor
    {
        //Maximum number of Single Resource Ids that can be sent in the current single fetchXml query without exceed URL length limitations
        private const int MaxSingleResourceIds = 454;
        //Maximum number of Group Resource Ids that can be sent in the current group fetchXml query without exceed URL length limitations
        private const int MaxGroupResourceIds = 432;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMemoryCache _memoryCache;
        private long _checkPoint;
        private int _patientFacility = 0;
        private IOptions<ApplicationSettings> _settings;
        private Stopwatch _stopwatch;

        public FindAvailableTimesProcessor(ILoggerFactory logger, IMemoryCache memoryCache, IOptions<ApplicationSettings> settings, IConfiguration config)
        {
            _config = config;
            //_logger = logger;
            _logger = logger.CreateLogger<FindAvailableTimesProcessor>();
            _loggerFactory = logger;
            _memoryCache = memoryCache;
            _settings = settings;
            _logger.LogDebug("FindAvailableTimesProcessor Start Execution");
        }

        public async Task<TelehealthSpecialtyLocationsFindAvailableTimesResponse> ExecuteAsync(TelehealthSpecialtyLocationsFindAvailableTimesRequest message, Stopwatch stopwatch)
        {
            _stopwatch = stopwatch;
            var timeRemaining = _settings.Value.TimeOut - _stopwatch.Elapsed.TotalSeconds;
            _logger.LogDebug($"Find available remaining execution time: {timeRemaining}");
            var cancelTokenSrc = new CancellationTokenSource(TimeSpan.FromSeconds(timeRemaining));
            var helper = new RestHelper(_logger, _settings, _config);
            var token = await helper.GetTokenAsync(_settings.Value.AppId, _config["TMP_Client_Secret"], _settings.Value.Scope, _settings.Value.TenantId);
            var messageId = Guid.NewGuid().ToString();

            var encoder = UrlEncoder.Default;

            var spFetchXml = string.Format(_settings.Value.SchedulingPackagesFetchXml.Replace("\\", ""), message.PatientFacility, message.StopCode);
            var gpSpFetchXml = string.Format(_settings.Value.SchedulingPackagesGroupResourcesFetchXml.Replace("\\", ""), message.PatientFacility, message.StopCode);

            //Load the fetchXml query for retrieving the scheduling package data consisting of the Service Ids, Scheduling Package Ids, Participating Site Ids, Facility Name, Facility Time Zone, and Facility VISN
            var schedulingPackagesFetchXml = encoder.Encode(string.Format(_settings.Value.SchedulingPackagesFetchXml.Replace("\\", ""), message.PatientFacility, message.StopCode));
            var schedulingPackagesGroupResourcesFetchXml = encoder.Encode(string.Format(_settings.Value.SchedulingPackagesGroupResourcesFetchXml.Replace("\\", ""), message.PatientFacility, message.StopCode));

            var responseMessage = new SchedulingPackagesResponseMessage { SchedulingPackages = new List<ResourcePackage>() };
            var singleResourceRespMsg = new SchedulingPackagesResponseMessage { SchedulingPackages = new List<ResourcePackage>() };
            var groupResourceRespMsg = new SchedulingPackagesResponseMessage { SchedulingPackages = new List<ResourcePackage>() };
            var tasks = new List<Task<ParticipatingSitesResponseMessage>>();

            var schedulingPachagesUrl = $"{_settings.Value.BaseUrl}/api/data/{_settings.Value.ApiVersion}/cvt_resourcepackages?fetchXml={schedulingPackagesFetchXml}";

            try
            {
                _patientFacility = int.Parse(message.PatientFacility);

                //Retrieve the Scheduling Package data
                #region Retrieve Scheduling Package Data
                var key = $"{message.PatientFacility}-{message.StopCode}";
                var skipCount = 2;
                var singleRsSkipCount = MaxSingleResourceIds;
                var grpRsSkipCount = MaxGroupResourceIds;

                _logger.LogDebug($"CorrelationId: {messageId} - Calling Web API Url: {schedulingPachagesUrl}");
                singleResourceRespMsg = helper.GetAsync<SchedulingPackagesResponseMessage>(schedulingPachagesUrl, messageId, token).ConfigureAwait(false).GetAwaiter().GetResult();
                schedulingPachagesUrl = $"{_settings.Value.BaseUrl}/api/data/{_settings.Value.ApiVersion}/cvt_resourcepackages?fetchXml={schedulingPackagesGroupResourcesFetchXml}";
                messageId = Guid.NewGuid().ToString();
                _logger.LogDebug($"CorrelationId: {messageId} - Calling Web API Url: {schedulingPachagesUrl}");
                groupResourceRespMsg = helper.GetAsync<SchedulingPackagesResponseMessage>(schedulingPachagesUrl, messageId, token).ConfigureAwait(false).GetAwaiter().GetResult();

                if (singleResourceRespMsg == null && groupResourceRespMsg == null)
                {
                    throw new WebException("No Results Found");
                }

                if (!_memoryCache.TryGetValue(key, out responseMessage))
                {
                    if (responseMessage == null) responseMessage = new SchedulingPackagesResponseMessage { SchedulingPackages = new List<ResourcePackage>() };

                    //Initial Single Resource Scheduling Packages variable
                    var srResourcePackages = singleResourceRespMsg.SchedulingPackages.Distinct(new ResourcePackageComparer());
                    //Initial Group Resource Scheduling Packages variable
                    var grResourcePackages = groupResourceRespMsg.SchedulingPackages.Distinct(new ResourcePackageComparer());

                    //Get a List of ALL the Participating Site Ids which Single Resources that have an associated Service
                    var singleResourceParticipatingSiteIds = GetParticipatingSiteIdParams(srResourcePackages);

                    //Get a List of ALL the Participating Site Ids with Group Resources that have an associated Service
                    var groupResourceParticipatingSiteIds = GetParticipatingSiteIdParams(grResourcePackages);

                    //Initialize a variable that contains the total # of Participating Site Ids
                    var srTotalIds = singleResourceParticipatingSiteIds.Count;
                    var grTotalIds = groupResourceParticipatingSiteIds.Count;

                    //Initialize a  Id Counter to Zero (0)
                    var idCount = 0;

                    //Take the first 454 Patient Participating Site Ids for Single Resources
                    var singleResourceIds = singleResourceParticipatingSiteIds.Take(singleRsSkipCount);
                    //Take the first 432 Patient Participating Site Ids for Group Resources
                    var grpResourceIds = groupResourceParticipatingSiteIds.Take(grpRsSkipCount);

                    //Send requests for both Single and Group Resources
                    _logger.LogDebug($"Begin Resource Queries at: {_stopwatch.Elapsed}");
                    do
                    {
                        //Initial query for Participating Sites with single (non-group) resources
                        if (!singleResourceIds.Count().Equals(0))
                        {
                            var singleResourcesFetchXml = encoder.Encode(string.Format(_settings.Value.SingleResourcesFetchXml.Replace("\\", ""), string.Join("", singleResourceIds)));

                            var singleResourceUri = $"{_settings.Value.BaseUrl}/api/data/{_settings.Value.ApiVersion}/cvt_participatingsites?fetchXml={singleResourcesFetchXml}";

                            messageId = Guid.NewGuid().ToString();
                            _logger.LogDebug($"CorrelationId: {messageId} - Calling Web API Url: {singleResourceUri}");
                            tasks.Add(helper.GetAsync<ParticipatingSitesResponseMessage>(singleResourceUri, messageId, token));
                        }

                        //Initial query for Participating Sites with group resources
                        if (!grpResourceIds.Count().Equals(0))
                        {
                            var groupResourcesFetchXml = encoder.Encode(string.Format(_settings.Value.GroupResourcesFetchXml.Replace("\\", ""), string.Join("", grpResourceIds)));

                            var groupResourceUri = $"{_settings.Value.BaseUrl}/api/data/{_settings.Value.ApiVersion}/cvt_participatingsites?fetchXml={groupResourcesFetchXml}";

                            messageId = Guid.NewGuid().ToString();
                            _logger.LogDebug($"CorrelationId: {messageId} - Calling Web API Url: {groupResourceUri}");
                            tasks.Add(helper.GetAsync<ParticipatingSitesResponseMessage>(groupResourceUri, messageId, token));
                        }
                        //increment the id counter
                        idCount += singleResourceIds.Count() + grpResourceIds.Count();

                        //Get the next set of single resource ids
                        if (singleResourceIds.Count() > 0) singleResourceIds = singleResourceParticipatingSiteIds.Skip(singleRsSkipCount).Take(MaxSingleResourceIds);
                        //Get the next set of group resource ids
                        if (grpResourceIds.Count() > 0) grpResourceIds = groupResourceParticipatingSiteIds.Skip(grpRsSkipCount).Take(MaxGroupResourceIds);

                        singleRsSkipCount += MaxSingleResourceIds;
                        grpRsSkipCount += MaxGroupResourceIds;

                        if (singleRsSkipCount > singleResourceParticipatingSiteIds.Count()) singleRsSkipCount = singleResourceParticipatingSiteIds.Count();
                        if (grpRsSkipCount > groupResourceParticipatingSiteIds.Count()) grpRsSkipCount = groupResourceParticipatingSiteIds.Count();

                    } while (idCount < (srTotalIds + grTotalIds));

                    responseMessage.SchedulingPackages.AddRange(srResourcePackages);
                    responseMessage.SchedulingPackages.AddRange(grResourcePackages);

                    if (!tasks.Count.Equals(0))
                    {
                        //Process the queries when they have all completed
                        await Task.Factory.ContinueWhenAll(tasks.ToArray(), queryTasks =>
                        {
                            queryTasks.ToList().ForEach(queryTask =>
                            {
                                //Only process the queries that completed successfully
                                if (queryTask.Status == TaskStatus.RanToCompletion)
                                {
                                    //Add the results from the Participating Sites queries to their respective Scheduling Package
                                    _logger.LogDebug("Combine results");
                                    queryTask.Result?.ParticipatingSites?.ForEach((ps) =>
                                    {
                                        if (!responseMessage.SchedulingPackages.Count().Equals(0)) responseMessage.SchedulingPackages.Find((sp) => sp.SchedulingPackageId.Equals(ps.SchedulingPackageId)).ParticipatingSites.Add(ps);
                                    });

                                    queryTask.Dispose();
                                }
                                else
                                {
                                    _logger.LogError($"Scheduling Package Search failed with the following Error: {queryTask.Exception}");
                                }
                            });
                        });
                    }

                    tasks.Clear();

                    if (responseMessage.SchedulingPackages.Count.Equals(0))
                    {
                        throw new WebException("No Results Found");
                    }

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromMinutes(_settings.Value.SchedulingPackagesCacheDuration));

                    _memoryCache.CreateEntry(key);
                    _memoryCache.Set(key, responseMessage, cacheEntryOptions);
                }
                else
                {
                    _logger.LogDebug($"Retrieve Scheduling Packages from cache for key: {key}");
                }
                #endregion

                _logger.LogDebug($"Begin Processing results at: {_stopwatch.Elapsed}");
                var findAvailbleTimesParameters = ProcessResponseMessage(responseMessage, token);

                var findAvailableTimesUri = $"{_settings.Value.BaseUrl}/api/data/{_settings.Value.ApiVersion}/tmp_FindProviderAvailability";

                var reqMessages = new List<Task<FindAvailableTimesResults>>();

                //remove any duplicate results
                findAvailbleTimesParameters = findAvailbleTimesParameters.Distinct().ToList();
                var isPartialResult = false;

                #region Peform the Search for Available Times
                _checkPoint = _stopwatch.ElapsedMilliseconds;

                if (_stopwatch.Elapsed.TotalSeconds > _settings.Value.TimeOut)
                {
                    _logger.LogDebug("No results");
                    return await Task.FromResult(new TelehealthSpecialtyLocationsFindAvailableTimesResponse
                    {
                        ErrorMessage = "Timeout limit has been reached.  Try adjusting search parameters such as Duration and try again.",
                        Status = "NO RESULTS FOUND"
                    });
                }

                timeRemaining = _settings.Value.TimeOut - _stopwatch.Elapsed.TotalSeconds;
                _logger.LogDebug($"Find available remaining execution time: {timeRemaining}");
                cancelTokenSrc = new CancellationTokenSource(TimeSpan.FromSeconds(timeRemaining - 10));

                //build the search requests containing 2 sets of parameters each
                skipCount = 2;
                var findAvailableTimesParams = findAvailbleTimesParameters.Take(skipCount);

                //create an array of tasks containing the requests to run asynchronously
                _logger.LogDebug($"Begin Availability Searches at: {_stopwatch.Elapsed}");
                do
                {
                    var req = new FindAvailableTimesRequest
                    {
                        Parameters = findAvailableTimesParams.ToList(),
                        AvailabilityStartDate = message.DesiredDate,
                        EndDate = message.DesiredDate.AddDays(message.DateRange)
                    };

                    var reqMessage = new FindAvailableTimesMessage
                    {
                        SerializedQueryParameters = JsonSerializer.Serialize(req)
                    };

                    messageId = Guid.NewGuid().ToString();
                    _logger.LogDebug($"CorrelationId: {messageId} - Sending request to TMP Custom API: {findAvailableTimesUri} - Content: {JsonSerializer.Serialize(reqMessage)}");
                    reqMessages.Add(helper.PostAsync<FindAvailableTimesResults, FindAvailableTimesMessage>(findAvailableTimesUri, reqMessage, messageId, token, cancelTokenSrc.Token));
                    findAvailableTimesParams = findAvailbleTimesParameters.Skip(skipCount).Take(2);
                    skipCount += 2;
                    if (skipCount > findAvailbleTimesParameters.Count()) skipCount = findAvailbleTimesParameters.Count;
                } while (findAvailableTimesParams.Count() > 0);

                var findAvailableTimesResponseMessage = new FindAvailableTimesResponse { AvailableAppointmentTimes = new List<AvailableAppointmentTime>() };
                //Initial variable to track the completed search results
                var completedResults = new ConcurrentBag<FindAvailableTimesResults>();

                //add the search responses to the response message
                _checkPoint = _stopwatch.ElapsedMilliseconds;
                try
                {
                    ///Wait for a maximum of 90 seconds for all Custom API Calls to complete
                    var timeLimitForWait = TimeSpan.FromSeconds(90);
                    _logger.LogDebug($"Time Limit: {timeLimitForWait}");

                    var sw = new Stopwatch();
                    sw.Start();

                    var completedRequestMessages = new List<Task<FindAvailableTimesResults>>();
                    var completedTasks = new List<Task<FindAvailableTimesResults>>();

                    _logger.LogDebug($"Begin Checking for completed Searches at: {_stopwatch.Elapsed}");
                    do
                    {
                        Task.WaitAny(reqMessages.Where(rm => !rm.Status.Equals(TaskStatus.RanToCompletion)).ToArray(), cancelTokenSrc.Token);
                        _logger.LogDebug($"{completedRequestMessages.Count()} Searched completed at: {_stopwatch.Elapsed}");
                        completedRequestMessages = reqMessages.Where(rm => rm.Status.Equals(TaskStatus.RanToCompletion)).Except(completedTasks).ToList();
                        completedRequestMessages.ForEach(availabilityQuery =>
                        {
                            _logger.LogDebug($"Query Status: {availabilityQuery.Status}");
                            _logger.LogDebug($"CorrelationId: {availabilityQuery.Result.MessageId} - Query Status: {availabilityQuery.Status}");

                            //only process the responses that completed successfully
                            var findAvailableTimesResp = availabilityQuery != null && availabilityQuery.Result != null && !string.IsNullOrEmpty(availabilityQuery.Result.SerializedQueryResults)
                                ? JsonSerializer.Deserialize<FindAvailableTimesResponse>(availabilityQuery.Result.SerializedQueryResults)
                                : new FindAvailableTimesResponse() { ErrorMessage = "No Results Found" };

                            if (findAvailableTimesResp?.AvailableAppointmentTimes != null)
                            {
                                findAvailableTimesResponseMessage.AvailableAppointmentTimes.AddRange(findAvailableTimesResp.AvailableAppointmentTimes);
                            }
                            else
                            {
                                isPartialResult = findAvailableTimesResp != null && findAvailableTimesResp.ErrorMessage != null
                                    ? (bool)findAvailableTimesResp?.ErrorMessage?.Equals("No Results Found")
                                    : false;
                            }

                            availabilityQuery.Dispose();
                        });

                        completedTasks.AddRange(completedRequestMessages);
                        completedRequestMessages.Clear();

                    } while (sw.Elapsed.TotalSeconds < timeLimitForWait.TotalSeconds);

                    sw.Stop();
                }
                catch (OperationCanceledException e)
                {
                    _logger.LogDebug("Timeout limit has been reached");
                    _logger.LogError(e.ToString());
                }

                _logger.LogDebug($"Stop Checking for completed Searches at: {_stopwatch.Elapsed}");
                _logger.LogDebug($"Time it took in milliseconds to send requests to Custom API: {TimeSpan.FromMilliseconds(_stopwatch.ElapsedMilliseconds - _checkPoint)}");
                _logger.LogDebug($"Overall runtime so far:{_stopwatch.Elapsed}");
                _checkPoint = _stopwatch.ElapsedMilliseconds;

                reqMessages.Clear();
                #endregion

                _logger.LogDebug($"FindAvailableTimesProcessor Search Execution time: {TimeSpan.FromMilliseconds(_stopwatch.ElapsedMilliseconds - _checkPoint)}");
                _checkPoint = _stopwatch.ElapsedMilliseconds;

                _logger.LogDebug("Searches have completed");
                //convert the results and return it to the caller
                if (findAvailableTimesResponseMessage.AvailableAppointmentTimes != null && !findAvailableTimesResponseMessage.AvailableAppointmentTimes.Count.Equals(0))
                {
                    _logger.LogDebug($"Begin Process Search results at: {_stopwatch.Elapsed}");
                    _logger.LogDebug($"Overall runtime so far in total seconds:{_stopwatch.Elapsed.TotalSeconds}");
                    _logger.LogDebug($"Timeout Limit: {_settings.Value.TimeOut}");
                    var processTimeLeft = (_settings.Value.TimeOut - (int)_stopwatch.Elapsed.TotalSeconds) - 1;
                    _logger.LogDebug($"Remaining Processing Time: {processTimeLeft}");

                    //Initialize a new Cancellation Token to be used to prevent the processing of the Available Times Response from exceeding the timeout
                    var cancelSrc = _settings.Value.TimeOut > ((int)_stopwatch.Elapsed.TotalSeconds + 1)
                        ? new CancellationTokenSource(TimeSpan.FromSeconds(processTimeLeft))
                        : new CancellationTokenSource(1);

                    //Gracefully handle the execution exceeding the allowable timeout
                    var availableTimes = _settings.Value.TimeOut > ((int)_stopwatch.Elapsed.TotalSeconds + 1)
                        ? ProcessAvailableTimesResponse(findAvailbleTimesParameters, findAvailableTimesResponseMessage, cancelSrc.Token)
                        : new List<AvailableTime>();
                    _logger.LogDebug($"Results found: {availableTimes.Count()}");
                    _logger.LogDebug($"Time it took to process Custom API Responses: {TimeSpan.FromMilliseconds(_stopwatch.ElapsedMilliseconds - _checkPoint)}");
                    _logger.LogDebug($"Overall runtime so far:{_stopwatch.Elapsed}");

                    if (isPartialResult)
                    {
                        return await Task.FromResult(new TelehealthSpecialtyLocationsFindAvailableTimesResponse
                        {
                            AvailableTimes = availableTimes,
                            ErrorMessage = "Partial results due to time out limit reached.",
                            ErrorOccurred = true,
                            Status = "PARTIAL RESULT"
                        });
                    }
                    return await Task.FromResult(new TelehealthSpecialtyLocationsFindAvailableTimesResponse { AvailableTimes = availableTimes });
                }
                else
                {
                    _logger.LogDebug("No results");
                    return await Task.FromResult(new TelehealthSpecialtyLocationsFindAvailableTimesResponse
                    {
                        ErrorMessage = "No Results Available.",
                        Status = "NO RESULTS FOUND"
                    });
                }
            }
            catch (System.AggregateException ae)
            {
                _logger.LogError(ae.ToString());
                return await Task.FromResult(new TelehealthSpecialtyLocationsFindAvailableTimesResponse
                {
                    ErrorMessage = "Reqest failed due to time out limit reached. No Results Available.",
                    ErrorOccurred = true,
                    Status = "FAILED"
                });
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.ToString());
                if (e.InnerException != null && e.InnerException is TimeoutException)
                    throw;

                throw new WebException(e.Message);
            }
            finally
            {
                _logger.LogDebug($"Processing Search Results time: {_stopwatch.ElapsedMilliseconds - _checkPoint}");
                _logger.LogDebug($"FindAvailableTimesProcessor Execution time: {_stopwatch.Elapsed}");
                cancelTokenSrc.Dispose();
            }
        }

        /// <summary>
        /// Convert the search results into Available Times
        /// </summary>
        /// <param name="findAvailbleTimesParameters"></param>
        /// <param name="findAvailableTimesResponseMessage"></param>
        /// <returns></returns>
        private List<AvailableTime> ProcessAvailableTimesResponse(List<FindAvailableTimesParameter> findAvailbleTimesParameters, FindAvailableTimesResponse findAvailableTimesResponseMessage, CancellationToken token)
        {
            _logger.LogDebug("Begin Processing Available Times Response");
            var availableTimes = new ConcurrentBag<AvailableTime>();

            try
            {
                Parallel.ForEach(findAvailableTimesResponseMessage.AvailableAppointmentTimes.Where(apt => !string.IsNullOrEmpty(apt.AvailabilityStartDate) &&
                    findAvailbleTimesParameters.Any(atp => apt.SiteId.Equals(apt.SiteId))).ToList(), new ParallelOptions { CancellationToken = token }, (availableTime) =>
                    {
                        availableTimes.Add(new AvailableTime
                        {
                            Clinic = availableTime.ClinicName,
                            Date = DateTime.Parse(availableTime.AvailabilityStartDate).ToString("yyyy-MM-dd"),
                            Duration = availableTime.Duration,
                            Facility = string.IsNullOrEmpty(availableTime.Facility) ? findAvailbleTimesParameters.FirstOrDefault(p => p.SiteId.Equals(availableTime.SiteId)).FacilityName : availableTime.Facility,
                            GroupAppointment = availableTime.GroupAppointment,
                            PatientLocationType = availableTime.PatientLocationType,
                            Modality = availableTime.SchedulingPackageModality,
                            PatientFacilityTimeZone = availableTime.PatientFacilityTimeZone,
                            ProviderFacilityStationName = availableTime.ProviderFacilityStationName,
                            ProviderFacilityStationNumber = availableTime.ProviderFacilityStationNumber,
                            ProviderFacilityTimeZone = availableTime.ProviderFacilityTimeZone,
                            Provider = availableTime.ProviderName,
                            SchedulingPackageName = availableTime.SchedulingPackageName,
                            Time = DateTime.Parse(availableTime.AvailabilityStartDate).ToString("HH:mm:ssZ"),
                            VISN = availableTime.VISN
                        });
                    });
            }
            catch (Exception e)
            {
                _logger.LogDebug("Timeout exceeded for processing available times");
                _logger.LogError(e.ToString());
            }

            _logger.LogDebug($"Results found: {availableTimes.Count}");
            return availableTimes.Distinct(new AvailableTimeComparer()).OrderBy((at) => at.Date).ThenBy((at) => at.Time).ToList();
        }

        /// <summary>
        /// Convert the Scheduling Package query results into search parameters
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        private List<FindAvailableTimesParameter> ProcessResponseMessage(SchedulingPackagesResponseMessage responseMessage, string token)
        {
            var helper = new RestHelper(_logger, _settings, _config);
            var parameters = new ConcurrentBag<FindAvailableTimesParameter>();
            var providerFacilityName = responseMessage.SchedulingPackages.Select(sp => sp.ProviderFacilityName).Where(facilityName => !string.IsNullOrEmpty(facilityName)).FirstOrDefault();
            var patientFacilityTimeZoneCode = responseMessage.SchedulingPackages.Select(sp => sp.PatientFacilityTimeZoneCode).Where(timeZoneCode => timeZoneCode > 0).FirstOrDefault();

            if (patientFacilityTimeZoneCode.Equals(0))
            {
                patientFacilityTimeZoneCode = GetPatientTimeZoneCodeByStationNumber(helper, token);
            }

            var facilityIds = responseMessage.SchedulingPackages.Select(sp => $"<value>{sp.ProviderFacilityId}</value>").Where(pfId => !Equals(pfId, Guid.Empty)).Distinct().ToList();

            //Initialize the memory cache with all the VISNs
            if (!facilityIds.Count.Equals(0))
            {
                GetAllFacilityVisns(facilityIds, helper, token);
            }

            //responseMessage.SchedulingPackages.Select(sp => sp.ProviderFacilityId).Where(pfId => !Equals(pfId, Guid.Empty)).ToList().ForEach(fId =>
            //{
            //    //Initialize the memory cache with all the VISNs
            //    GetVisnByFacilityId(fId, token, helper);
            //});

            _logger.LogDebug($"Unfiltered Scheduling Packages Count: {responseMessage.SchedulingPackages.Count}");

            var filteredSchedulingPackages = responseMessage.SchedulingPackages.FindAll(sp => sp.PatientParticipatingSiteServiceId != null || sp.ProviderParticipatingSiteServiceId != null || sp.SchedulingPackageServiceId != null);

            Parallel.ForEach(responseMessage.SchedulingPackages.FindAll(sp => sp.PatientParticipatingSiteServiceId != null || sp.ProviderParticipatingSiteServiceId != null || sp.SchedulingPackageServiceId != null), (sp) =>
            {
                sp.ParticipatingSites.AsParallel().ForAll((ps) =>
                {
                    var visn = GetVisnByFacilityId(sp.ProviderFacilityId, token, helper);
                    //Do not create a parameter if there is no Clinic
                    if (ps.SingleResourceClinicId.HasValue || ps.SingleRelatedResourceClinicId.HasValue || (ps.GroupRelatedResourceClinicId.HasValue && ps.GroupResourceRelatedResourceType.Equals(251920000) && ps.GroupRelatedResourceClinicId.HasValue))
                    {
                        //Get the Provider for the Participating Site
                        var providerId = GetProvider(sp);
                        //Do not create a search parameter if there is no Provider
                        if (!providerId.Equals(Guid.Empty))
                        {
                            if (sp.PatientParticipatingSiteServiceId.HasValue)
                            {
                                parameters.Add(new FindAvailableTimesParameter
                                {
                                    ClinicId = GetClinicId(ps),
                                    ClinicName = GetClinicName(ps),
                                    FacilityName = string.IsNullOrEmpty(sp.ProviderFacilityName) ? providerFacilityName : sp.ProviderFacilityName,
                                    FacilityTimeZoneCode = sp.PatientFacilityTimeZoneCode.Equals(0) ? patientFacilityTimeZoneCode : sp.PatientFacilityTimeZoneCode,
                                    GroupAppointment = sp.GroupAppointment,
                                    PatientLocationType = sp.PatientLocationType,
                                    ProviderId = providerId,
                                    ProviderFacilityTimeZoneCode = sp.ProviderFacilityTimeZoneCode,
                                    ProviderFacilityStationName = string.IsNullOrEmpty(sp.ProviderFacilityName) ? providerFacilityName : sp.ProviderFacilityName,
                                    ProviderFacilityStationNumber = sp.ProviderFacilityStationNumber,
                                    ServiceId = sp.PatientParticipatingSiteServiceId.Value,
                                    ServiceSpecId = sp.PatientParticipatingSiteServiceSpecId.Value,
                                    SchedulingPackageModality = GetModalityByCode(sp.SchedulingPackageModalityCode),
                                    SchedulingPackageName = sp.SchedulingPackageName,
                                    SiteName = ps.RelatedActualSiteName,
                                    SiteId = ps.RelatedActualSiteId,
                                    VISN = string.IsNullOrEmpty(sp.ProviderFacilityVISN) ? GetVisnByFacilityId(sp.ProviderFacilityId, token, helper) : sp.ProviderFacilityVISN
                                });
                            }
                            if (sp.ProviderParticipatingSiteServiceId.HasValue)
                            {
                                parameters.Add(new FindAvailableTimesParameter
                                {
                                    ClinicId = GetClinicId(ps),
                                    ClinicName = GetClinicName(ps),
                                    FacilityName = string.IsNullOrEmpty(sp.ProviderFacilityName) ? providerFacilityName : sp.ProviderFacilityName,
                                    FacilityTimeZoneCode = sp.PatientFacilityTimeZoneCode.Equals(0) ? patientFacilityTimeZoneCode : sp.PatientFacilityTimeZoneCode,
                                    GroupAppointment = sp.GroupAppointment,
                                    PatientLocationType = sp.PatientLocationType,
                                    ProviderId = providerId,
                                    ProviderFacilityTimeZoneCode = sp.ProviderFacilityTimeZoneCode,
                                    ProviderFacilityStationName = string.IsNullOrEmpty(sp.ProviderFacilityName) ? providerFacilityName : sp.ProviderFacilityName,
                                    ProviderFacilityStationNumber = sp.ProviderFacilityStationNumber,
                                    ServiceId = sp.ProviderParticipatingSiteServiceId.Value,
                                    ServiceSpecId = sp.ProviderParticipatingSiteServiceSpecId.Value,
                                    SchedulingPackageModality = GetModalityByCode(sp.SchedulingPackageModalityCode),
                                    SchedulingPackageName = sp.SchedulingPackageName,
                                    SiteName = ps.RelatedActualSiteName,
                                    SiteId = ps.RelatedActualSiteId,
                                    VISN = string.IsNullOrEmpty(sp.ProviderFacilityVISN) ? GetVisnByFacilityId(sp.ProviderFacilityId, token, helper) : sp.ProviderFacilityVISN
                                });
                            }
                            if (sp.SchedulingPackageServiceId.HasValue)
                            {
                                parameters.Add(new FindAvailableTimesParameter
                                {
                                    ClinicId = GetClinicId(ps),
                                    ClinicName = GetClinicName(ps),
                                    FacilityName = string.IsNullOrEmpty(sp.ProviderFacilityName) ? providerFacilityName : sp.ProviderFacilityName,
                                    FacilityTimeZoneCode = sp.PatientFacilityTimeZoneCode.Equals(0) ? patientFacilityTimeZoneCode : sp.PatientFacilityTimeZoneCode,
                                    GroupAppointment = sp.GroupAppointment,
                                    PatientLocationType = sp.PatientLocationType,
                                    ProviderId = providerId,
                                    ProviderFacilityTimeZoneCode = sp.ProviderFacilityTimeZoneCode,
                                    ProviderFacilityStationName = string.IsNullOrEmpty(sp.ProviderFacilityName) ? providerFacilityName : sp.ProviderFacilityName,
                                    ProviderFacilityStationNumber = sp.ProviderFacilityStationNumber,
                                    ServiceId = sp.SchedulingPackageServiceId.Value,
                                    ServiceSpecId = sp.SchedulingPackageServiceSpecId.Value,
                                    SchedulingPackageModality = GetModalityByCode(sp.SchedulingPackageModalityCode),
                                    SchedulingPackageName = sp.SchedulingPackageName,
                                    SiteName = ps.RelatedActualSiteName,
                                    SiteId = ps.RelatedActualSiteId,
                                    VISN = string.IsNullOrEmpty(sp.ProviderFacilityVISN) ? GetVisnByFacilityId(sp.ProviderFacilityId, token, helper) : sp.ProviderFacilityVISN
                                });
                            }
                        }
                    }
                });
            });

            return parameters.ToList();
        }

        private void GetAllFacilityVisns(List<string> facilityIds, RestHelper helper, string token)
        {
            var fetchXml = string.Format($"<fetch><entity name=\"mcs_facility\"><filter><condition attribute=\"mcs_facilityid\" operator=\"in\" uitype=\"mcs_facility\">{string.Join("", facilityIds)}</condition></filter><link-entity name=\"businessunit\" from=\"businessunitid\" to=\"mcs_visn\" alias=\"VISN\"><attribute name=\"name\"/></link-entity></entity></fetch>");
            var messageId = Guid.NewGuid().ToString();
            var uri = $"{_settings.Value.BaseUrl}/api/data/{_settings.Value.ApiVersion}/mcs_facilities?fetchXml={fetchXml}";
            _logger.LogDebug($"CorrelationId: {messageId} - Get Visn By Facility Id FetchXml Request");
            _logger.LogDebug(uri);

            var facilityResp = helper.GetAsync<FacilityResponse>(uri, messageId, token).ConfigureAwait(false).GetAwaiter().GetResult();

            if (facilityResp != null && facilityResp.Facilities != null && !facilityResp.Facilities.Count.Equals(0))
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromSeconds(_settings.Value.VISNCacheDuration));

                facilityResp.Facilities.ForEach(facility =>
                {
                    _memoryCache.CreateEntry(facility.Id);
                    _memoryCache.Set(facility.Id, facility.Visn, cacheEntryOptions);
                    _logger.LogDebug($"VISN Added to cache for Facility Id: {facility.Id}");
                });
            }
        }

        private Guid GetClinicId(ParticipatingSite ps)
        {
            var clinicId = Guid.Empty;

            if (ps.SingleResourceClinicId.HasValue || ps.SingleRelatedResourceClinicId.HasValue)
            {
                clinicId = ps.SingleResourceClinicId ?? ps.SingleRelatedResourceClinicId.Value;
            }
            else
            {
                if (ps.GroupResourceRelatedResourceType.Equals(251920000) && ps.GroupRelatedResourceClinicId.HasValue)
                {
                    clinicId = ps.GroupRelatedResourceClinicId.Value;
                }
            }

            return clinicId;
        }

        private string GetClinicName(ParticipatingSite ps)
        {
            var clinicName = string.Empty;

            if (ps.SingleResourceClinicId.HasValue)
            {
                clinicName = ps.SingleResourceClinicName;
            }
            else
            {
                if (ps.GroupResourceRelatedResourceType.Equals(251920000) && ps.ResourceGroupId.HasValue)
                {
                    clinicName = ps.GroupResourceRelatedClinicName;
                }
            }

            return clinicName;
        }

        /// <summary>
        /// Convert the Modality Code to it's Name
        /// </summary>
        /// <param name="schedulingPackageModalityCode"></param>
        /// <returns></returns>
        private string GetModalityByCode(int schedulingPackageModalityCode)
        {
            string modality;
            switch (schedulingPackageModalityCode)
            {
                case 917290000:
                    modality = "Clinical Video Telehealth";
                    break;
                case 917290001:
                    modality = "Store and Forward";
                    break;
                case 917290002:
                    modality = "Telephone";
                    break;
                default:
                    modality = "Unknown";
                    break;
            }

            return modality;
        }

        private List<string> GetParticipatingSiteIdParams(IEnumerable<ResourcePackage> srResourcePackages)
        {
            var participatingSiteIds = new List<string>();

            srResourcePackages.ToList().ForEach(sp =>
            {
                //Only run the query if either the Scheduling Package or the Patient Participating Site have a Service Id
                if (!sp.PatientPariticipatingSiteId.Equals(Guid.Empty) && (!sp.PatientParticipatingSiteServiceId.Equals(Guid.Empty) || !sp.SchedulingPackageServiceId.Equals(Guid.Empty)))
                {
                    participatingSiteIds.Add($"<value>{sp.PatientPariticipatingSiteId}</value>");
                }
                //Only run the query if the Provider Participating Site has a Service Id
                if (!sp.ProviderPariticipatingSiteId.Equals(Guid.Empty) && !sp.ProviderParticipatingSiteServiceId.Equals(Guid.Empty))
                {
                    participatingSiteIds.Add($"<value>{sp.ProviderPariticipatingSiteId}</value>");
                }
            });

            return participatingSiteIds;
        }

        private int GetPatientTimeZoneCodeByStationNumber(RestHelper helper, string token)
        {
            var fetchXml = $"<fetch><entity name=\"mcs_facility\"><attribute name=\"mcs_timezone\" /><filter><condition attribute=\"mcs_stationnumber\" operator=\"eq\" value=\"{_patientFacility}\" /></filter></entity></fetch>";
            var messageId = Guid.NewGuid().ToString();
            var uri = $"{_settings.Value.BaseUrl}/api/data/{_settings.Value.ApiVersion}/mcs_facilities?fetchXml={fetchXml}";
            _logger.LogDebug($"CorrelationId: {messageId} - Get Facility by Station Number FetchXml Request");
            _logger.LogDebug(uri);

            var facilityResp = helper.GetAsync<FacilityResponse>(uri, messageId, token).ConfigureAwait(false).GetAwaiter().GetResult();

            var timeZoneCode = facilityResp != null && facilityResp.Facilities != null && !facilityResp.Facilities.Count.Equals(0) ? facilityResp.Facilities.First().Timezone : 0;
            return timeZoneCode;
        }

        private Guid GetProvider(ResourcePackage sp)
        {
            var providerId = Guid.Empty;

            sp.ParticipatingSites.ForEach(ps =>
            {
                if (providerId.Equals(Guid.Empty))
                {
                    if (!ps.SingleResourceUserId.Equals(Guid.Empty))
                    {
                        providerId = !ps.SingleResourceUserId.Equals(Guid.Empty)
                            ? ps.SingleResourceUserId
                            : Guid.Empty;
                    }
                    else
                    {
                        providerId = !ps.GroupRelatedUserId.Equals(Guid.Empty)
                                ? ps.GroupRelatedUserId
                                : !ps.GroupResourceRelatedUserId.Equals(Guid.Empty)
                                    ? ps.GroupResourceRelatedUserId
                                    : !ps.ResourceRelatedUserId.Equals(Guid.Empty)
                                        ? ps.ResourceRelatedUserId
                                        : Guid.Empty;
                    }

                    if (!providerId.Equals(Guid.Empty))
                    {
                        _logger.LogDebug($"Provider Id: {providerId}");
                    }
                }
            });

            return providerId;
        }

        private string GetVisnByFacilityId(Guid facilityId, string token, RestHelper helper)
        {
            var visn = string.Empty;

            if (!facilityId.Equals(Guid.Empty))
            {
                if (!_memoryCache.TryGetValue(facilityId, out visn))
                {
                    var fetchXml = string.Format("<fetch><entity name=\"mcs_facility\"><filter><condition attribute=\"mcs_facilityid\" operator=\"eq\" value=\"{0}\" uitype=\"mcs_facility\"/></filter><link-entity name=\"businessunit\" from=\"businessunitid\" to=\"mcs_visn\" alias=\"VISN\"><attribute name=\"name\"/></link-entity></entity></fetch>", facilityId);
                    var messageId = Guid.NewGuid().ToString();
                    var uri = $"{_settings.Value.BaseUrl}/api/data/{_settings.Value.ApiVersion}/mcs_facilities?fetchXml={fetchXml}";
                    _logger.LogDebug($"CorrelationId: {messageId} - Get Visn By Facility Id FetchXml Request");
                    _logger.LogDebug(uri);

                    var facilityResp = helper.GetAsync<FacilityResponse>(uri, messageId, token).ConfigureAwait(false).GetAwaiter().GetResult();

                    visn = facilityResp != null && facilityResp.Facilities != null && !facilityResp.Facilities.Count.Equals(0) ? facilityResp.Facilities.First().Visn : string.Empty;

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetSlidingExpiration(TimeSpan.FromSeconds(_settings.Value.VISNCacheDuration));

                    _memoryCache.CreateEntry(facilityId);
                    _memoryCache.Set(facilityId, visn, cacheEntryOptions);
                    _logger.LogDebug($"VISN Added to cache for Facility Id: {facilityId}");
                }
                else
                {
                    _logger.LogDebug($"VISN retrieved from cache for Facility Id: {facilityId}");
                }
            }

            return visn;
        }

        [DataContract]
        class FindAvailableTimesMessage
        {
            public string SerializedQueryParameters { get; set; }
        }

        class FindAvailableTimesResults : TmpBaseResponseMessage
        {
            [JsonPropertyName("@odata.context")]
            public string Context { get; set; }
            public string SerializedQueryResults { get; set; }
        }
    }
}
