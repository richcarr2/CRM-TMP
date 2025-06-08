using System.Diagnostics;
using System.Linq;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.Helpers;
using VA.TMP.Integration.Mvi.StateObject;
using VA.TMP.Integration.Rest.Interface;
using VEIS.Mvi.Messages;

namespace VA.TMP.Integration.Mvi.PipelineSteps.PersonSearch
{
    /// <summary>
    /// Send Person Search to EC step.
    /// </summary>
    public class SendPersonSearchToEcStep : IFilter<PersonSearchStateObject>
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcPersonSearchUri => _settings.Items.First(x => x.Key == "EcPersonSearchUri").Value;

        private string EcUnattendedPersonSearchUri => _settings.Items.First(x => x.Key == "EcUnattendedPersonSearchUri").Value;

        private string MviOrgName => _settings.Items.First(x => x.Key == "MviOrgName").Value;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SendPersonSearchToEcStep(ILog logger, IServicePost servicePost, Settings settings)
        {
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(PersonSearchStateObject state)
        {
            SendPersonSearchToEc(state);
        }

        /// <summary>
        /// Send request to EC.
        /// </summary>
        /// <param name="state">State.</param>
        /// <returns></returns>
        private RetrieveOrSearchPersonResponse SendPersonSearchToEc(PersonSearchStateObject state)
        {
            if (state.IsAttended) state.AttendedSearchRequest.OrganizationName = MviOrgName;
            else state.UnattendedSearchRequest.OrganizationName = MviOrgName;

            RetrieveOrSearchPersonResponse response = null;

            switch (state.PersonSearchFakeResponseType)
            {
                case "0":
                    response = PersonSearchUtilities.CreateFakeSuccess0Result();
                    break;
                case "1":
                    response = PersonSearchUtilities.CreateFakeSuccess1Result(state, _logger);
                    break;
                case "2":
                    response = PersonSearchUtilities.CreateFakeSuccessManyResults(state);
                    break;
                case "3":
                    response = PersonSearchUtilities.CreateFakeEdipiResult(state);
                    break;
                default:
                    var timer = new Stopwatch();
                    timer.Start();

                    if (state.IsAttended)
                    {
                        response = _servicePost.PostToEc<AttendedSearchRequest, RetrieveOrSearchPersonResponse>(
                        "MVI Person Search", EcPersonSearchUri, _settings, state.AttendedSearchRequest).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    else
                    {
                        response = _servicePost.PostToEc<UnattendedSearchRequest, RetrieveOrSearchPersonResponse>(
                        "MVI Unattended Person Search", EcUnattendedPersonSearchUri, _settings, state.UnattendedSearchRequest).ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    timer.Stop();
                    state.EcProcessingTimeMs += (int)timer.ElapsedMilliseconds;
                    break;
            }

            if (state.IsAttended)
            {
                if (response == null) _logger.Error("Response returned from Attended Person Search is null");
                else _logger.Info("Response returned from Attended Person Search is " + Serialization.DataContractSerialize(response));
            }
            else
            {
                if (response == null) _logger.Error("Response returned from Unattended Person Search is null");
                else _logger.Info("Response returned from Unattended Person Search is " + Serialization.DataContractSerialize(response));
            }

            state.RetrieveOrSearchPersonResponse = response;

            if (response == null)
            {
                ErrorOccurred(state, "No Response was receieved from Person Search");
            }
            else if (response.Acknowledgement == null)
            {
                ErrorOccurred(state, "No Acknowledgement was receieved in the Person Search Response");
            }
            else if (response.Acknowledgement.TypeCode == null)
            {
                ErrorOccurred(state, "Acknowledgment Type Code is null");
            }
            else
            {
                switch (response.Acknowledgement.TypeCode)
                {
                    case "AA":
                        if (response.QueryAcknowledgement.QueryResponseCode == "NF" || response.QueryAcknowledgement.QueryResponseCode == "QE") state.ExceptionMessage = response.QueryAcknowledgement.QueryResponseCode;
                        break;
                    case "AR":
                    case "AE":
                        if (response.Acknowledgement.AcknowledgementDetails == null || response.Acknowledgement.AcknowledgementDetails.Length == 0)
                        {
                            ErrorOccurred(state, "No Acknowledgment Details Listed in Person Search Fail Response");
                        }
                        else
                        {
                            var ack2 = response.Acknowledgement.AcknowledgementDetails.FirstOrDefault();
                            if (ack2 != null && ack2.Code != null)
                            {
                                if (!string.IsNullOrEmpty(ack2.Code.Code))
                                {
                                    var exceptionMessage = string.Format("Person Search failed with the following: {0} - {1} - {2}", ack2.Code.Code, ack2.Code.DisplayName, ack2.Text);
                                    ErrorOccurred(state, exceptionMessage);
                                }
                                else
                                {
                                    ErrorOccurred(state, "No Acknowledgment Code Listed in Person Search Fail Response");
                                }
                            }
                            else
                            {
                                ErrorOccurred(state, "Acknowledgment or Acknowledgment Code Fail is null");
                            }
                        }
                        break;
                }
            }

            return response;
        }

        /// <summary>
        /// Logs and set state on an exception.
        /// </summary>
        /// <param name="state">State.</param>
        /// <param name="errorMessage">Error Message.</param>
        private void ErrorOccurred(PersonSearchStateObject state, string errorMessage)
        {
            state.ExceptionOccured = true;
            state.ExceptionMessage = errorMessage;
            _logger.Error(errorMessage);
        }
    }
}
