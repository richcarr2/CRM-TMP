using System;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.Mappers;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.PersonSearch
{
    /// <summary>
    /// Map Person Search to Attended Search step.
    /// </summary>
    public class MapPersonSearchToAttendedSearchStep : IFilter<PersonSearchStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapPersonSearchToAttendedSearchStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(PersonSearchStateObject state)
        {
            if (state.IsAttended)
            {
                state.AttendedSearchRequest = new PersonSearchMapper(state).Map();
            }
            else
            {
                state.UnattendedSearchRequest = new PersonSearchMapper(state).MapUnattended();
            }
            state.SerializedInstance = $"PersonSearch Response {Environment.NewLine},{Serialization.DataContractSerialize(state.PersonSearchResponseMessage)}";
        }
    }
}
