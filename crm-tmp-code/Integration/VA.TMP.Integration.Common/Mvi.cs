using System;
using VEIS.Mvi.Messages;

namespace VA.TMP.Integration.Common
{
    public static class Mvi
    {
        /// <summary>
        /// Creates a Fake Proxy Add Success response.
        /// </summary>
        /// <param name="stationNumber">VistA station number.</param>
        /// <returns>AddPersonResponse</returns>
        public static AddPersonResponse CreateFakeSuccessResponse(string stationNumber)
        {
            return new AddPersonResponse
            {
                MessageId = Guid.NewGuid().ToString(),
                ExceptionOccured = false,
                Message = "Correlation Proxy Added to VistA",
                RawMviExceptionMessage = string.Empty,
                Acknowledgement = new Acknowledgement
                {
                    TypeCode = "AA",
                    TargetMessage = "1.2.840.114350.1.13.0.1.7.1.1",
                    AcknowledgementDetails = new[]
                    {
                        new AcknowledgementDetail
                        {
                            Code = new AcknowledgementDetailCode
                            {
                                CodeSystemName = "MVI",
                                DisplayName = "IEN",
                                Code = string.Format("7172149^PI^{0}^", stationNumber)
                            },
                            Text = "Correlation Proxy Added to VistA"
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Creates a Fake Proxy Add Failure response.
        /// </summary>
        /// <returns>AddPersonResponse.</returns>
        public static AddPersonResponse CreateFakeFailureResponse()
        {
            return new AddPersonResponse
            {
                //MessageId = Guid.NewGuid().ToString(),
                ExceptionOccured = false,
                Message = "Communication Failure with requested VistA Station",
                RawMviExceptionMessage = string.Empty,
                Acknowledgement = new Acknowledgement
                {
                    TypeCode = "AE",
                    TargetMessage = "1.2.840.114350.1.13.0.1.7.1.1",
                    AcknowledgementDetails = new[]
                    {
                        new AcknowledgementDetail
                        {
                            Code = new AcknowledgementDetailCode
                            {
                                DisplayName = "Internal System Error",
                                Code = "INTERR"
                            },
                            Text = "Communication Failure with requested VistA Station"
                        },
                        new AcknowledgementDetail
                        {
                            Code = new AcknowledgementDetailCode
                            {
                                CodeSystemName = string.Empty,
                                DisplayName = string.Empty,
                                Code = string.Empty
                            },
                            Text = "Communication Failure with requested VistA Station"
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Creates a Fake Proxy Add MVI DB is down response.
        /// </summary>
        /// <returns>AddPersonResponse.</returns>
        public static AddPersonResponse CreateFakeDbDownResponse()
        {
            return new AddPersonResponse
            {
                MessageId = Guid.NewGuid().ToString(),
                ExceptionOccured = false,
                Message = "MVI[S]:INVALID REQUEST",
                RawMviExceptionMessage = string.Empty,
                Acknowledgement = new Acknowledgement
                {
                    TypeCode = "AE",
                    TargetMessage = "1.2.840.114350.1.13.0.1.7.1.1",
                    AcknowledgementDetails = new[]
                    {
                        new AcknowledgementDetail
                        {
                            Code = new AcknowledgementDetailCode
                            {
                                DisplayName = "Internal System Error",
                                Code = "INTERR"
                            },
                            Text = "MVI[S]:INVALID REQUEST"
                        },
                        new AcknowledgementDetail
                        {
                            Code = new AcknowledgementDetailCode
                            {
                                CodeSystemName = string.Empty,
                                DisplayName = string.Empty,
                                Code = string.Empty
                            },
                            Text = "MVI[S]:INVALID REQUEST"
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Creates a Fake Proxy Add MVI Server is down response.
        /// </summary>
        /// <returns>AddPersonResponse.</returns>
        public static AddPersonResponse CreateFakeServerDownResponse()
        {
            return new AddPersonResponse
            {
                MessageId = Guid.NewGuid().ToString(),
                ExceptionOccured = true,
                Message = "An unexpected error occured during the Add Person Operation",
                RawMviExceptionMessage = "Internal Error (from server)",
            };
        }
    }
}