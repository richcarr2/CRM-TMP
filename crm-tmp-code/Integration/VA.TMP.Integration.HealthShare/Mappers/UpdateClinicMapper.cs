using log4net;
using Microsoft.Xrm.Sdk;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.HealthShare.Mappers
{
    internal class UpdateClinicMapper
    {
        private readonly TmpHealthShareUpdateClinicRequestMessage _request;
        private readonly IOrganizationService _organizationService;
        private readonly ILog _logger;

        public UpdateClinicMapper(TmpHealthShareUpdateClinicRequestMessage request, IOrganizationService organizationService, ILog logger)
        {
            _request = request;
            _organizationService = organizationService;
            _logger = logger;
        }

        internal mcs_resource Map()
        {
            var resourceClinic = MappingResolver.ClinicResolver(_organizationService, _request.Institution.ToString(), _request.ClinicIen.ToString(), _request.StationNumber, _logger);

            var clinic = new mcs_resource
            {
                cvt_Institution = _request.Institution.ToString(),
                cvt_VISNText = _request.Visn.ToString(),
                cvt_primarystopcode = _request.PrimaryStopCode.ToString(),
                cvt_secondarystopcode = _request.SecondaryStopCode.ToString(),
                cvt_ien = _request.ClinicIen.ToString(),
                mcs_UserNameInput = _request.ClinicName,
                cvt_TreatingSpecialty = _request.TreatingSpecialty,
                cvt_ServiceText = _request.Service,
                cvt_DefaultProviderNameImport = _request.DefaultProviderName,
                cvt_DefaultProviderEmail = _request.DefaultProviderEmail,
                cvt_defaultproviderduz = _request.DefaultProviderId,
                cvt_StationNumber = _request.StationNumber,
                cvt_OverBookAllowed = _request.OverBookAllowed,
                mcs_Type = new OptionSetValue((int)mcs_resourcetype.VistaClinic),
                mcs_RelatedSiteId = MappingResolver.SiteResolver(_organizationService, _request.StationNumber, _request.ClinicIen.ToString(), _logger),
                cvt_defaultprovider = MappingResolver.DefaultProviderResolver(_organizationService, _request.DefaultProviderEmail, _logger),
                mcs_resourceId = resourceClinic?.mcs_resourceId,
            };

            return clinic;
        }
    }
}