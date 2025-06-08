--Conditions
--All Users or specific user
--No Sub Type option selected
--H/M - then Patient Site can be null
--Group - then Patient Site can be null, but must match Facility

--Combinations:
--1.	All Users		No Subtype selected			No H/M		No Group
--2.	All Users		No Subtype not selected		No H/M		No Group
--3.	All Users		No Subtype selected			Yes H/M		No Group
--4.	All Users		No Subtype not selected		Yes H/M		No Group
--5.	All Users		No Subtype selected			Yes H/M		Yes Group
--6.	All Users		No Subtype not selected		Yes H/M		Yes Group
--7.	All Users		No Subtype selected			No H/M		Yes Group
--8.	All Users		No Subtype not selected		No H/M		Yes Group

--9.	Specific User	No Subtype selected			No H/M		No Group
--10.	Specific User	No Subtype not selected		No H/M		No Group
--11.	Specific User	No Subtype selected			Yes H/M		No Group
--12.	Specific User	No Subtype not selected		Yes H/M		No Group
--13.	Specific User	No Subtype selected			Yes H/M		Yes Group
--14.	Specific User	No Subtype not selected		Yes H/M		Yes Group
--15.	Specific User	No Subtype selected			No H/M		Yes Group
--16.	Specific User	No Subtype not selected		No H/M		Yes Group


--1.	All Users		No Subtype selected			No H/M		No Group
if (' All Users' in ( @User ) AND ('00000000-0000-0000-0000-000000000000' in ( @SpecialtySubType )) AND 'False' in ( @IncludeHomeMobile ) AND 'False' in ( @IncludeGroup ))
BEGIN
		select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
		--Joined to Provider List
	

		--Joined to Patient List
		
		--Parameters referenced
		--1.	All Users		No Subtype selected			No H/M		No Group
		where  
		TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )
		and TSA.cvt_relatedpatientsiteid in ( @PatientSite )
		and TSA.cvt_servicetype in ( @Specialty )
		and (TSA.cvt_servicesubtype in ( @SpecialtySubType )
		or TSA.cvt_servicesubtype is null)
		and 
		TSA.cvt_type <> 1 
		and TSA.cvt_groupappointment <> 1
END 
--2.	All Users		No Subtype not selected		No H/M		No Group
ELSE IF (' All Users' in ( @User ) AND ('00000000-0000-0000-0000-000000000000' not in ( @SpecialtySubType )) AND 'False' in ( @IncludeHomeMobile ) AND 'False' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		
		from ods.mcs_servicesBase TSA 
		
		--Joined to Provider List
	
		--Parameters referenced
		--2.	All Users		No Subtype not selected		No H/M		No Group
		where  
		TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )
		and TSA.cvt_relatedpatientsiteid in ( @PatientSite )
		and TSA.cvt_servicetype in ( @Specialty )
		and TSA.cvt_servicesubtype in ( @SpecialtySubType )
		and 
		TSA.cvt_type <> 1 
		and TSA.cvt_groupappointment <> 1
END
--3.	All Users		No Subtype selected			Yes H/M		No Group
ELSE IF (' All Users' in ( @User ) AND ('00000000-0000-0000-0000-000000000000' not in ( @SpecialtySubType )) AND 'True' in ( @IncludeHomeMobile ) AND 'False' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'
		from ods.mcs_servicesBase TSA 
		
		

		--Parameters referenced
		--3.	All Users		No Subtype selected			Yes H/M		No Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite ) OR (TSA.cvt_type = 1 AND TSA.cvt_relatedpatientsiteid is null))
		and TSA.cvt_servicetype in ( @Specialty )

		and (TSA.cvt_servicesubtype in ( @SpecialtySubType )
		or TSA.cvt_servicesubtype is null)
		and TSA.cvt_groupappointment <> 1
END
--4.	All Users		No Subtype not selected		Yes H/M		No Group
ELSE IF (' All Users' in ( @User ) AND ('00000000-0000-0000-0000-000000000000' not in ( @SpecialtySubType )) AND 'True' in ( @IncludeHomeMobile ) AND 'False' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
	

		--Parameters referenced
		--4.	All Users		No Subtype not selected		Yes H/M		No Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite )
		OR (TSA.cvt_type = 1 AND TSA.cvt_relatedpatientsiteid is null))
		and TSA.cvt_servicetype in ( @Specialty )

		and TSA.cvt_servicesubtype in ( @SpecialtySubType )
		and TSA.cvt_groupappointment <> 1
END
--5.	All Users		No Subtype selected			Yes H/M		Yes Group
ELSE IF (' All Users' in ( @User ) AND ('00000000-0000-0000-0000-000000000000' in ( @SpecialtySubType )) AND 'True' in ( @IncludeHomeMobile ) AND 'True' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
	

		--Parameters referenced
		--5.	All Users		No Subtype selected			Yes H/M		Yes Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite ) OR 
			(TSA.cvt_type = 1 AND TSA.cvt_relatedpatientsiteid is null) OR 
			(TSA.cvt_groupappointment = 1 AND TSA.cvt_patientfacility in (@PatientFacility)))
		and TSA.cvt_servicetype in ( @Specialty )
		and (TSA.cvt_servicesubtype in ( @SpecialtySubType ) OR TSA.cvt_servicesubtype is null)
END
--6.	All Users		No Subtype not selected		Yes H/M		Yes Group
ELSE IF (' All Users' in ( @User ) AND ('00000000-0000-0000-0000-000000000000' not in ( @SpecialtySubType )) AND 'True' in ( @IncludeHomeMobile ) AND 'True' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
	

		--Parameters referenced
		--6.	All Users		No Subtype not selected		Yes H/M		Yes Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite ) OR 
			(TSA.cvt_type = 1 AND TSA.cvt_relatedpatientsiteid is null) OR 
			(TSA.cvt_groupappointment = 1 AND TSA.cvt_patientfacility in (@PatientFacility)))
		and TSA.cvt_servicetype in ( @Specialty )
		and TSA.cvt_servicesubtype in ( @SpecialtySubType )
END
--7.	All Users		No Subtype selected			No H/M		Yes Group
ELSE IF (' All Users' in ( @User ) AND ('00000000-0000-0000-0000-000000000000' in ( @SpecialtySubType )) AND 'False' in ( @IncludeHomeMobile ) AND 'True' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
	

		--Parameters referenced
		--7.	All Users		No Subtype selected			No H/M		Yes Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite ) OR 			
			(TSA.cvt_groupappointment = 1 AND TSA.cvt_patientfacility in (@PatientFacility)))
		and TSA.cvt_servicetype in ( @Specialty )
		and (TSA.cvt_servicesubtype in ( @SpecialtySubType ) OR TSA.cvt_servicesubtype is null)
END
--8.	All Users		No Subtype not selected		No H/M		Yes Group
ELSE IF (' All Users' in ( @User ) AND ('00000000-0000-0000-0000-000000000000' not in ( @SpecialtySubType )) AND 'False' in ( @IncludeHomeMobile ) AND 'True' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
	
		--Parameters referenced
		--8.	All Users		No Subtype not selected		No H/M		Yes Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite ) OR 			
			(TSA.cvt_groupappointment = 1 AND TSA.cvt_patientfacility in (@PatientFacility)))
		and TSA.cvt_servicetype in ( @Specialty )
		and TSA.cvt_servicesubtype in ( @SpecialtySubType )
END
--Second half with a specific user selected
--		and (ProviderQuery.Providers like ( '%' + @User + '%' ) OR
--		PatientQuery.PatientUsers like ( '%' + @User + '%'))
--9.	Specific User	No Subtype selected			No H/M		No Group
if (' All Users' not in ( @User ) AND ('00000000-0000-0000-0000-000000000000' in ( @SpecialtySubType )) AND 'False' in ( @IncludeHomeMobile ) AND 'False' in ( @IncludeGroup ))
BEGIN
		select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
	

		--Parameters referenced
		--9.	Specific User	No Subtype selected			No H/M		No Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )
		and TSA.cvt_relatedpatientsiteid in ( @PatientSite )
		and TSA.cvt_servicetype in ( @Specialty )
		and (TSA.cvt_servicesubtype in ( @SpecialtySubType )
		or TSA.cvt_servicesubtype is null)
		and TSA.cvt_type <> 1 
		and TSA.cvt_groupappointment <> 1
		and (TSA.cvt_patsiteusers like ( '%' + @User + '%' ) OR
			TSA.cvt_patsiteusers like ( '%' + @User + '%'))
	
END 
--10.	Specific User	No Subtype not selected		No H/M		No Group
ELSE IF (' All Users' not in ( @User ) AND ('00000000-0000-0000-0000-000000000000' not in ( @SpecialtySubType )) AND 'False' in ( @IncludeHomeMobile ) AND 'False' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
		
		--Parameters referenced
		--10.	Specific User	No Subtype not selected		No H/M		No Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )
		and TSA.cvt_relatedpatientsiteid in ( @PatientSite )
		and TSA.cvt_servicetype in ( @Specialty )
		and TSA.cvt_servicesubtype in ( @SpecialtySubType )
		and TSA.cvt_type <> 1 
		and TSA.cvt_groupappointment <> 1
		and (TSA.cvt_patsiteusers like ( '%' + @User + '%' ) OR
			TSA.cvt_patsiteusers like ( '%' + @User + '%'))
	
END
--11.	Specific User	No Subtype selected			Yes H/M		No Group
ELSE IF (' All Users' not in ( @User ) AND ('00000000-0000-0000-0000-000000000000' not in ( @SpecialtySubType )) AND 'True' in ( @IncludeHomeMobile ) AND 'False' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as 'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
		--Joined to Provider List
	
		--Parameters referenced
		--11.	Specific User	No Subtype selected			Yes H/M		No Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite ) OR (TSA.cvt_type = 1 AND TSA.cvt_relatedpatientsiteid is null))
		and TSA.cvt_servicetype in ( @Specialty )

		and (TSA.cvt_servicesubtype in ( @SpecialtySubType )
		or TSA.cvt_servicesubtype is null)
		and TSA.cvt_groupappointment <> 1
		and (TSA.cvt_patsiteusers like ( '%' + @User + '%' ) OR
			TSA.cvt_patsiteusers like ( '%' + @User + '%'))

END
--12.	Specific User	No Subtype not selected		Yes H/M		No Group
ELSE IF (' All Users' not in ( @User ) AND ('00000000-0000-0000-0000-000000000000' not in ( @SpecialtySubType )) AND 'True' in ( @IncludeHomeMobile ) AND 'False' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
	

		--Parameters referenced
		--12.	Specific User	No Subtype not selected		Yes H/M		No Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite )
		OR (TSA.cvt_type = 1 AND TSA.cvt_relatedpatientsiteid is null))
		and TSA.cvt_servicetype in ( @Specialty )

		and TSA.cvt_servicesubtype in ( @SpecialtySubType )
		and TSA.cvt_groupappointment <> 1
		and (TSA.cvt_patsiteusers like ( '%' + @User + '%' ) OR
			TSA.cvt_patsiteusers like ( '%' + @User + '%'))
	
END
--13.	Specific User	No Subtype selected			Yes H/M		Yes Group
ELSE IF (' All Users' not in ( @User ) AND ('00000000-0000-0000-0000-000000000000' in ( @SpecialtySubType )) AND 'True' in ( @IncludeHomeMobile ) AND 'True' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 

		--Parameters referenced
		--13.	Specific User	No Subtype selected			Yes H/M		Yes Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite ) OR 
			(TSA.cvt_type = 1 AND TSA.cvt_relatedpatientsiteid is null) OR 
			(TSA.cvt_groupappointment = 1 AND TSA.cvt_patientfacility in (@PatientFacility)))
		and TSA.cvt_servicetype in ( @Specialty )
		and (TSA.cvt_servicesubtype in ( @SpecialtySubType ) OR TSA.cvt_servicesubtype is null)
		and (TSA.cvt_patsiteusers like ( '%' + @User + '%' ) OR
			TSA.cvt_patsiteusers like ( '%' + @User + '%'))
	
END
--14.	Specific User	No Subtype not selected		Yes H/M		Yes Group
ELSE IF (' All Users' not in ( @User ) AND ('00000000-0000-0000-0000-000000000000' not in ( @SpecialtySubType )) AND 'True' in ( @IncludeHomeMobile ) AND 'True' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
		--Joined to Provider List
	
		--Parameters referenced
		--14.	Specific User	No Subtype not selected		Yes H/M		Yes Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite ) OR 
			(TSA.cvt_type = 1 AND TSA.cvt_relatedpatientsiteid is null) OR 
			(TSA.cvt_groupappointment = 1 AND TSA.cvt_patientfacility in (@PatientFacility)))
		and TSA.cvt_servicetype in ( @Specialty )
		and TSA.cvt_servicesubtype in ( @SpecialtySubType )
		and (TSA.cvt_patsiteusers like ( '%' + @User + '%' ) OR
			TSA.cvt_patsiteusers like ( '%' + @User + '%'))
		
END
--15.	Specific User	No Subtype selected			No H/M		Yes Group
ELSE IF (' All Users' not in ( @User ) AND ('00000000-0000-0000-0000-000000000000' in ( @SpecialtySubType )) AND 'False' in ( @IncludeHomeMobile ) AND 'True' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
	
		--Parameters referenced
		--15.	Specific User	No Subtype selected			No H/M		Yes Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite ) OR 			
			(TSA.cvt_groupappointment = 1 AND TSA.cvt_patientfacility in (@PatientFacility)))
		and TSA.cvt_servicetype in ( @Specialty )
		and (TSA.cvt_servicesubtype in ( @SpecialtySubType ) OR TSA.cvt_servicesubtype is null)	
		and (TSA.cvt_patsiteusers like ( '%' + @User + '%' ) OR
			TSA.cvt_patsiteusers like ( '%' + @User + '%'))
	
END
--16.	Specific User	No Subtype not selected		No H/M		Yes Group
ELSE IF (' All Users' not in ( @User ) AND ('00000000-0000-0000-0000-000000000000' not in ( @SpecialtySubType )) AND 'False' in ( @IncludeHomeMobile ) AND 'True' in ( @IncludeGroup ))
BEGIN
select TSA.mcs_servicesid 'tsaId'
		, TSA.mcs_name 'Name'
		, TSA.cvt_relatedprovidersiteidname 'Provider Site'
		, TSA.cvt_providerfacilityname 'Provider Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_providerfacility) 'Provider VISN'
		, TSA.cvt_relatedpatientsiteidname 'Patient Site'
		, TSA.cvt_patientfacilityname 'Patient Facility'
		, (Select mcs_visnname from ods.mcs_facilityBase where mcs_facilityid = TSA.cvt_patientfacility) 'Patient VISN'
		, TSA.cvt_servicetypename 'Specialty'
		, TSA.cvt_servicesubtypename 'Specialty Sub-Type'
		, TSA.statuscodename 'Status'
		, TSA.modifiedon
		, TSA.cvt_providers as  'ProvQueryField'
		, TSA.cvt_patsiteusers as 'PatQueryField'

		from ods.mcs_servicesBase TSA 
		
	
		--Parameters referenced
		--16.	Specific User	No Subtype not selected		No H/M		Yes Group
		where  TSA.statuscode in (@Status)
		and TSA.cvt_relatedprovidersiteid in ( @ProviderSite )

		and (TSA.cvt_relatedpatientsiteid in ( @PatientSite ) OR 			
			(TSA.cvt_groupappointment = 1 AND TSA.cvt_patientfacility in (@PatientFacility)))
		and TSA.cvt_servicetype in ( @Specialty )
		and TSA.cvt_servicesubtype in ( @SpecialtySubType )
		and (TSA.cvt_patsiteusers like ( '%' + @User + '%' ) OR
			TSA.cvt_patsiteusers like ( '%' + @User + '%'))
	
END