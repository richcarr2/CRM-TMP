select distinct ISNULL(IBT.ComponentType,'Codec, Hardware') ComponentType, IBT.MANUFACTURER Manufacturer,IBT.Model, IBT.PartNumber, 
ISNULL(ISNULL(IBT.SerialNumber, C.[Hardware Serial Number]),'DUMMY' + CONVERT(varchar(10),IBT.ImportId)) SerialNumber, c.[Specific System Type Description] [TMS System Type Description], c.[E#164 Alias] [E.164 Alias], c.[System Name] [TMS System Name],
ISNULL(ISNULL(IBT.MasterSerialNumber, C.[Hardware Serial Number]), 'DUMMY' + CONVERT(varchar(10),IBT.ImportId)) MasterSerialNumber,  STM.TMPSystemType [System Type],
STM.TMPCartType [Cart Type], C.[System ID] [Unique ID], ISNULL(dbo.udf_clean(IBT.[Station Code]), IBT.Facility) Facility, --IBT.[Station Code] STNCODE, IBT.Facility FCT,
LTRIM(RTRIM(IBT.[MEDICAL CENTER])) [MEDICAL CENTER], IBT.IFCAPPONumber, dbo.udf_clean(C.[System Contact]) POC, C.[IP Address], IBT.Visn  from (
select 'Main codec camera' ComponentType, null MANUFACTURER, 'Camera' Model, null PartNumber, dbo.udf_clean([CAMERA]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType --, [CART MASTER_SERIAL#],[SYSTEM TYPE] SystemType, SysType
, ImportId, [Station Code] from IronBow I 
where [CAMERA] is not null 
UNION
select 'Codec, Hardware' ComponentType, 'Cisco' MANUFACTURER, dbo.udf_clean([TYPE]) Model, dbo.udf_clean([CODEC PN]) PartNumber, dbo.udf_clean([CODEC/VIDEO _ENDPOINT]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType--, [CART MASTER_SERIAL#],[SYSTEM TYPE] SystemType, SysType
, ImportId, [Station Code] from IronBow I 
where [CODEC/VIDEO _ENDPOINT] is not null 
UNION
select 'Digital Stethoscope' ComponentType, 'GlobalMed' MANUFACTURER, 'ClearSteth USB Chestpiece' Model, null PartNumber, dbo.udf_clean([CLEARSTETH]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [CLEARSTETH] is not null 
UNION
select 'Otoscope/Dermascope' ComponentType, 'GlobalMed' MANUFACTURER, 'TotalENT Otoscope' Model, 'GMR14030019' PartNumber, dbo.udf_clean([OTOSCOPE/DERMSCOPE _COMBO 4MM O_GMR14030019]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [OTOSCOPE/DERMSCOPE _COMBO 4MM O_GMR14030019] is not null 
UNION
select 'Oto/Derm Scope for Otoscopy ' ComponentType, 'GlobalMed' MANUFACTURER, 'TotalENT Otoscope' Model, 'GMD6024ODS' PartNumber, dbo.udf_clean([OTO/DERM SCOPE FOR OTOSCOPY _AND SURFACE VIEWING_GMD6024ODS]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [OTO/DERM SCOPE FOR OTOSCOPY _AND SURFACE VIEWING_GMD6024ODS] is not null and RTRIM(LTRIM([OTO/DERM SCOPE FOR OTOSCOPY _AND SURFACE VIEWING_GMD6024ODS] )) <> ''
UNION
select 'Lightbox' ComponentType, 'JedMed' MANUFACTURER, 'TotalENT Lightbox' Model, 'GMR140300002' PartNumber, dbo.udf_clean([JED MED LIGHTBOX_GMR140300002]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [JED MED LIGHTBOX_GMR140300002] is not null 
UNION
select 'Total Exam Camera 2' ComponentType, 'GlobalMed' MANUFACTURER, 'TotalExam 2 Camera' Model, 'GMR5502WR00' PartNumber, dbo.udf_clean([TOTAL EXAM_TE2 CAMERA (NTSC) WL_GMR5502WR00 ]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [TOTAL EXAM_TE2 CAMERA (NTSC) WL_GMR5502WR00 ] is not null 
UNION
select 'Total Exam Camera 2' ComponentType, 'GlobalMed' MANUFACTURER, 'TotalExam 2 Camera' Model, 'GMR5502WR02' PartNumber, dbo.udf_clean([TOTALEXAM2™ CAMERA _(NTSC)_GMR5502WR02 ]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [TOTALEXAM2™ CAMERA _(NTSC)_GMR5502WR02 ] is not null 
UNION
select 'Otoscope' ComponentType, 'JedMed' MANUFACTURER, 'TotalENT Otoscope' Model, null PartNumber, dbo.udf_clean([LIGHTBOX CAMERA]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [LIGHTBOX CAMERA] is not null 
UNION
select 'Spirometer' ComponentType, 'Welch Alyn Inc' MANUFACTURER, 'Welch Allyn Spirometer' Model, 'GMD50500001' PartNumber, dbo.udf_clean([WA SPIROPERFECT™ _SPIROMETER   _GMD50500001                   ]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [WA SPIROPERFECT™ _SPIROMETER   _GMD50500001                   ] is not null 
UNION
select 'Total Exam Camera' ComponentType, 'GlobalMed' MANUFACTURER, 'TotalExam Camera' Model, 'GMD5500W' PartNumber, dbo.udf_clean([TOTALEXAM™ CAMERA (NTSC) _HANDHELD S-VIDEO _EXAMINATION CAMERA_G]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [TOTALEXAM™ CAMERA (NTSC) _HANDHELD S-VIDEO _EXAMINATION CAMERA_G] is not null 
UNION
select 'Lightbox' ComponentType, 'JedMed' MANUFACTURER, 'TotalENT Lightbox' Model, 'GMR14030002' PartNumber, dbo.udf_clean([COMBO 24 CAMERA/LIGHT _SOURCE_GMR14030002]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [COMBO 24 CAMERA/LIGHT _SOURCE_GMR14030002] is not null 
UNION
select 'ECG/EKG Unit' ComponentType, 'Welch Alyn Inc' MANUFACTURER, 'Welch Allyn USB ECG/EKG Unit' Model, 'GMD50600001 ' PartNumber, dbo.udf_clean([WA ECG/EKG 12 LEAD _SYSTEM W/  _GMD50600001                   ]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [WA ECG/EKG 12 LEAD _SYSTEM W/  _GMD50600001                   ] is not null 
UNION
select 'Otoscope' ComponentType, 'JedMed' MANUFACTURER, 'Welch Allyn Digital Otoscope' Model, 'GMD5100U' PartNumber, dbo.udf_clean([WA DIGITAL MACROVIEW _OTOSCOPE USB_GMD5100U]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [WA DIGITAL MACROVIEW _OTOSCOPE USB_GMD5100U] is not null 
UNION
select 'Otoscope' ComponentType, 'Welch Alyn Inc' MANUFACTURER, 'Welch Allyn Digital Otoscope' Model, 'GMR14030013' PartNumber, dbo.udf_clean([DIGITAL MACROVIEW _OTOSCOPE_GMR14030013                   ]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [DIGITAL MACROVIEW _OTOSCOPE_GMR14030013                   ] is not null 
UNION
select 'Total Exam Camera' ComponentType, 'GlobalMed' MANUFACTURER, 'TotalExam Camera' Model, 'GMD5500A' PartNumber, dbo.udf_clean([TOTALEXAM CAMERA (NTSC) _(S-VIDEO VERSION)_GMD5500A]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [TOTALEXAM CAMERA (NTSC) _(S-VIDEO VERSION)_GMD5500A] is not null 
UNION
select 'Total Exam Camera' ComponentType, 'GlobalMed' MANUFACTURER, 'TotalExam Camera' Model, 'GMD5500ACA' PartNumber, dbo.udf_clean([TOTALEXAM™ CAMERA (NTSC) _HANDHELD S-VIDEO EXAMINATION CAMERA ON]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [TOTALEXAM™ CAMERA (NTSC) _HANDHELD S-VIDEO EXAMINATION CAMERA ON] is not null 
UNION
select 'Astera' ComponentType, 'Otometrics' MANUFACTURER, 'Astera Audiometer' Model, 'GMR12080010' PartNumber, dbo.udf_clean([ASTERA W/ACP + HIFREQ_GMR12080010]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [ASTERA W/ACP + HIFREQ_GMR12080010] is not null 
UNION
select 'Hearing Instrument Test Box' ComponentType, 'Otometrics' MANUFACTURER, 'Aurical HIT Box' Model, 'GMR12080014' PartNumber, dbo.udf_clean([AURICAL REM/HIT W/HIPRO_GMR12080014]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [AURICAL REM/HIT W/HIPRO_GMR12080014] is not null 
UNION
select 'Noahlink' ComponentType, 'Otometrics' MANUFACTURER, 'Noahlink' Model, 'GMR12080013' PartNumber, dbo.udf_clean([NOAHLINK_GMR12080013]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [NOAHLINK_GMR12080013] is not null 
UNION
select 'Otoflex' ComponentType, 'Otometrics' MANUFACTURER, 'Otoflex' Model, 'GMR12080012' PartNumber, dbo.udf_clean([OTO FLEX_GMR12080012]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [OTO FLEX_GMR12080012] is not null 
UNION
select 'Hi-Pro' ComponentType, 'Otometrics' MANUFACTURER, 'HI-PRO' Model, 'GMR12080017' PartNumber, dbo.udf_clean([HI-PRO USB_GMR12080017]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [HI-PRO USB_GMR12080017] is not null 
UNION
select 'Auricle' ComponentType, 'Otometrics' MANUFACTURER, 'AURICAL PMM WITH HI-PRO' Model, 'GMR12080018' PartNumber, dbo.udf_clean([AURICAL PMM WITH HI-PRO_GMR12080018]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [AURICAL PMM WITH HI-PRO_GMR12080018] is not null 
UNION
select 'ACP Adapter' ComponentType, 'Otometrics' MANUFACTURER, 'REMOTE ASTERA ACP ADAPTER' Model, 'GMR12080028' PartNumber, dbo.udf_clean([REMOTE ASTERA ACP ADAPTER_GMR12080028]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [REMOTE ASTERA ACP ADAPTER_GMR12080028] is not null 
UNION
select 'Total Exam Camera HD' ComponentType, 'GlobalMed' MANUFACTURER, 'TotalExam HD' Model, 'GMR5503HD00' PartNumber, dbo.udf_clean([TOTALEXAMHD™ CAMERA_GMR5503HD00]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [TOTALEXAMHD™ CAMERA_GMR5503HD00] is not null 
UNION
select 'Otoscope' ComponentType, null MANUFACTURER, 'Otocam Video Otoscope' Model, 'GMR12080011' PartNumber, dbo.udf_clean([OTOCAM (VIDEO OTOCAM)_GMR12080011]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [OTOCAM (VIDEO OTOCAM)_GMR12080011] is not null 
UNION
select 'Lightbox' ComponentType, 'JedMed' MANUFACTURER, 'TotalENT Lightbox' Model, 'GMR14030045' PartNumber, dbo.udf_clean([JEDMED COMBOCAM HD W/O FT PDL_GMR14030045]) SerialNumber,  
ISNULL(dbo.udf_clean([CART MASTER_SERIAL#]),dbo.udf_clean([CODEC/VIDEO _ENDPOINT])) MasterSerialNumber, [CODEC/VIDEO _ENDPOINT],  
dbo.udf_clean([SYSTEM TYPE]) [SYSTEM TYPE], LEFT(dbo.udf_GetNumeric([ IFCAP PO#]), 3) Facility, dbo.udf_clean(VISN) Visn, dbo.udf_clean([ IFCAP PO#]) IFCAPPONumber, dbo.udf_clean([MEDICAL CENTER]) [MEDICAL CENTER], dbo.udf_SystemType([CART MASTER_SERIAL#],[SYSTEM TYPE], SysType) CartType
, ImportId, [Station Code] from IronBow I 
where [JEDMED COMBOCAM HD W/O FT PDL_GMR14030045] is not null 
) IBT
LEFT JOIN SystemTypeMapping STM on STM.ImportSystemType = IBT.CartType
FULL OUTER JOIN CEVN C on IBT.SerialNumber = C.[Hardware Serial Number]
where IBT.ImportId is not null
order by MasterSerialNumber
