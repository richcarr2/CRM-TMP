namespace VA.TMP.Integration.Messages.Cerner
{
    public class TmpCernerOutboundResponseMessage
    {
      
        public string ExceptionMessage { get; set; }

        public string RequestMessage { get; set; }

        public string ResponseMessage { get; set; }

        public bool ExceptionOccured { get; set; }

        public int MessageProcessingTime { get; set; }

        public string ControlId { get; set; }
    }
}
