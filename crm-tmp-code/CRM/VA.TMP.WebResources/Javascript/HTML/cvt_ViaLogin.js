if (typeof MCS == "undefined") MCS = {};
if (typeof MCS.VIALogin == "undefined") MCS.VIALogin = {};

{
  MCS.VIALogin.patFacilityNumber = null;
  MCS.VIALogin.proFacilityNumber = null;
  MCS.VIALogin.patFacilityId = null;
  MCS.VIALogin.proFacilityId = null;
  MCS.VIALogin.currentUser = null;
  MCS.VIALogin.SamlString = "";
  MCS.VIALogin.PatDuzRecord = null;
  MCS.VIALogin.ProDuzRecord = null;
  MCS.VIALogin.FakeSAML = "";
  MCS.VIALogin.TestingAV = ""; //pro or pat
  MCS.VIALogin.isGrpClinicappt = false;
  MCS.VIALogin.isBlockResource = false;
  MCS.VIALogin.IsCancelServiceActivityInProgress = false;
}

MCS.VIALogin.LoginButton = function () {
  console.log("Begin MCS.VIALogin.LoginButton");
  var loginDeferred = MCS.VIALogin.Login();

  $.when(loginDeferred).done(function (returnData) {
    return;
  });
};

MCS.VIALogin.LoginOnCancelAppointment = function () {
  console.log("Begin MCS.VIALogin.LoginOnCancelAppointment");
  MCS.VIALogin.IsCancelServiceActivityInProgress = true;

  var loginDeferred = MCS.VIALogin.Login();

  $.when(loginDeferred).done(function (returnData) {
    MCS.VIALogin.IsCancelServiceActivityInProgress = false;
    return;
  });
};

MCS.VIALogin.Login = function () {

    var checkFacilityTypeDeferred = MCS.VIALogin.checkFacilityType();
    if (checkFacilityTypeDeferred == null) { return; }
     
  $.when(checkFacilityTypeDeferred).done(function (returnData) {
    ////debugger;
    if (!returnData.data.isBothSiteFacCerner) {
      setTimeout(function () {
        var loginDeferred = $.Deferred();

        console.log('VIA login call initiated');
        MCS.VIALogin.ToggleButton(false);
        //MCS.VIALogin.hideAll();

        var samlTokenDeferred = MCS.VIALogin.IsValidSamlToken();

        $.when(samlTokenDeferred).done(function (validSamlToken) {

          if (validSamlToken) {
            console.log('SAML is valid.  Checking DUZ.');
            var isValidUserDuzDeferred = MCS.VIALogin.IsValidUserDuz();

            $.when(isValidUserDuzDeferred).done(function (returnData) {
              console.log("isValidUserDuzDeferred done.");

              // For testing
              var isValidUserDuz = false;

              if (typeof returnData.data !== 'undefined') {
                isValidUserDuz = returnData.data.duzIsValid;
              }
              // var isValidUserDuz = returnData.data.duzIsValid;
              console.log("IsValidUserDUZ = " + isValidUserDuz);
              if (isValidUserDuz) { //Success, so untoggle button and Trigger Get Consults
                MCS.VIALogin.ToggleButton(true);
                MCS.VIALogin.TriggerGetConsults();
                loginDeferred.resolve(returnData);
              }
              else { //Not a Valid User Duz, Need to get Duz
                console.log("Not IsValidUserDUZ");
                var viaLoginVimtDeferred = MCS.VIALogin.ViaLoginVimt();
                $.when(viaLoginVimtDeferred).done(function (returnData) {
                  MCS.VIALogin.ToggleButton(true);
                  MCS.VIALogin.TriggerGetConsults();
                  loginDeferred.resolve(returnData);
                });
              }
            });
          }
          else {
            console.log("else validSamlToken");

            //OR get new token - which calls this function again
            if (!MCS.VIALogin.isGrpClinicappt || MCS.VIALogin.IsCancelServiceActivityInProgress) {
              var samlDeferred = MCS.VIALogin.Saml();
              console.log('SAML *not* valid.  Regenerating.');

              $.when(samlDeferred).done(function (returnData) {
                // done, verify returnData.success
                loginDeferred.resolve(returnData);
              });
            }
          }
        });

        return loginDeferred.promise();
      }, 1500);
    }
  });
};


// ************************************************SAML Functions****************************************************
{
  //SAML: 1. Determine if the SAML on the User's record is still active
  //Return True or False
  MCS.VIALogin.IsValidSamlToken = function () {
    console.log("Begin MCS.VIALogin.IsValidSamlToken");
    var mainDeferred = $.Deferred();

    if (typeof Xrm.Page.ui == "undefined" || Xrm.Page.ui == null) Xrm = parent.Xrm;

    var typeCode = Xrm.Utility.getGlobalContext().getQueryStringParameters().etc;
    var entityTypeName = Xrm.Utility.getGlobalContext().getQueryStringParameters().entityTypeName;
    typeCode = entityTypeName === "serviceappointment" ? 4214 : 4201;

    if (typeCode == 4214 && (!Xrm.Page.getAttribute("cvt_type").getValue() && Xrm.Page.getAttribute("mcs_groupappointment").getValue())) {
      MCS.VIALogin.isGrpClinicappt = true;
    }
    else {
      MCS.VIALogin.isGrpClinicappt = false;
    }

    if (MCS.VIALogin.isGrpClinicappt && !MCS.VIALogin.IsCancelServiceActivityInProgress) {
      MCS.VIALogin.ToggleIcon("SAML", "success", "VistA Integration not required at this stage.");
      mainDeferred.resolve(false);
    }

    var vistaintegrationDeferred = MCS.VIALogin.RunVistaIntegration();

    $.when(vistaintegrationDeferred).done(function (returnData) {

      if (returnData.integrationEnabled === false) {

        MCS.VIALogin.ToggleIcon("SAML", "failure", "VistA Integration not turned on.");
        mainDeferred.resolve(true);
      }
      else {
        MCS.VIALogin.ToggleIcon("SAML", "working", "Checking for valid token.");

        var retrieveTokenDeferred = MCS.VIALogin.RetrieveUserSAMLToken();

        $.when(retrieveTokenDeferred).done(function (data) {

          if (MCS.VIALogin.SamlString == "") //No token
          {
            MCS.VIALogin.ToggleIcon("SAML", "failure", "No Token found.");
            mainDeferred.resolve(false);
          }

          var errorTag = MCS.VIALogin.SamlString.indexOf("<wst:Reason>");
          if (errorTag != -1) //Errored Token
          {
            MCS.VIALogin.ToggleIcon("SAML", "failure", "Check failed: Token has error.");
            mainDeferred.resolve(false);
          }
          var endTimeIndex = MCS.VIALogin.SamlString.indexOf("NotOnOrAfter");
          if (endTimeIndex != -1) {
            var endTimeString = MCS.VIALogin.SamlString.substring(endTimeIndex + "NotOnOrAfter".length + 2, endTimeIndex + "NotOnOrAfter".length + 2 + 20);
            var end = new Date(endTimeString).getTime();

            if (isNaN(end)) {
              MCS.VIALogin.ToggleIcon("SAML", "failure", "Check failed: Expiration Date could not be parsed.");
              mainDeferred.resolve(false);
            }

            //Note: VA computer clocks are controlled by IT, so it should be safe to assume that clocks are close enough to accurate that this is a safe way to check
            //Should we make sure the SAML is active for longer than end-30 second?
            if (new Date().getTime() + 1000 * 30 > end) {//Inactive token, or token will be inactive in 30 seconds, so consider it expired
              MCS.VIALogin.ToggleIcon("SAML", "failure", "Check failed: Token is expired.");
              mainDeferred.resolve(false);
            }
            else //Valid token that doesnt expire for at least 30 seconds from now            
            {
              MCS.VIALogin.ToggleIcon("SAML", "success", "Check passed: Token is valid.");
              mainDeferred.resolve(true);
            }
          }
          else //Invalid Token - all valid tokens must contain the NotOnOrAfter token
          {
            MCS.VIALogin.ToggleIcon("SAML", "failure", "Check failed: Token missing expiration time.");
            mainDeferred.resolve(false);
          }
        });
      }
    });

    return mainDeferred.promise();
  };

  //SAML: 1A. Gets SAML from User's record 
  //Sets global variable to SAML token or "" if not found 
  MCS.VIALogin.RetrieveUserSAMLToken = function () {
    console.log("Begin MCS.VIALogin.RetrieveUserSamlToken");
    var deferred = $.Deferred();
    var returnData = {
      success: false,
      data: {}
    };

    if (MCS.VIALogin.currentUser == null) {
      if (typeof Xrm != "undefined") MCS.VIALogin.currentUser = Xrm.Page.context.getUserId();
      else MCS.VIALogin.currentUser = parent.Xrm.Page.context.getUserId();
    }

    Xrm.WebApi.retrieveRecord('systemuser', MCS.VIALogin.currentUser, "?$select=systemuserid, fullname, cvt_samltoken").then(
      function success(result) {
        if (result != null && result.cvt_samltoken != null) {
          var samlString = result.cvt_samltoken;
          MCS.VIALogin.SamlString = samlString;
          MCS.VIALogin.ToggleIcon("SAML", "success", "Success on User Retrieve: SAML Token on user's record.");
          returnData.success = true;
          returnData.data.samlstring = samlString;
          deferred.resolve(returnData);
        }
        else {
          MCS.VIALogin.SamlString = "";
          MCS.VIALogin.ToggleIcon("SAML", "working", "Success on User Retrieve: No SAML Token on user's record. Attempting to get new SAML.");
          returnData.success = true;
          returnData.data.samlstring = "";
          deferred.resolve(returnData);
        }
      },
      function (error) {
        CS.VIALogin.SamlString = "";
        MCS.VIALogin.ToggleIcon("SAML", "failure", "Error on User Retrieve: Unable to get user's record.");

        returnData.success = false;
        deferred.resolve(returnData);
      }
    );

    return deferred.promise();
  };

  //Updates SAML token on User's record
  //Called from IAM, KeepAlive or Fakes functions
  MCS.VIALogin.UpdateUserSAMLToken = function (saml) {
    console.log("Begin MCS.VIALogin.UpdateUserSamlToken");
    var deferred = $.Deferred();
    console.log("Saml TOKEN:\n" + saml);
    if (MCS.VIALogin.currentUser == null) {
      if (typeof Xrm != "undefined") MCS.VIALogin.currentUser = Xrm.Page.context.getUserId();
      else MCS.VIALogin.currentUser = parent.Xrm.Page.context.getUserId();
    }

    var userRecord = {
      'cvt_samltoken': saml
    };

    Xrm.WebApi.updateRecord("systemuser", MCS.VIALogin.currentUser, userRecord).then(
      function success(result) {
        MCS.VIALogin.ToggleIcon("SAML", "success", "Successful Retrieval of new SAML Token.");
        MCS.VIALogin.Login();
        deferred.resolve(true);
      },
      function (error) {
        MCS.VIALogin.ToggleIcon("SAML", "failure", "Error Updating User SAML Information: ");// + MCS.cvt_Common.RestError(error.message));
        deferred.resolve(false);
      }
    );

    return deferred.promise();
  };

  //Retrieves the SSOi SAML Token either through Keep Alive or through Direct IAM STS depending on which URLs are available
  //Called from Appt, BR ribbons and LoginButton
  MCS.VIALogin.Saml = function () {
    console.log("Begin MCS.VIALogin.Saml");
    var deferred = $.Deferred();
    var vistaintegrationDeferred = MCS.VIALogin.RunVistaIntegration();

    $.when(vistaintegrationDeferred).done(function (returnData) {

      if (returnData.success === false) {
        deferred.resolve(returnData);
      }
      else {
        var directIamUrl;

        getURLDeferred = MCS.VIALogin.GetStsUrl();

        $.when(getURLDeferred).done(function (returnData) {
          directIamUrl = returnData.data.value;

          if (directIamUrl != null && directIamUrl != "")
            MCS.VIALogin.CallIamSts(directIamUrl);
          else {
            alert("No PIV Authentication has been set up.  Faking valid SAML Token.");

            MCS.VIALogin.UpdateFakeSAML();

            var udateSAMLTokenDeferred = MCS.VIALogin.UpdateUserSAMLToken(MCS.VIALogin.FakeSAML);

            $.when(udateSAMLTokenDeferred).then(function () {

              deferred.resolve(returnData);
            });
          }
        });
      }
    });

    return deferred.promise();
  };
}
// ************************************************STS Retrieval Functions****************************************************
{
  //Update Fake SAML to tomorrow
  MCS.VIALogin.UpdateFakeSAML = function () {
    console.log("Begin MCS.VIALogin.UpdateFakeSaml");
    var currentTime = new Date(); //2017-11-27T19:37:26Z
    var month = (currentTime.getMonth() + 1);
    if (month < 10) month = "0" + month;
    var date = (currentTime.getDate() + 2);
    if (date < 10) date = "0" + date;
    var tomorrow = currentTime.getFullYear() + "-" + month + "-" + date;
    var fake = '<?xml version="1.0" encoding="UTF-8"?>' +
      '<soapenv:Envelope xmlns:wst="http://schemas.xmlsoap.org/ws/2005/02/trust" xmlns:wsa="http://www.w3.org/2005/08/addressing" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"><soapenv:Body><wst:RequestSecurityTokenResponse><wst:RequestedSecurityToken><saml:Assertion ID="Assertion-uuid7cf42a96-a36b-4a51-9135-bb22579a358d" Version="2.0" IssueInstant="2018-03-07T14:35:30Z" xmlns:saml="urn:oasis:names:tc:SAML:2.0:assertion"><saml:Issuer Format="urn:oasis:names:tc:SAML:2.0:assertion">https://ssoi.sts.va.gov/Issuer/SAML2</saml:Issuer><Signature xmlns="http://www.w3.org/2000/09/xmldsig#">' +
      '<SignedInfo>' +
      '  <CanonicalizationMethod Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>' +
      '  <SignatureMethod Algorithm="http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"/>' +
      '  <Reference URI="#Assertion-uuid7cf42a96-a36b-4a51-9135-bb22579a358d">' +
      '    <Transforms>' +
      '      <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"/>' +
      '      <Transform Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>' +
      '    </Transforms>' +
      '    <DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>' +
      '    <DigestValue>gDIooXX3salmum0DQg8MSF2orJT/4+wjQbQfmDk/yVA=</DigestValue>' +
      '  </Reference>' +
      '</SignedInfo>' +
      '    <SignatureValue>P+PB9Vbn2OwocpzSHi+phJZk+XibOk4XTyo0ORplumK/2C6RKneeZRsAkCvAyW//gQWdRoepctqNIEPAesbPCqSU5z3OhD8f+rgOf5VWlAeb/rwpeenbm4Q1e1xvZ1/OjamsjrD7m5qGGVxRHLZNhp68Zq3JSuEDEmr3VP/KRKt/eFsBC5zqBUUaF5aT2duNA3hjK3/fw9NGeGlxcOIrdtEfWMWmOwSceDb8kKnDONpUHAh1915u0ARPzuTr7SZ+wSZFjYIhbP8FBgusECnA4UU7X3o05Gj1FXdpCzz+htG1SISqsa2NVswSm8FVvFEUG/UP9OAO3LpCXleX37kZ7q7nA9NaHGWwfFHd0Y3w3+Jk9u5r0nK+c6ULb5+1hoYhFaW3xH+LmjGyduHG028QX6HSnnUnidpL/FIrfD3zfRdC+H1NbxAUnvg2L3fpJCzdhgI+3Kh9OylCjN/jbdKeykHvYoxzlT+uR0hg9jl/RD21I2PBR1okovmAMnlYlarch+C3Di+7K48BMm3KAlgbl+UZMgEMVdaeklwJTHLS77ROOVqRZsLVIYMIIwrkChx7TS/DjTdzLWGtt4rApqSEtVeHllo4HrczPQbeika8glVE0YwwG2ix1JHZbviUByw+5r8OIevQ6ztvu2xivlXddqRCiLnhr8bHNDRiWwwzLjE=</SignatureValue><KeyInfo><X509Data><X509Certificate>MIIHQjCCBiqgAwIBAgIHPQAAAADKIDANBgkqhkiG9w0BAQsFADBKMRMwEQYKCZImiZPyLGQBGRYDZ292MRIwEAYKCZImiZPyLGQBGRYCdmExHzAdBgNVBAMTFlZBLUludGVybmFsLVMyLUlDQTEtdjEwHhcNMTcwNDI2MTgzNjEwWhcNMjAwNDI1MTgzNjEwWjCBsTELMAkGA1UEBhMCVVMxDjAMBgNVBAgTBVRleGFzMQ8wDQYDVQQHEwZBdXN0aW4xLDAqBgNVBAoTI1UuUy4gRGVwYXJ0bWVudCBvZiBWZXRlcmFucyBBZmZhaXJzMQwwCgYDVQQLEwNJQU0xIjAgBgNVBAMTGWRldi5zZXJ2aWNlcy5lYXV0aC52YS5nb3YxITAfBgkqhkiG9w0BCQEWEkVBdXRoQWRtaW5zQHZhLmdvdjCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAK1/Vzfs2MbiOptER+q6hm6lJ/PWd8lWrlQkIeae2OpE4tIVSvAXNMT9HXcVL/UOdNarhBy1FuYgp7D6fNlZhLnfButk/QowA+JmpTID+6R70xUVMizevNc+HgAUP4b1Xm4D5fHswUrYhripBGl00pMTR053u+1OWl3OiGhAwkC1Dp6rBnpCuTuDxjC+uTBbCcKV0Crfnm4SuKq3XGukOv8y93hQ6tYZiDg2yXrqRZJeUU3LTZRzzGCPzjqY95w4ypn5gthRTrk1gnXL9OKerg6Ek9Vzm6lM+uNPolA2IEEB4/tHTub1GX+PRCAgGH2TLt/t4krBvNv/8iQSshh0e2kGnwOPYbCf452l92YpjkQxR9RC/H8gJ/9RsnTVhdQDMZwJfQxF1KAAHJ4v6suA4XCGqUA/FGO8ivRGzfeknu5MP83EpG03yJqpYZJPCq/ZaaDyNhNWXtyIiXHd3UdzR/Ukq3IAPTAnXZq0IbC6tAgmV2pmlSK6JOn+m4517vzMz+zRR1vW5gRqWxOKECYdZ2vK+d1FIOHxJwFoJSi85EejvMAlwnlPzs4brJbaqGeoFzt+CDwDKRQ2mjQnbNEIvKgxlMtDW89JadeQThz6tB1eez3GUreDgLtX9IoPMSNpyZVEGuW5qKcqYRWco5Sr+AqtbP42rj4isTDVIbKHcPvdAgMBAAGjggLDMIICvzAdBgNVHSUEFjAUBggrBgEFBQcDAgYIKwYBBQUHAwEwggEfBgNVHREBAf8EggETMIIBD4IZZGV2LnNlcnZpY2VzLmVhdXRoLnZhLmdvdoIZaW50LnNlcnZpY2VzLmVhdXRoLnZhLmdvdoIacGludC5zZXJ2aWNlcy5lYXV0aC52YS5nb3aCGXNxYS5zZXJ2aWNlcy5lYXV0aC52YS5nb3aCHXByZXByb2Quc2VydmljZXMuZWF1dGgudmEuZ292ghhzZXJ2aWNlcy5kZXYyLmlhbS52YS5nb3aCF2Rldi5zZXJ2aWNlcy5pYW0udmEuZ292ghhwaW50LnNlcnZpY2VzLmlhbS52YS5nb3aCF3NxYS5zZXJ2aWNlcy5pYW0udmEuZ292ghtwcmVwcm9kLnNlcnZpY2VzLmlhbS52YS5nb3YwHQYDVR0OBBYEFEJT0YkqlZjptGcVCVQZMP6xpOr2MB8GA1UdIwQYMBaAFBtt3+s95eIN7xax0N5fWBpWy+TsMEkGA1UdHwRCMEAwPqA8oDqGOGh0dHA6Ly9jcmwucGtpLnZhLmdvdi9wa2kvY3JsL1ZBLUludGVybmFsLVMyLUlDQTEtdjEuY3JsMHsGCCsGAQUFBwEBBG8wbTBHBggrBgEFBQcwAoY7aHR0cDovL2FpYS5wa2kudmEuZ292L3BraS9haWEvdmEvVkEtSW50ZXJuYWwtUzItSUNBMS12MS5jZXIwIgYIKwYBBQUHMAGGFmh0dHA6Ly9vY3NwLnBraS52YS5nb3YwCwYDVR0PBAQDAgWgMD0GCSsGAQQBgjcVBwQwMC4GJisGAQQBgjcVCIHIwzOB+fAGgaWfDYTggQiFwqpLBoOCn2CB4ItSAgFkAgESMCcGCSsGAQQBgjcVCgQaMBgwCgYIKwYBBQUHAwIwCgYIKwYBBQUHAwEwDQYJKoZIhvcNAQELBQADggEBADFVJMfA1EbQewQ/wU+ipChfsucmUAZ000/AWIvQjdcv2zULdFZqna5Nn9GYkwqVFbln6eyVVpSBuLkbJlnUeJPyvpNb2h9EMCMqiu2XpMDXTRZhfjsFKqc6ZnlwbPcR+BvSDCCl+h60NFTdkr3FRgqElxuGuvccl3nWVN9cHZQsUqrqygGsTBeObCJp6kUBXW0Mxtjz/6iT08IzwwVoXUaopKn/ahzYpL29ZASQBzj1unpZRWaGNIUmok71jdBfUPSTJrvOaPbfo2UqqoFpqtLwTY7OUFAM6qhaswpSnJpCOZMYw5LTpWLVjjArJ/EjYf289+ubrMoOFiIDrLn4iS8=</X509Certificate><X509IssuerSerial><X509IssuerName>CN=VA-Internal-S2-ICA1-v1, DC=va, DC=gov</X509IssuerName><X509SerialNumber>17169973579401760</X509SerialNumber></X509IssuerSerial></X509Data></KeyInfo></Signature><saml:Subject><saml:NameID Format="urn:oasis:names:tc:SAML:2.0:nameid-format:persistent">CN=LastName\, FirstName,OU=JIT,OU=Users,OU=National Contractors,DC=vha,DC=med,DC=va,DC=gov</saml:NameID><saml:SubjectConfirmation Method="urn:oasis:names:tc:SAML:2.0:cm:sender-vouches"><saml:SubjectConfirmationData Recipient="https://internalcrm.crm15.xrm.va.gov/TMP" Address="CN=LastName\, FirstName,OU=JIT,OU=Users,OU=National Contractors,DC=vha,DC=med,DC=va,DC=gov"/></saml:SubjectConfirmation></saml:Subject><saml:Conditions NotBefore="2018-03-07T14:30:30Z" NotOnOrAfter="' +
      tomorrow +
      'T14:50:30Z"><saml:AudienceRestriction><saml:Audience>https://*.va.gov/*</saml:Audience></saml:AudienceRestriction></saml:Conditions><saml:AuthnStatement AuthnInstant="2018-03-07T14:35:30Z"><saml:AuthnContext><saml:AuthnContextClassRef>urn:oasis:names:tc:SAML:2.0:ac:classes:TLSClient</saml:AuthnContextClassRef></saml:AuthnContext></saml:AuthnStatement><saml:AttributeStatement><saml:Attribute Name="transactionid" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>807743ce-f0f9-40a8-bd83-329567b0f08d</saml:AttributeValue></saml:Attribute><saml:Attribute Name="urn:va:vrm:iam:corpid" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue/></saml:Attribute><saml:Attribute Name="issueinstant" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>2018-03-07T14:35:30.849Z</saml:AttributeValue></saml:Attribute><saml:Attribute Name="authnsystem" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>SSOi</saml:AttributeValue></saml:Attribute><saml:Attribute Name="authenticationtype" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>Direct</saml:AttributeValue></saml:Attribute><saml:Attribute Name="proofingauthority" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>VA</saml:AttributeValue></saml:Attribute><saml:Attribute Name="assurancelevel" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>3</saml:AttributeValue></saml:Attribute><saml:Attribute Name="sessionScope" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>B</saml:AttributeValue></saml:Attribute><saml:Attribute Name="urn:va:vrm:iam:firstname" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>FIRSTNAME</saml:AttributeValue></saml:Attribute><saml:Attribute Name="urn:va:vrm:iam:lastname" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>LASTNAME</saml:AttributeValue></saml:Attribute><saml:Attribute Name="urn:va:vrm:iam:secid" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>1013029324</saml:AttributeValue></saml:Attribute><saml:Attribute Name="urn:va:ad:samaccountname" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>UserName</saml:AttributeValue></saml:Attribute><saml:Attribute Name="upn" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>Email@va.gov</saml:AttributeValue></saml:Attribute><saml:Attribute Name="email" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>Email@va.gov</saml:AttributeValue></saml:Attribute><saml:Attribute Name="urn:oasis:names:tc:xspa:1.0:subject:subject-id" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>FIRSTNAME LASTNAME</saml:AttributeValue></saml:Attribute><saml:Attribute Name="urn:oasis:names:tc:xspa:1.0:subject:organization" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>Department of Veterans Affairs</saml:AttributeValue></saml:Attribute><saml:Attribute Name="urn:oasis:names:tc:xspa:1.0:subject:organization-id" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>urn:oid:2.16.840.1.113883.4.349</saml:AttributeValue></saml:Attribute><saml:Attribute Name="urn:nhin:names:saml:homeCommunityId" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>urn:oid:2.16.840.1.113883.4.349</saml:AttributeValue></saml:Attribute><saml:Attribute Name="uniqueUserId"><saml:AttributeValue><SECID Name="urn:va:vrm:iam:secid">000000000</SECID></saml:AttributeValue></saml:Attribute><saml:Attribute Name="urn:va:vrm:iam:mviicn" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>1013029324V606354</saml:AttributeValue></saml:Attribute><saml:Attribute Name="urn:va:vrm:iam:vistaid" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified"><saml:AttributeValue>516-00000000</saml:AttributeValue><saml:AttributeValue>631-000000000</saml:AttributeValue></saml:Attribute></saml:AttributeStatement></saml:Assertion></wst:RequestedSecurityToken><wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Validate</wst:RequestType><wst:TokenType>http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0</wst:TokenType><wst:Status><wst:Code>http://docs.oasis-open.org/ws-sx/ws-trust/200512/status/valid</wst:Code></wst:Status></wst:RequestSecurityTokenResponse></soapenv:Body></soapenv:Envelope>';

    MCS.VIALogin.FakeSAML = fake;
  };

  //Retrieves the Token from IAM directly
  MCS.VIALogin.CallIamSts = function (iamUrl) {
    var url = Xrm.Utility.getGlobalContext().getClientUrl();

    console.log("Begin MCS.VIALogin.CallIamSts");
    var poststring = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns=\"http://docs.oasis-open.org/ws-sx/ws-trust/200512\">"
      + "<soapenv:Header/>"
      + "<soapenv:Body>"
      + "<ns:RequestSecurityToken>"
      + "<ns:Base>"
      + "<wss:TLS xmlns:wss=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"/>"
      + "</ns:Base>"
      + "<wsp:AppliesTo xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\">"
      + "<wsa:EndpointReference xmlns:wsa=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\">"
      + "<wsa:Address>" + url + "/tmp</wsa:Address>"
      + "</wsa:EndpointReference>"
      + "</wsp:AppliesTo>"
      + "<ns:Issuer>"
      + "<wsa:Address xmlns:wsa=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\">https://ssoi.sts.va.gov/Issuer/smtoken/SAML2</wsa:Address>"
      + "</ns:Issuer>"
      + "<ns:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Validate</ns:RequestType>"
      + "</ns:RequestSecurityToken>"
      + "</soapenv:Body>"
      + "</soapenv:Envelope>"
      ;
    url = iamUrl == null ? MCS.VIALogin.GetStsUrl() : iamUrl;
    //$.support.cors = true;
    $.get(url).done(function () {
      $.ajax({
        url: url,
        type: 'POST',
        crossDomain: true,
        data: poststring,
        contentType: 'text/plain',
        dataType: 'text',
        success: function (result) {
          MCS.VIALogin.UpdateUserSAMLToken(result);
          //MCS.VIALogin.Login();
        },
        error: function (jqXHR, tranStatus, errorThrown) {
          alert("Unable to Get SAML token to log into Vista\n" +
            'Status: ' + jqXHR.status + ' ' + jqXHR.statusText + '. ' +
            'Response: ' + jqXHR.responseText
          );
        }
      });
    }).fail(function (err) {
      alert("Failure to Get SAML token to log into Vista\n" +
        'Status: ' + err.status + ' ' + err.statusText + '. ' +
        'Response: ' + err.responseText
      );
    });
  };
}

// ************************************************DUZ Checking Functions****************************************************
{
  //Returns T or F. //this returns false if either pat or pro is not valid.
  MCS.VIALogin.IsValidUserDuz = function () {
    console.log("Begin MCS.VIALogin.IsValidUserDuz");
    var deferred = $.Deferred();

    var returnData = {
      success: true,
      data: {}
    };

    var vistaintegrationDeferred = MCS.VIALogin.RunVistaIntegration();

    $.when(vistaintegrationDeferred).done(function (returnData) {

      if (returnData.integrationEnabled === false) {
        deferred.resolve(returnData);
        return true;
      }
      else {
        var deferredArray = [];
        var duzSideValue = MCS.VIALogin.DuzSide();
        var duzIsValid = true;

        console.log('vistaintegrationDeferred done.');
        console.log("Duz Side: " + duzSideValue);

        //Intrafacility - so just pro
        if (duzSideValue == "both" || duzSideValue == "pro") {
          var getFacilityNumberDeferred = MCS.VIALogin.GetFacilityNumber("pro");
          deferredArray.push(getFacilityNumberDeferred);
        }

        //Interfacility or just pat
        if (duzSideValue == "both" || duzSideValue == "pat") {
          var getFacilityNumberDeferred = MCS.VIALogin.GetFacilityNumber("pat");
          deferredArray.push(getFacilityNumberDeferred);
        }

        console.log('calling deferredArray ' + deferredArray.length);

        $.when.all(deferredArray).then(function (returnData) {

          console.log('deferredArray done.');

          var duzArray = [];
          var proCheck = "";
          var patCheck = "";
          var proDuz = "";
          var patDuz = "";

          // The chain of calls from here are as follows. 
          // ActiveDuz-- > GetUserDuzRecord-- > LoginWithAction ? --> SetUserDuz

          for (n = 0; n < returnData.length; n++) {
            if (returnData[n].data.Side == "pro") {
              proCheck = returnData[n].data.facilityNumber;
              MCS.VIALogin.proFacilityNumber = proCheck;
              var proDuzDeferred = MCS.VIALogin.ActiveDuz(MCS.VIALogin.proFacilityNumber, "pro");
              duzArray.push(proDuzDeferred);
            }
            if (returnData[n].data.Side == "pat") {
              patCheck = returnData[n].data.facilityNumber;
              MCS.VIALogin.patFacilityNumber = patCheck;
              var patDuzDeferred = MCS.VIALogin.ActiveDuz(MCS.VIALogin.patFacilityNumber, "pat");
              duzArray.push(patDuzDeferred);
            }
          }

          $.when.all(duzArray).then(function (returnData) {

            console.log('duzArray done.');

            for (n = 0; n < returnData.length; n++) {
              if (returnData[n].Side == "pro") {
                if (returnData[n].data.duzRecord != null) proDuz = returnData[n].data.duzRecord.cvt_name;
              }
              if (returnData[n].Side == "pat") {
                if (returnData[n].data.duzRecord != null) patDuz = returnData[n].data.duzRecord.cvt_name;
              }
            }

            var typeCode = Xrm.Utility.getGlobalContext().getQueryStringParameters().etc;
            var entityTypeName = Xrm.Utility.getGlobalContext().getQueryStringParameters().entityTypeName;
            typeCode = entityTypeName === "serviceappointment" ? 4214 : 4201;
            //if (typeCode == 4214){//this is all happening on a SERVICE APPOINTMENT form
            if ((proDuz != "") || (patDuz != "")) {
              console.log("canceling from a ServiceActivity.  Duz functions follow");
              if (proDuz != "" && !MCS.VIALogin.isProviderSiteFacCerner) {
                MCS.VIALogin.ToggleIcon("ProDuz", "success", "Successful retrieval of current Provider Duz: " + MCS.VIALogin.proFacilityNumber);
                if (MCS.VIALogin.proFacilityNumber != null) {
                  if (typeCode == 4214) {
                    Xrm.Page.getAttribute("cvt_providerstationcode").setValue(MCS.VIALogin.proFacilityNumber);
                  }
                }

                if (typeCode == 4214) {
                  if (Xrm.Page.getAttribute("cvt_prouserduz") != null) { Xrm.Page.getAttribute("cvt_prouserduz").setValue(proDuz); }
                  else if (parent.Xrm.Page.getAttribute("cvt_prouserduz") != null) { parent.Xrm.Page.getAttribute("cvt_prouserduz").setValue(proDuz); }
                }
              }
              else {
                if (MCS.VIALogin.proFacilityNumber != null) {
                  if (MCS.VIALogin.proFacilityNumber != null) {
                    if (typeCode == 4214) {
                      Xrm.Page.getAttribute("cvt_providerstationcode").setValue(MCS.VIALogin.proFacilityNumber);
                    }
                  }
                  if (!MCS.VIALogin.isProviderSiteFacCerner) {
                    console.log("Login Failure - Provider Facility Number is: " + MCS.VIALogin.proFacilityNumber);
                    MCS.VIALogin.ToggleIcon("ProDuz", "failure", "Unsuccessful retrieval of current Provider Duz:" + MCS.VIALogin.proFacilityNumber);
                  }
                }
                else {
                  if (!MCS.VIALogin.isProviderSiteFacCerner) {
                    console.log("Login Failure - Facility Number is NULL");
                    MCS.VIALogin.ToggleIcon("ProDuz", "success", "");
                  }
                }
              }

              if (patDuz != "" && !MCS.VIALogin.isPatientSiteFacCerner) {
                MCS.VIALogin.ToggleIcon("PatDuz", "success", "Successful retrieval of current Patient Duz: " + MCS.VIALogin.patFacilityNumber);
                if (typeCode == 4214) {
                  if (Xrm.Page.getAttribute("cvt_patuserduz") != null) { Xrm.Page.getAttribute("cvt_patuserduz").setValue(patDuz); }
                  else if (parent.Xrm.Page.getAttribute("cvt_patuserduz") != null) { parent.Xrm.Page.getAttribute("cvt_patuserduz").setValue(patDuz); }
                }
              }
              else {
                if (MCS.VIALogin.patFacilityNumber != null && !MCS.VIALogin.isPatientSiteFacCerner) {
                  console.log("Login Failure - Patient Facility Number is: " + MCS.VIALogin.patFacilityNumber);
                  MCS.VIALogin.ToggleIcon("PatDuz", "failure", "Unsuccessful retrieval of current Patient Duz: " + MCS.VIALogin.patFacilityNumber);
                }
                else if (!MCS.VIALogin.isPatientSiteFacCerner) {
                  console.log("Login Failure - Patient Facility Number is NULL");
                  MCS.VIALogin.ToggleIcon("PatDuz", "success", "");
                }
              }
            }


            console.log('... setting duzIsValid.');
            returnData.data = {};

            duzIsValid = !duzIsValid ? false : proDuz != "";
            returnData.data.duzIsValid = duzIsValid;


            duzIsValid = !duzIsValid ? false : patDuz != "";
            returnData.data.duzIsValid = duzIsValid;

            deferred.resolve(returnData);
          });

        });
      }
    });

    return deferred.promise();
  };

  //Duz Supporting Functions
  //Gets Pat Pro or Both
  MCS.VIALogin.DuzSide = function () {
    console.log("Begin MCS.VIALogin.DuzSide");
    if (typeof Xrm == "undefined") Xrm = parent.Xrm;

    var typeCode = Xrm.Utility.getGlobalContext().getQueryStringParameters().etc;
    var entityTypeName = Xrm.Utility.getGlobalContext().getQueryStringParameters().entityTypeName;
    typeCode = entityTypeName === "serviceappointment" ? 4214 : 4201;
    var runProSide = true;
    var runPatSide = true;

    if (typeCode == 4214) {
      runPatSide = Xrm.Page.getAttribute("cvt_type").getValue() == false;
      runProSide = Xrm.Page.getAttribute("cvt_telehealthmodality").getValue() == false;
    }

    if (runPatSide && runProSide) return "both";
    else if (runPatSide) return "pat";
    else if (runProSide) return "pro";
    else return null;
  };

  MCS.VIALogin.GetPatSite = function (typeCode) {
    console.log("Begin MCS.VIALogin.GetPatSite");
    var deferred = $.Deferred();

    var returnData = {
      success: true,
      data: {
      }
    };

    var filter = "_cvt_serviceactivityid_value eq " + Xrm.Page.data.entity.getId().replace('{', '').replace('}', '');

    Xrm.WebApi.retrieveMultipleRecords("appointment", "?$select=_cvt_site_value" + "&$filter=" + filter).then(
      function success(results) {

        if (results && results.entities && results.entities != null && results.entities.length > 0) {
          var record = results.entities[0];

          if (record['_cvt_site_value'] != null) {
            patSite = record['_cvt_site_value'];
            MCS.VIALogin.SetLookup(record['_cvt_site_value'], Xrm.Page.getAttribute("mcs_relatedsite"));

            returnData.data.patSite = patSite;
            deferred.resolve(returnData);
          }
        }
        else {
          // no records found
          returnData.success = false;
          deferred.resolve(returnData);
        }
      },
      function (error) {
        returnData.success = false;
        alert('Error occurred while retrieving the appointment : ' + error);
        deferred.resolve(returnData);
      }
    );

    return deferred.promise();
  };

  MCS.VIALogin.GetProSite = function (serviceAppointment) {
    console.log("Begin MCS.VIALogin.GetProSite");
    var deferred = $.Deferred();

    var returnData = {
      success: true,
      data: {
      }
    };

    if (serviceAppointment !== null) {
      serviceAppointment = serviceAppointment.getValue();

      if (serviceAppointment !== null) {

        var serviceApptID = null;
        if (typeof serviceAppointment !== 'undefined') {
          if (typeof serviceAppointment.id !== 'undefined') {
            serviceApptID = serviceAppointment.id;
          }
          else if (Array.isArray(serviceAppointment) && serviceAppointment.length > 0 && typeof serviceAppointment[0].id !== 'undefined') {
            serviceApptID = serviceAppointment[0].id.replace('{', '').replace('}', '');
          }
        }
        Xrm.WebApi.retrieveRecord('serviceappointment', serviceApptID, "?$select=_mcs_relatedprovidersite_value").then(
          function success(result) {
            if (result != null) returnData.data.proSite = result._mcs_relatedprovidersite_value;

            patSite = Xrm.Page.getAttribute("cvt_site").getValue();

            if (patSite != null) returnData.data.patSite = patSite[0].id;

            deferred.resolve(returnData);
          },
          function (error) {
            console.log("failed to retrieve Service Appointment, Unable to perform VIA login: "); // + MCS.cvt_Common.RestError(error.message));
            returnData.success = false;
            deferred.resolve(returnData);
          }
        );
      }
    }

    else {
      returnData.success = false;
      deferred.resolve(returnData);
    }
    return deferred.promise();
  };


  MCS.VIALogin.GetPatAndProSite = function (typeCode, side) {
    console.log("Begin MCS.VIALogin.GetPatAndProSite");

    var deferred = $.Deferred();
    var returnData = {
      success: true,
      data: {
      }
    };

    var deferreds = [];

    if (typeCode == 4214) {//Service Appointment
      patSite = Xrm.Page.getAttribute("mcs_relatedsite").getValue();
      proSite = Xrm.Page.getAttribute("mcs_relatedprovidersite").getValue();

      if (patSite != null) {
        patSite = patSite[0].id;
        returnData.data.patSite = patSite;
      } ////if cancel is in progress and side is pat for clinic based Group appt
      else if (MCS.VIALogin.IsCancelServiceActivityInProgress && side == "pat" && !Xrm.Page.getAttribute("cvt_type").getValue() && Xrm.Page.getAttribute("mcs_groupappointment").getValue()) {
        deferreds.push(MCS.VIALogin.GetPatSite(typeCode));
      }

      if (proSite != null) {
        proSite = proSite[0].id;
        returnData.data.proSite = proSite;
      }
      else {

        //Can't do this, I think.  The serviceactivity doesn't exist yet???
        var serviceAppointment = Xrm.Page.getAttribute("cvt_serviceactivityid");

        //so how do we get the provider site?  It has to come from the SP
        if (serviceAppointment != null) deferreds.push(MCS.VIALogin.GetProSite(serviceAppointment));
      }
    }
    else {
      var serviceAppt = Xrm.Page.getAttribute("cvt_serviceactivityid");
      if (serviceAppt != null) deferreds.push(MCS.VIALogin.GetProSite(serviceAppt));
    }

    if (deferreds.length == 0) {
      deferred.resolve(returnData);
    }
    else {
      $.when.all(deferreds).then(function (returnData) {
        returnData.data = {};

        for (var i = 0; i < returnData.length; i++) {
          if (typeof returnData[i].data.patSite !== 'undefined') returnData.data.patSite = returnData[i].data.patSite;
          if (typeof returnData[i].data.proSite !== 'undefined') returnData.data.proSite = returnData[i].data.proSite;
        }

        if (typeof returnData.data.patSite === 'undefined') {
          patSite = Xrm.Page.getAttribute("cvt_site");
          if (patSite != null) {
            patSite = patSite.getValue();

            if (patSite != null) {
              patSite = patSite[0].id;
              returnData.data.patSite = patSite;
            }
          }
        }

        deferred.resolve(returnData);
      });
    }

    return deferred.promise();
  };

  MCS.VIALogin.getProSiteFacId = function (proSite) {
    var deferred = $.Deferred();
    var returnData = {
      success: true,
      data: {
      }
    };

    Xrm.WebApi.retrieveRecord('mcs_facility', proSite, "?$select=mcs_facilityid,mcs_name").then(
      function success(result) {
        if (result != null) {
          if (result.mcs_facilityid != null) {
            returnData.data.proSiteFacId = result.mcs_facilityid;
            deferred.resolve(returnData);
          }
        }
        deferred.resolve(returnData);
      },
      function (error) {
        Xrm.WebApi.retrieveRecord('mcs_site', proSite, "?$select=mcs_facilityid,mcs_siteid").then(
          function success(result) {
            if (result != null) {
              if (result.mcs_facilityid != null) {
                returnData.data.proSiteFacId = result.mcs_facilityid;
                deferred.resolve(returnData);
              }
            }
          },
          function (error) {
            alert("failed TMP Site check, Unable to perform VIA login: " + error.message);
            returnData.success = false;
            deferred.resolve(returnData);
          }
        );
      }
    );

    return deferred.promise();
  };

  MCS.VIALogin.GetSP = function () {
    console.log("Begin MCS.VIALogin.GetSP");
    var deferred = $.Deferred();
    var returnData = {
      success: true,
      data: {
      }
    };

    var schedPkg = Xrm.Page.getAttribute("cvt_relatedschedulingpackage");
    var schedPkgId = "";
    if (schedPkg !== null && schedPkg.getValue() != null) {
      schedPkgId = schedPkg.getValue()[0].id.toString().replace("{", "").replace("}", "");

      console.log("Scheduling Package: " + schedPkgId);

      Xrm.WebApi.retrieveRecord('cvt_resourcepackage', schedPkgId, "?$select=cvt_name&$expand=cvt_providerfacility($select=mcs_facilityid,mcs_name,mcs_stationnumber)").then(
        function success(result) {
          if (result != null) {
            if (result.cvt_providerfacility != null) {
              var proFacId = result.cvt_providerfacility;
              returnData.data.proFacId = proFacId;
              console.log("Provider Facility : " + proFacId);
              deferred.resolve(returnData);
            }
            else {
              deferred.resolve(returnData);
            }
          }
          else {
            deferred.resolve(returnData);
          }
        },
        function (error) {
          returnData.success = false;
          deferred.resolve(returnData);
        }
      );
    }
    else { //***schedPkg IS null, because we're logging in from a ReserveResource****
      schedPkgId = "";
      var spRecordId = "";
      var spRecord = null;
      spRecord = Xrm.Page.getAttribute("cvt_serviceactivityid");
      if (spRecord !== null && spRecord.getValue() != null) {
        spRecordId = spRecord.getValue()[0].id.replace("{", "").replace("}", "");
        //get the Service Activity 
        Xrm.WebApi.retrieveRecord('serviceappointment', spRecordId, "?$select=_cvt_relatedschedulingpackage_value").then(
          function success(result) {
            console.log("Retrieve success!");

            if (result._cvt_relatedschedulingpackage_value != null) {
              schedPkg = result._cvt_relatedschedulingpackage_value;
              schedPkgId = schedPkg;
              console.log("Scheduling Package: " + schedPkgId);
              Xrm.WebApi.retrieveRecord('cvt_resourcepackage', schedPkgId, "?$select=cvt_name&$expand=cvt_providerfacility($select=mcs_facilityid,mcs_name,mcs_stationnumber)").then(
                function success(result) {
                  if (result.cvt_providerfacility != null) {
                    var proFacId = result.cvt_providerfacility;
                    returnData.data.proFacId = proFacId;
                    console.log("Provider Facility : " + proFacId);
                    deferred.resolve(returnData);
                  }
                },
                function (error) {
                  returnData.success = false;
                  deferred.resolve(returnData);
                }
              );
            }

          },
          function (error) {
            //  //////debugger;
            console.log(error.message);
            returnData.success = false;
            //deferred.resolve(returnData);					
          }
        );

        //deferred.resolve(returnData);
      }
    }

    return deferred.promise();
  };

  MCS.VIALogin.getPatSiteFacId = function (patSite, showError) {
    console.log("Begin MCS.VIALogin.getPatSiteFacId");
    var deferred = $.Deferred();
    var returnData = {
      success: true,
      data: {
      }
    };

    Xrm.WebApi.retrieveRecord('mcs_site', patSite, "?$select=_mcs_facilityid_value,mcs_siteid").then(
      function success(result) {
        if (result != null) {
          if (result._mcs_facilityid_value != null) {
            patSiteFacId = result._mcs_facilityid_value;
            returnData.data.patSiteFacId = patSiteFacId;
            deferred.resolve(returnData);
          }
        }
        deferred.resolve(returnData);
      },
      function (error) {
        if (showError) console.log("failed TMP Site check, Unable to perform VIA login: "); // + MCS.cvt_Common.RestError(error.message));
        returnData.success = false;
        deferred.resolve(returnData);
      });

    return deferred.promise();
  };

  MCS.VIALogin.GetPatSiteFacIdandProSiteFacId = function (patSite, proSite) {
    console.log("Begin MCS.VIALogin.GetPatSiteFacIdandProSiteFacId");
    var deferred = $.Deferred();
    var returnData = {
      success: true,
      data: {
      }
    };

    var deferreds = [];

    if (patSite != null) {
      var patSiteDeferred = MCS.VIALogin.getPatSiteFacId(patSite, true);
      deferreds.push(patSiteDeferred);
    }

    if (proSite != null) {
      var proSiteDeferred = MCS.VIALogin.getProSiteFacId(proSite);
      deferreds.push(proSiteDeferred);
    }

    $.when.all(deferreds).then(function (dataReturned) {
      for (n = 0; n < dataReturned.length; n++) {
        if (typeof dataReturned[n].data.patSiteFacId !== 'undefined' && dataReturned[n].data.patSiteFacId != null) {
          returnData.data.patSiteFacId = dataReturned[n].data.patSiteFacId;
        }

        if (typeof dataReturned[n].data.proSiteFacId !== 'undefined' && dataReturned[n].data.proSiteFacId != null) {
          returnData.data.proSiteFacId = dataReturned[n].data.proSiteFacId;
        }
      }
      deferred.resolve(returnData);
    });

    return deferred.promise();
  };

  MCS.VIALogin.GetFacilityNumberOfSite = function (siteFacId) {
    console.log('GetFacilityNumber enter');
    var deferred = $.Deferred();
    var returnData = {
      success: true,
      data: {
      }
    };

    if (siteFacId != null) {
      Xrm.WebApi.retrieveRecord('mcs_facility', siteFacId, "?$select=mcs_stationnumber").then(
        function success(result) {

          console.log('Facility ID Retrieve success ...');

          if (result != null) {
            if (result.mcs_stationnumber != null) {
              console.log('Facility ID StationNumber not null 1');

              facilityNumber = result.mcs_stationnumber;
              returnData.data.facilityNumber = facilityNumber;

              console.log('Facility ID StationNumber not null 1');

              deferred.resolve(returnData);
            }
            else {
              console.log('Facility ID StationNumber null ...');
              deferred.resolve(returnData);
            }
          }
          else {
            console.log('Facility ID result is null...');
            deferred.resolve(returnData);
          }
        },
        function (error) {
          alert("Failed to get Facility station number, Unable to Perform VIA Login: " + error.message);
          returnData.success = false;
          deferred.resolve(returnData);
        });
    }
    else {
      console.log('SiteFacID is null, resolving...');
      deferred.resolve(returnData);
    }

    return deferred.promise();
  };

  //Helper method to get the station number of the facility from input of "pat" or "pro"
  MCS.VIALogin.GetFacilityNumber = function (side) {
    console.log("Begin MCS.VIALogin.GetFacilityNumber");
    var deferred = $.Deferred();
    var siteId;
    var patSite = null;
    var proSite = null;
    var patSiteFacId = null;
    var proSiteFacId = null;
    var facilityNumber = null;

    var typeCode = Xrm.Utility.getGlobalContext().getQueryStringParameters().etc;
    var entityTypeName = Xrm.Utility.getGlobalContext().getQueryStringParameters().entityTypeName;
    typeCode = entityTypeName === "serviceappointment" ? 4214 : 4201;
    var getPatAndProSiteDeferred = MCS.VIALogin.GetPatAndProSite(typeCode, side);

    $.when(getPatAndProSiteDeferred).done(function (returnData) {

      if (returnData.data.proSite != null) {
        proSite = returnData.data.proSite.replace("{", "").replace("}", "");
      }
      if (returnData.data.patSite != null) {
        patSite = returnData.data.patSite.replace("{", "").replace("}", "");
      }
      var spDeferred = MCS.VIALogin.GetSP();
      $.when(spDeferred).done(function (returnData) {
        if (returnData != null) {
          if (returnData.data.proFacId != null && returnData.data.proFacId.mcs_facilityid != null) {
            proSite = returnData.data.proFacId.mcs_facilityid.replace("{", "").replace("}", "");
          }
          if (returnData.data.proFacId != null && returnData.data.proFacId.mcs_stationnumber != null) {
            facilityNumber = returnData.data.proFacId.mcs_stationnumber;
          }
        }

        var getPatSiteFacIdandProSiteFacIdDeferred = MCS.VIALogin.GetPatSiteFacIdandProSiteFacId(patSite, proSite);

        $.when(getPatSiteFacIdandProSiteFacIdDeferred).done(function (returnData) {
          patSiteFacId = returnData.data.patSiteFacId;
          proSiteFacId = returnData.data.proSiteFacId;

          if ((proSiteFacId == null) && (Xrm.Page.getAttribute("cvt_providerfacility") != null && Xrm.Page.getAttribute("cvt_providerfacility").getValue() != null)) {
            proSiteFacId = Xrm.Page.getAttribute("cvt_providerfacility").getValue()[0].id;
          }

          var siteFacId = side == "pat" ? patSiteFacId : proSiteFacId;

          var getFacilityNumberOfSiteDeferred = MCS.VIALogin.GetFacilityNumberOfSite(siteFacId);

          $.when(getFacilityNumberOfSiteDeferred).done(function (returnData) {
            returnData.data.Side = side;
            returnData.data.facilityNumber = returnData.data.facilityNumber;
            deferred.resolve(returnData);
          });
        });
      });
    });

    return deferred.promise();
  };

  //Returns the duz name or ""
  MCS.VIALogin.ActiveDuz = function (stationNumber, side) {
    console.log("Begin MCS.VIALogin.ActiveDuz " + stationNumber + " " + side);
    var deferred = $.Deferred();
    var returnDataActiveDuz = {
      success: true,
      Side: side,
      data: {
      }
    };

    var getUserDuzRecordDeferred = MCS.VIALogin.GetUserDuzRecord(stationNumber, true, side);
    $.when(getUserDuzRecordDeferred).done(function (returnData) {
      returnDataActiveDuz.data.duzRecord = returnData.data.duzRecord;
      deferred.resolve(returnDataActiveDuz);
    });

    return deferred.promise();
  };

  //Retrieves the actual duzRecord - sets global variable
  MCS.VIALogin.GetUserDuzRecord = function (stationNumber, isActive, side) {
    console.log("Begin MCS.VIALogin.GetUserDuzRecord " + stationNumber + " " + isActive + " " + side);
    var deferred = $.Deferred();
    var returnDataGetDuz = {
      success: true,
      Side: side,
      data: {
      }
    };

    if (typeof Xrm == "undefined") Xrm = parent.Xrm;
    var varState = 0;
    if (!isActive) varState = 1;

    var userGUID = MCS.cvt_Common.TrimBookendBrackets(Xrm.Page.context.getUserId());
    var filter = "statecode eq " + varState + " and cvt_stationnumber eq '" + stationNumber + "' and _cvt_user_value eq " + userGUID;
    var select = "cvt_name, statecode, cvt_expirationdate, cvt_userduzid, cvt_stationnumber";

    var duzRecord = null;
    Xrm.WebApi.retrieveMultipleRecords("cvt_userduz", "?$select=" + select + "&$filter=" + filter).then
      (
        function success(results) {
          if (results && results.entities && results.entities != null) {
            if (results.entities.length === 0) {
              console.log("Calling LoginWithAction with side: " + side);
              duzRecordDeferred = MCS.VIALogin.LoginWithAction(stationNumber, side, "", "");
              $.when(duzRecordDeferred).done(function (returnData) {
                returnDataGetDuz.data.duzRecord = returnData.data.duzRecord;
                deferred.resolve(returnDataGetDuz);
              });
            }
            else if (results.entities.length === 1) {
              duzRecord = results.entities[0];
              if (side === "pat") MCS.VIALogin.PatDuzRecord = duzRecord;
              else if (side === "pro") MCS.VIALogin.ProDuzRecord = duzRecord;
              else if (side === "both") {
                MCS.VIALogin.PatDuzRecord = duzRecord;
                MCS.VIALogin.ProDuzRecord = duzRecord;
              }
              returnDataGetDuz.data.duzRecord = duzRecord;
              deferred.resolve(returnDataGetDuz);
            }
            else {
              //if active, get most recently modified?
              //if inactive, then get most recently modified?
              duzRecord = results.entities[0];

              if (duzRecord.cvt_stationnumber == MCS.VIALogin.patFacilityNumber && !MCS.VIALogin.isPatientSiteFacCerner) {
                MCS.VIALogin.ToggleIcon("PatDuz", "failure", "More than 1 DUZ for that User/StationNumber (" + duzRecord.cvt_StationNumber + ") combination found.  Only 1 or 0 records should be found. Using first one returned.");
              }
              if (duzRecord.cvt_stationnumber == MCS.VIALogin.proFacilityNumber && !MCS.VIALogin.isProviderSiteFacCerner) {
                MCS.VIALogin.ToggleIcon("ProDuz", "failure", "More than 1 DUZ for that User/StationNumber (" + duzRecord.cvt_StationNumber + ") combination found.  Only 1 or 0 records should be found. Using first one returned");
              }

              if (side === "pat") MCS.VIALogin.PatDuzRecord = duzRecord;
              else if (side === "pro") MCS.VIALogin.ProDuzRecord = duzRecord;
              else if (side === "both") {
                MCS.VIALogin.PatDuzRecord = duzRecord;
                MCS.VIALogin.ProDuzRecord = duzRecord;
              }

              returnDataGetDuz.data.duzRecord = duzRecord;
              deferred.resolve(returnDataGetDuz);
            }
          }
        },
        function (error) {
          //var errorMessage = MCS.cvt_Common.RestError(error.message);
          console.log("Failed to retrieve Duz: "); // + errorMessage);

          returnDataGetDuz.data.error = "Get User Duz Record"; // errorMessage;
          returnDataGetDuz.success = false;

          deferred.resolve(returnDataGetDuz);
        }
      );

    return deferred.promise();
  };
}

/////////////////////EC ViaLogin  //main method to log into VIA using VIMT EC (through Actions/CWF/LOB)
// ************************************************DUZ Getting Functions****************************************************
//Access and Verify input were always undefined, so set to ""
{
  MCS.VIALogin.ViaLoginVimt = function () {
    console.log("Begin MCS.VIALogin.ViaLoginVimt");
    var duzSideValue = MCS.VIALogin.DuzSide();
    var vimtDeferred = $.Deferred();
    var facilityArray = [];
    var returnData = {
      success: false,
      data: {}
    };

    if (duzSideValue == "both") {
      if (MCS.VIALogin.patFacilityNumber == null) {
        var getFacilityNumberDeferred = MCS.VIALogin.GetFacilityNumber("pat");
        facilityArray.push(getFacilityNumberDeferred);
      }

      if (MCS.VIALogin.proFacilityNumber == null) {
        var getFacilityNumberDeferred = MCS.VIALogin.GetFacilityNumber("pro");
        facilityArray.push(getFacilityNumberDeferred);
      }

      console.log("Facility Array Length: " + facilityArray.length);

      $.when.all(facilityArray).then(function (returnData) {
        console.log("Facility Array done");
        for (n = 0; n < returnData.length; n++) {
          if (typeof returnData[n].data.Side !== 'undefined' && returnData[n].data.Side == "pro") {
            MCS.VIALogin.proFacilityNumber = returnData[n].data.facilityNumber;
          }
          if (typeof returnData[n].data.Side !== 'undefined' && returnData[n].data.Side == "pat") {
            MCS.VIALogin.patFacilityNumber = returnData[n].data.facilityNumber;
          }
        }

        vimtDeferred.resolve(returnData);
      });
    }
    else {
      var stationNumber = duzSideValue == "pro" ? MCS.VIALogin.proFacilityNumber : MCS.VIALogin.patFacilityNumber;
      var proLoginWithActionDeferred = MCS.VIALogin.LoginWithAction(stationNumber, duzSideValue, "", "");
      $.when(proLoginWithActionDeferred).done(function (returnData) {
        vimtDeferred.resolve(returnData);
      });
    }
    return vimtDeferred.promise();
  };

  //Calls the action and submits the resulting DUZ or else calls the Login through Access-Verify codes
  MCS.VIALogin.LoginWithAction = function (stationNumber, side, access, verify) {
    console.log("Begin MCS.VIALogin.LoginWithAction " + stationNumber + " " + side);
    var deferred = $.Deferred();
    var returnDataLoginWithAction = {
      success: true,
      Side: side,
      data: {
      }
    };
    var isAV = (access != "" && verify != "") ? true : false;
    var loginType = isAV ? "VistA login with Access/Verify" : "VistA login with SAML";
    if (parent.Xrm.Page.context.getClientUrl().search("pssc") > -1) {
      if (isAV) {
        var setUserDuzDeferred = MCS.VIALogin.SetUserDuz(stationNumber, "FakeUserDuzAV", side);

        $.when(setUserDuzDeferred).done(function (returnData) {
          console.log("MCS.VIALogin.LoginWithAction isAV " + returnData.success);
          // should have been set, verify returnData.success
          returnDataLoginWithAction.data = returnData.data;
          deferred.resolve(returnDataLoginWithAction);
        });
      }
      else {
        if (MCS.VIALogin.TestingAV != side) {
          var setUserDuzDeferred = MCS.VIALogin.SetUserDuz(stationNumber, "FakeUserDuzSAML" + side, side);

          $.when(setUserDuzDeferred).done(function (returnData) {
            console.log("MCS.VIALogin.LoginWithAction TestingAV " + returnData.success);
            // should have been set, verify returnData.success
            returnDataLoginWithAction.data = returnData.data;
            deferred.resolve(returnDataLoginWithAction);
          });
        }
        else {
          switch (side) {
            case "pat":
              if (!MCS.VIALogin.isPatientSiteFacCerner) {
                $("#PatAV").show();
                MCS.VIALogin.ToggleIcon("PatDuz", "failure", "Failed VistA login with SAML. Try Access/Verify codes for patient (" + stationNumber + ").");
              }
              break;
            case "pro":
              if (!MCS.VIALogin.isProviderSiteFacCerner) {
                $("#ProAV").show();
                MCS.VIALogin.ToggleIcon("ProDuz", "failure", "Failed VistA login with SAML. Try Access/Verify codes for provider (" + stationNumber + ").");
              }
              break;
            case "both":
              if (!MCS.VIALogin.isPatientSiteFacCerner && !MCS.VIALogin.isProviderSiteFacCerner) {
                $("#ProAV").show();
                MCS.VIALogin.ToggleIcon("ProDuz", "failure", "Failed VistA login with SAML. Try Access/Verify codes for intrafacility (" + stationNumber + ").");
              }
              break;
          }
        }
      }
      deferred.resolve(returnDataLoginWithAction);
    }

    /*var requestName = "cvt_ViaLoginAction";
    var requestParams =
        [{
            Key: "StationNumber",
            Type: MCS.Scripts.Process.DataType.String,
            Value: stationNumber
        },
        {
            Key: "SamlToken",
            Type: MCS.Scripts.Process.DataType.String,
            Value: MCS.VIALogin.SamlString
        },
        {
            Key: "AccessCode",
            Type: MCS.Scripts.Process.DataType.String,
            Value: typeof access === "undefined" ? "" : access
        },
        {
            Key: "VerifyCode",
            Type: MCS.Scripts.Process.DataType.String,
            Value: typeof verify === "undefined" ? "" : verify
        }];*/

    var LoginWithActionRequest = {
      StationNumber: stationNumber,
      SamlToken: MCS.VIALogin.SamlString,
      AccessCode: typeof access === "undefined" ? "" : access,
      VerifyCode: typeof verify === "undefined" ? "" : verify,

      getMetadata: function () {
        return {
          boundParameter: null,
          parameterTypes: {
            "StationNumber": {
              "typeName": "Edm.String",
              "structuralProperty": 1
            },
            "SamlToken": {
              "typeName": "Edm.String",
              "structuralProperty": 1
            },
            "AccessCode": {
              "typeName": "Edm.String",
              "structuralProperty": 1
            },
            "VerifyCode": {
              "typeName": "Edm.String",
              "structuralProperty": 1
            }
          },
          operationType: 0,
          operationName: "cvt_ViaLoginAction"
        };
      }
    };

    Xrm.WebApi.online.execute(LoginWithActionRequest)
      .then(function (response) {
        if (response.ok) {
          response.json().then(
            function (responseBody) {
              console.log("Begin MCS.Scripts.Process.ExecuteAction " + side);
              console.log("Begin MCS.Scripts.Process.ExecuteAction - Response " + responseBody);
              var userDuz = responseBody.UserDuz;
              var success = responseBody.SuccessfulLogin;
              var errorMessage = responseBody.ErrorMessage;

              if (success) {
                var setUserDuzDeferred = MCS.VIALogin.SetUserDuz(stationNumber, userDuz, side);
                $.when(setUserDuzDeferred).done(function (returnData) {
                  // should have been set, verify returnData.success
                  if ((Xrm.Page.getAttribute("cvt_patuserduz") == null & parent.Xrm.Page.getAttribute("cvt_patuserduz") != null))
                    Xrm = parent.Xrm;

                  console.log("Provider Facility Number: " + MCS.VIALogin.proFacilityNumber);
                  console.log("Patient Facility Number: " + MCS.VIALogin.patFacilityNumber);
                  console.log("Station Number: " + stationNumber);

                  if (stationNumber == MCS.VIALogin.proFacilityNumber) {
                    Xrm.Page.getAttribute("cvt_prouserduz").setValue(userDuz);
                    Xrm.Page.getAttribute("cvt_providerstationcode").setValue(MCS.VIALogin.proFacilityNumber);
                    if (!MCS.VIALogin.isProviderSiteFacCerner)
                      MCS.VIALogin.ToggleIcon("ProDuz", "success", loginType + " successful for " + stationNumber + ".");
                  }
                  if (stationNumber == MCS.VIALogin.patFacilityNumber) {
                    Xrm.Page.getAttribute("cvt_patuserduz").setValue(userDuz);
                    if (!MCS.VIALogin.isPatientSiteFacCerner)
                      MCS.VIALogin.ToggleIcon("PatDuz", "success", loginType + " successful for " + stationNumber + ".");
                  }

                  //Trigger the Get Consults for Intrafacility or one of the facility is VistA in Inter Facility
                  //if ((stationNumber === MCS.VIALogin.proFacilityNumber || MCS.VIALogin.proFacilityNumber === null) && (stationNumber === //MCS.VIALogin.patFacilityNumber || MCS.VIALogin.patFacilityNumber === null)) 
                  if (!MCS.VIALogin.isPatientSiteFacCerner || !MCS.VIALogin.isProviderSiteFacCerner) {
                    MCS.VIALogin.TriggerGetConsults();
                  }
                  else {
                    var stationNumberToVerify = null;

                    if (MCS.VIALogin.proFacilityNumber !== null && MCS.VIALogin.proFacilityNumber !== undefined && stationNumber !== MCS.VIALogin.proFacilityNumber) {
                      stationNumberToVerify = MCS.VIALogin.proFacilityNumber;
                    }
                    else if (MCS.VIALogin.patFacilityNumber !== null && MCS.VIALogin.patFacilityNumber !== undefined && stationNumber !== MCS.VIALogin.patFacilityNumber) {
                      stationNumberToVerify = MCS.VIALogin.patFacilityNumber;
                    }

                    if (stationNumberToVerify !== null) {
                      var isValidUserDuzDeferred = MCS.VIALogin.VerifyUserDuzRecord(stationNumberToVerify, true);


                      $.when(isValidUserDuzDeferred).done(function (isValidUserDuz) {
                        if (isValidUserDuz)
                          MCS.VIALogin.TriggerGetConsults();
                      });
                    }
                  }

                  // should have been set, verify returnData.success
                  returnDataLoginWithAction.data = returnData.data;
                  deferred.resolve(returnDataLoginWithAction);
                });
              }
              else {
                //Intrafacility
                if (MCS.VIALogin.patFacilityNumber == MCS.VIALogin.proFacilityNumber && !MCS.VIALogin.isProviderSiteFacCerner) {
                  $("#ProAV").show();
                  MCS.VIALogin.ToggleIcon("ProDuz", "failure", loginType + " failed for " + stationNumber + ". Try Access/Verify codes. Error Details: " + errorMessage);
                }
                else { //Interfacility, SFT, or H/M
                  //Pro
                  if (stationNumber == MCS.VIALogin.proFacilityNumber && !MCS.VIALogin.isProviderSiteFacCerner) {
                    $("#ProAV").show();
                    MCS.VIALogin.ToggleIcon("ProDuz", "failure", loginType + " failed for " + stationNumber + ". Try Access/Verify codes. Error Details: " + errorMessage);
                  }

                  //Pat
                  if (stationNumber == MCS.VIALogin.patFacilityNumber && !MCS.VIALogin.isPatientSiteFacCerner) {
                    $("#PatAV").show();
                    MCS.VIALogin.ToggleIcon("PatDuz", "failure", loginType + " failed for " + stationNumber + ". Try Access/Verify codes. Error Details: " + errorMessage);
                  }
                }

                MCS.VIALogin.ToggleButton(true);
                returnDataLoginWithAction.success = false;
                deferred.resolve(returnDataLoginWithAction);
              }
            });
        }
      },
        function (err) {
          if (stationNumber == MCS.VIALogin.proFacilityNumber && !MCS.VIALogin.isProviderSiteFacCerner) {
            MCS.VIALogin.ToggleIcon("ProDuz", "failure", loginType + " failed for " + stationNumber + ". Error Details: " + err.message);
          }
          if (stationNumber == MCS.VIALogin.patFacilityNumber && !MCS.VIALogin.isPatientSiteFacCerner) {
            MCS.VIALogin.ToggleIcon("PatDuz", "failure", loginType + " failed for " + stationNumber + ". Error Details: " + err.message);
          }

          MCS.VIALogin.ToggleButton(true);
          returnDataLoginWithAction.success = false;
          deferred.resolve(returnDataLoginWithAction);
        });

    return deferred.promise();
  };

  //Verify whether the station has an active duz available and return the boolean value
  MCS.VIALogin.VerifyUserDuzRecord = function (stationNumber, isActive) {
    console.log("Begin MCS.VIALogin.VerifyUserDuzRecord " + stationNumber + " " + isActive);
    var deferred = $.Deferred();

    if (typeof Xrm == "undefined") Xrm = parent.Xrm;
    var varState = 0;
    if (!isActive) varState = 1;

    var userGUID = MCS.cvt_Common.TrimBookendBrackets(Xrm.Page.context.getUserId());
    var filter = "statecode eq " + varState + " and cvt_stationnumber eq '" + stationNumber + "' and _cvt_user_value eq " + userGUID;
    var select = "cvt_name, statecode, cvt_expirationdate, cvt_userduzid, cvt_stationnumber";

    var duzRecord = null;
    Xrm.WebApi.retrieveMultipleRecords("cvt_userduz", "?$select=" + select + "&$filter=" + filter).then
      (
        function success(results) {
          if (results && results.entities && results.entities != null) {
            if (results.entities.length === 0) {
              deferred.resolve(false);
            }
            else if (results.entities.length > 0) {
              deferred.resolve(true);
            }
          }
        },
        function (error) {
          console.log("Failed to Verify Duz: ");
          deferred.resolve(false);
        }
      );

    return deferred.promise();
  };

  //Do we write this new duz to the activity form?
  MCS.VIALogin.SetUserDuz = function (stationNumber, duz, side) {
    console.log("Begin MCS.VIALogin.SetUserDuz " + stationNumber + " " + duz + " " + side);
    var deferred = $.Deferred();
    var returnData = {
      success: true,
      Side: side,
      data: {
      }
    };

    if (typeof Xrm == "undefined") Xrm = parent.Xrm;

    var userDuzRecord = {
      'cvt_name': duz,
      'statecode': 0,
      'statuscode': 1
    };

    var tempDuz = null;
    if (side === "pat") tempDuz = MCS.VIALogin.PatDuzRecord;
    else if (side === "pro") tempDuz = MCS.VIALogin.ProDuzRecord;

    if (tempDuz) {
      if (tempDuz.cvt_userduzid != null) {
        if (tempDuz.cvt_name != duz) {
          Xrm.WebApi.updateRecord("cvt_userduz", tempDuz.cvt_userduzid, userDuzRecord).then(
            function success(result1) {
              Xrm.WebApi.retrieveRecord("cvt_userduz", result1.id, "?$select=*").then(
                function success(result2) {
                  if (side === "pat") MCS.VIALogin.PatDuzRecord = result2;
                  else if (side === "pro") MCS.VIALogin.ProDuzRecord = result2;
                  else if (side === "both") {
                    MCS.VIALogin.PatDuzRecord = result2;
                    MCS.VIALogin.ProDuzRecord = result2;
                  }
                  returnData.data.duzRecord = result2;
                  deferred.resolve(returnData);
                },
                function (error) {
                  console.log("Error Retrieving User Duz after update");
                  deferred.resolve(returnData);
                }
              );
            },
            function (error) {
              //var errorMessage = MCS.cvt_Common.RestError(error);
              console.log("Error Updating User Duz Login Information: "); // + errorMessage);
              returnData.success = false;
              deferred.resolve(returnData);
            }
          );
        }
      }
      else if (tempDuz.id != null) {
        if (tempDuz.id != duz) {
          Xrm.WebApi.updateRecord("cvt_userduz", tempDuz.id, userDuzRecord).then(
            function success(result1) {
              Xrm.WebApi.retrieveRecord("cvt_userduz", result1.id, "?$select=*").then(
                function success(result2) {
                  if (side === "pat") MCS.VIALogin.PatDuzRecord = result2;
                  else if (side === "pro") MCS.VIALogin.ProDuzRecord = result2;
                  else if (side === "both") {
                    MCS.VIALogin.PatDuzRecord = result2;
                    MCS.VIALogin.ProDuzRecord = result2;
                  }

                  returnData.data.duzRecord = result2;
                  deferred.resolve(returnData);
                },
                function (error) {
                  console.log("Error Retrieving User Duz after update");
                  deferred.resolve(returnData);
                }
              );
            },
            function (error) {
              //var errorMessage = MCS.cvt_Common.RestError(error);
              console.log("Error Updating User Duz Login Information: "); // + errorMessage);
              returnData.success = false;
              deferred.resolve(returnData);
            }
          );
        }
      }
    }
    else if (tempDuz == null) {
      userDuzRecord["cvt_stationnumber"] = stationNumber;
      userDuzRecord["cvt_user@odata.bind"] = "/systemusers(" + Xrm.Page.context.getUserId().replace("{", "").replace("}", "") + ")";

      //Create record from temp object
      console.log("Before saving Duz");
      Xrm.WebApi.createRecord("cvt_userduz", userDuzRecord).then(
        function success(result1) {
          console.log("After saving Duz");
          console.log("Retrieving Duz");
          Xrm.WebApi.retrieveRecord("cvt_userduz", result1.id, "?$select=*").then(
            function success(result2) {
              console.log("Duz retrieved");

              if (side === "pat") MCS.VIALogin.PatDuzRecord = result2;
              else if (side === "pro") MCS.VIALogin.ProDuzRecord = result2;
              else if (side === "both") {
                MCS.VIALogin.PatDuzRecord = result2;
                MCS.VIALogin.ProDuzRecord = result2;
              }

              returnData.data.duzRecord = result2;
              deferred.resolve(returnData);
            },
            function (error) {
              console.log("Error Retrieving User Duz after creating");
              returnData.success = false;
              deferred.resolve(returnData);
            }
          );
        },
        function (error) {
          //var errorMessage = MCS.cvt_Common.RestError(error);
          console.log("Error Creating User Duz Login Information: "); // + errorMessage);

          returnData.success = false;
          deferred.resolve(returnData);
        }
      );
    }
    else {
      deferred.resolve(returnData);
    }

    return deferred.promise();
  };

  MCS.VIALogin.TriggerGetConsults = function () {
    console.log("Begin MCS.VIALogin.TriggerGetConsults");

    if (Object.keys(Xrm.Utility.getGlobalContext().getQueryStringParameters()).length === 0)
      Xrm = parent.Xrm;

    //var typecode = "4214";
    var typeCode = Xrm.Utility.getGlobalContext().getQueryStringParameters().etc;
    var entityTypeName = Xrm.Utility.getGlobalContext().getQueryStringParameters().entityTypeName;
    typeCode = entityTypeName === "serviceappointment" ? 4214 : 4201;
    if (typeCode == 4214)
      Xrm.Page.getAttribute("cvt_rungetconsults").fireOnChange();
  };

  MCS.VIALogin.PatDuzButton = function () {
    var access = $("#PatientAccessCodeTextBox").val();
    var verify = $("#PatientVerifyCodeTextBox").val();

    if (!MCS.VIALogin.isPatientSiteFacCerner) {
      console.log("Begin MCS.VIALogin.PatDuzButton");
      MCS.VIALogin.ToggleIcon("PatDuz", "working", "Attempting Access/Verify codes.");
      if (access != "" && verify != "") {
        MCS.VIALogin.LoginWithAction(MCS.VIALogin.patFacilityNumber, "pat", access, verify);
        $("#PatAV").hide();
      }
      else MCS.VIALogin.ToggleIcon("PatDuz", "failure", "Please fill out Access and Verify codes then Submit.");
    }
  };

  MCS.VIALogin.ProDuzButton = function () {
    console.log("Begin MCS.VIALogin.ProDuzButton");
    var access = $("#ProviderAccessCodeTextBox").val();
    var verify = $("#ProviderVerifyCodeTextBox").val();

    if (!MCS.VIALogin.isProviderSiteFacCerner) {
      MCS.VIALogin.ToggleIcon("ProDuz", "working", "Attempting Access/Verify codes.");
      if (access != "" && verify != "") {
        MCS.VIALogin.LoginWithAction(MCS.VIALogin.proFacilityNumber, "pro", access, verify);
        $("#ProAV").hide();
      }
      else MCS.VIALogin.ToggleIcon("ProDuz", "failure", "Please fill out Access and Verify codes then Submit.");
    }
  };

  //Helper to re-call LoginVIMT using Access/Verify entered by user prompt - user can leave blank or hit cancel to end loop
  MCS.VIALogin.LoginWithAccessVerify = function (stationNumber, side) {
    console.log("Begin MCS.VIALogin.LoginWithAccessVerify");
    var facilityString = side == "both" ? "the" : "the " + side;
    var accessCode = prompt("Please enter your access code for " + facilityString + " facility (" + stationNumber + ").", "");

    if (accessCode != null && accessCode != "") {
      var verifyCode = prompt("Please enter your verify code for " + facilityString + " facility (" + stationNumber + ").", "");
      if (verifyCode != null && verifyCode != "") MCS.VIALogin.LoginWithAction(stationNumber, side, accessCode, verifyCode);
    }
  };
}

// ************************VistA Switches and Integration checks *************************************
{
  MCS.VIALogin.RunVistaIntegration = function () {
    console.log("Begin MCS.VIALogin.RunVistaIntegration");
    var vistaIntegrationDeferred = $.Deferred();
    var returnData = {
      integrationEnabled: false
    };

    //if (typeof Xrm.Page.ui == "undefined" || Xrm.Page.ui == null) Xrm = parent.Xrm;

    if (Object.keys(Xrm.Utility.getGlobalContext().getQueryStringParameters()).length === 0)
      Xrm = parent.Xrm;

    var typeCode = Xrm.Utility.getGlobalContext().getQueryStringParameters().etc;
    var entityTypeName = Xrm.Utility.getGlobalContext().getQueryStringParameters().entityTypeName;
    typeCode = entityTypeName === "serviceappointment" ? 4214 : 4201;

    //if block resource and theres no related appointment
    if (typeCode == 4201 && Xrm.Page.getAttribute("cvt_serviceactivityid").getValue() == null) {
      returnData.integrationEnabled = false;
      vistaIntegrationDeferred.resolve(returnData);
      return vistaIntegrationDeferred.promise();
    }
    else if (typeCode == 4201 && Xrm.Page.getAttribute("cvt_serviceactivityid").getValue() != null) {
      returnData.integrationEnabled = true;
      vistaIntegrationDeferred.resolve(returnData);
      return vistaIntegrationDeferred.promise();
    }

    //if appointment and no tsa is set
    if (typeCode == 4214 && (Xrm.Page.getAttribute("cvt_relatedschedulingpackage") == null || Xrm.Page.getAttribute("cvt_relatedschedulingpackage").getValue() == null)) {
      MCS.VIALogin.hideAll();
      MCS.VIALogin.ToggleIcon("SAML", "failure", "Please select a Scheduling Package.");
      returnData.integrationEnabled = false;
      vistaIntegrationDeferred.resolve(returnData);
      return vistaIntegrationDeferred.promise();
    }

    //if appt and clinic based Group
    if (typeCode == 4214 && (!Xrm.Page.getAttribute("cvt_type").getValue() && Xrm.Page.getAttribute("mcs_groupappointment").getValue())) {
      //vistaIntegrationDeferred.resolve(true);
      returnData.integrationEnabled = true;
      vistaIntegrationDeferred.resolve(returnData);
      return vistaIntegrationDeferred.promise();
    }

    var stateCode = Xrm.Page.getAttribute('statecode').getValue();

    //if not active
    if (stateCode != 3 && Xrm.Page.ui.getFormType() != 1) {
      returnData.integrationEnabled = false;
      vistaIntegrationDeferred.resolve(returnData);
      return vistaIntegrationDeferred.promise();
    }

    var switchesDeferred = MCS.VIALogin.CheckVistaSwitches();

    $.when(switchesDeferred).done(function (data) {

      //Note that this is a triple equal, not a double, so a null value is considered acceptable to continue, only a false will mean "don't show Get Consults"
      //////debugger;
      if (data.baseSwitchConfig === false) {
        returnData.integrationEnabled = false;
        vistaIntegrationDeferred.resolve(returnData);
        return vistaIntegrationDeferred.promise();
      }

      var appointmentSwitchDeferred;

      if (typeof Xrm != "undefined")
        appointmentSwitchDeferred = MCS.VIALogin.AppointmentTypeSwitchCheck(data.hmConfig, data.ifcConfig, data.singleNonHmConfig, Xrm);
      else
        appointmentSwitchDeferred = MCS.VIALogin.AppointmentTypeSwitchCheck(data.hmConfig, data.ifcConfig, data.singleNonHmConfig, parent.Xrm);

      $.when(appointmentSwitchDeferred).done(function (returnData) {
        var returnData = {
          integrationEnabled: true
        };

        vistaIntegrationDeferred.resolve(returnData);
      });
    });

    return vistaIntegrationDeferred.promise();
  };

  MCS.VIALogin.CheckVistaSwitches = function () {
    console.log("Begin MCS.VIALogin.CheckVistaSwitches");
    var deferred = $.Deferred();
    var VistaSwitchesConfig = true;
    var baseSwitchConfig = true;
    var hmConfig = true;
    var ifcConfig = true;
    var singleNonHmConfig = true;
    var filter = "mcs_name eq 'Active Settings'";
    var select = 'cvt_usevvshomemobile, cvt_usevvsinterfacility, cvt_usevvssingleencounternonhomemobile';

    Xrm.WebApi.retrieveMultipleRecords("mcs_setting", "?$select=*" + "&$filter=" + filter).then(
      function success(results) {
        if (results && results.entities && results.entities != null && results.entities.length != 0) {
          var record = results.entities[0];
          //we want this to return true if the appointment contains a VistA facility
          //////debugger;
          baseSwitchConfig = record.cvt_usevistaintegration == null ? false : record.cvt_usevistaintegration;
          hmConfig = record.cvt_usevvshomemobile == null ? false : record.cvt_usevvshomemobile;
          ifcConfig = record.cvt_usevvsinterfacility == null ? false : record.cvt_usevvsinterfacility;
          singleNonHmConfig = record.cvt_usevvssingleencounternonhomemobile == null ? false : record.cvt_usevvssingleencounternonhomemobile;

          var returnData = {
            record: record,
            baseSwitchConfig: baseSwitchConfig,
            hmConfig: hmConfig,
            ifcConfig: ifcConfig,
            singleNonHmConfig: singleNonHmConfig
          };

          deferred.resolve(returnData);
        }
      },
      function (error) {

        console.log('Error in fn  MCS.VIALogin.CheckVistaSwitches: ' + e);
        // return VistaSwitchesConfig;
        deferred.resolve(VistaSwitchesConfig);
      }
    );

    return deferred.promise();
  };

  MCS.VIALogin.RetrieveTSADetails = function (tsaId) {
    console.log("Begin MCS.VIALogin.RetrieveTSADetails");
    var deferred = $.Deferred();

    // Retrieving the TSA details   
    Xrm.WebApi.retrieveRecord('cvt_resourcepackage', tsaId, "?$select=_cvt_specialty_value, _cvt_specialtysubtype_value").then(
      function success(result) {
        if (result != null) {
          var returnData = {
            success: true,
            data: []
          };
          if (typeof result._cvt_specialty_value != 'undefined' && result._cvt_specialty_value !== null)
            returnData.data.specialty = result._cvt_specialty_value;

          if (typeof result._cvt_specialtysubtype_value != 'undefined' && result._cvt_specialtysubtype_value !== null)
            returnData.data.subSpecialty = result._cvt_specialtysubtype_value;

          deferred.resolve(returnData);
        }
      },
      function (error) {
        console.log("failed Scheduling Package check, defaulting to display Consults"); // + MCS.cvt_Common.RestError(error));

        var returnData = {
          success: false,
          error: "failed Scheduling Package check, defaulting to display Consults" //MCS.cvt_Common.RestError(error)
        };

        deferred.resolve(returnData);
      }
    );

    return deferred.promise();
  };

  MCS.VIALogin.GetSpecialtySwitch = function (specialty) {
    console.log("Begin MCS.VIALogin.GetSpecialtySwitch");
    var deferred = $.Deferred();
    var returnData = {
      success: false,
      data: {}
    };

    //Checking Specialty Switch
    var specialtySwitch = true;

    if (specialty != null) {

      Xrm.WebApi.retrieveRecord('mcs_servicetype', specialty, "?$select=cvt_usevvs").then(
        function success(result) {
          specialtySwitch = (result.cvt_usevvs == null ? false : result.cvt_usevvs);
          returnData.data = specialtySwitch;

          if (!specialtySwitch) {
            returnData.success = false;
            deferred.resolve(returnData);
          }

          returnData.success = true;
          deferred.resolve(specialtySwitch);
        },
        function (error) {
          console.log("Failed specialty type check, defaulting to display Consults"); // + MCS.cvt_Common.RestError(error));

          returnData.success = false;
          returnData.data = "Failed specialty type check, defaulting to display Consults"; //MCS.cvt_Common.RestError(err);
          deferred.resolve(returnData);
        }
      );
    }

    return deferred.promise();
  };

  MCS.VIALogin.GetSubSpecialtySwitch = function (subSpecialty) {
    console.log("Begin MCS.VIALogin.GetSubSpecialtySwitch");
    var deferred = $.Deferred();
    var returnData = {
      success: false,
      data: {}
    };

    //Checking Specialty Sub-type Switch
    var subSpecialtySwitch = true;

    if (subSpecialty != null) {
      Xrm.WebApi.retrieveRecord('mcs_servicesubtype', subSpecialty, '?$select=cvt_usevvs'.then(
        function success(result) {
          subSpecialtySwitch = (result.cvt_usevvs == null ? false : result.cvt_usevvs);

          returnData.data = subSpecialtySwitch;

          if (!subSpecialtySwitch) {
            returnData.success = false;
            deferred.resolve(returnData);
          }

          returnData.success = true;
          deferred.resolve(returnData);
        },
        function (error) {
          common.log("failed sub-specialty type check, looking for specialty switch: "); // + MCS.cvt_Common.RestError(err));

          returnData.success = false;
          returnData.data = "failed sub-specialty type check, looking for specialty switch: "; //MCS.cvt_Common.RestError(err);
          deferred.resolve(returnData);
        }
      ));
    }

    return deferred.promise();
  };

  MCS.VIALogin.GetUseVistaIntrafacility = function (siteFacId) {
    console.log("Begin MCS.VIALogin.GetUseVistaIntraFacility");
    var deferred = $.Deferred();

    var returnData = {
      success: true,
      data: {}
    };

    Xrm.WebApi.retrieveRecord('mcs_facility', siteFacId, "?$select=cvt_usevistaintrafacility").then(
      function success(result) {
        if (result != null) {
          patFacilitySwitch = (result.cvt_usevistaintrafacility == null ? false : result.cvt_usevistaintrafacility);
          returnData.data.patFacilitySwitch = patFacilitySwitch;
        }

        deferred.resolve(returnData);
      },
      function (error) {
        common.log("failed Patient facility check, defaulting to display Consults"); // + MCS.cvt_Common.RestError(error));
        returnData.success = false;
        deferred.resolve(returnData);
      }
    );

    return deferred.promise();
  };

  MCS.VIALogin.AppointmentTypeSwitchCheckHelper = function (tsaId, isHomeMobile, hmConfig, sftConfig, isSft) {
    console.log("Begin MCS.VIALogin.AppointmentTypeSwitchCheckHelper");
    var mainDeferred = $.Deferred();

    var returnData = {
      success: true,
      data: {}
    };

    if (tsaId != null) {
      //Checking MAIN H/M Switch

      if (isHomeMobile && !hmConfig) {
        // If Home Mobile and Main switch for VA Video Connect Encounter is turned off then return false
        returnData.success = false;
        mainDeferred.resolve(returnData);
      }

      //Checking MAIN SFT Switch
      if (isSft && !sftConfig) {
        // If SFT and Main switch for SFT Encounter is turned off then return false
        // mainDeferred.resolve(false);

        returnData.success = false;
        mainDeferred.resolve(returnData);
      }

      var retrieveTSADetailsDeferred = MCS.VIALogin.RetrieveTSADetails(tsaId);

      $.when(retrieveTSADetailsDeferred).done(function (returnData) {

        if (returnData.success) {

          var specialty = returnData.data.specialty;
          var subspecialty = returnData.data.subSpecialty;
          var getSpecialtySwitchDeferred = MCS.VIALogin.GetSpecialtySwitch(specialty);

          $.when(getSpecialtySwitchDeferred).done(function (returnData) {
            if (returnData.success !== true) {

              // Failed specialty type check, defaulting to display Consults
              returnData.success = false;
              mainDeferred.resolve(returnData);
            }
            else {
              var getSubSpecialtySwitchDeferred = MCS.VIALogin.GetSubSpecialtySwitch(subspecialty);

              $.when(getSubSpecialtySwitchDeferred).done(function (returnData) {
                if (subspecialtySwitchData.success !== true) {

                  // failed sub-specialty type check, looking for specialty switch:
                  returnData.success = false;
                  mainDeferred.resolve(returnData);
                }
              });
            }
          });
        }
        else {
          // TSA detail check failed 
          returnData.success = false;
          mainDeferred.resolve(returnData);
        }
      });

      var typeCode = Xrm.Utility.getGlobalContext().getQueryStringParameters().etc;
      var entityTypeName = Xrm.Utility.getGlobalContext().getQueryStringParameters().entityTypeName;
      typeCode = entityTypeName === "serviceappointment" ? 4214 : 4201;

      if (typeCode != 4201) {
        //Check Facility Switch
        var patSite = Xrm.Page.getAttribute("mcs_relatedsite").getValue();
        var proSite = Xrm.Page.getAttribute("mcs_relatedprovidersite").getValue();
      }

      var siteId;

      if (patSite != null) {
        siteId = patSite[0].id;
        var siteFacId = null;

        if (patSite != null) {

          var getSiteIdDeferred = MCS.VIALogin.getPatSiteFacId(siteId, false);

          $.when(getSiteIdDeferred).done(function (returnData) {
            if (returnData.success !== true) {
              alert("failed TMP Site check, Unable to perform VIA login ");
              returnData.success = false;
              mainDeferred.resolve(returnData);
            }
            else if (returnData.success === true) {
              siteFacId = returnData.data.patSiteFacId;
            }

            var patFacilitySwitch = true;
            var getUseVistaIntrafacilityDeferred = MCS.VIALogin.GetUseVistaIntrafacility(siteFacId);

            $.when(getUseVistaIntrafacilityDeferred).done(function (returnData) {
              if (returnData.success === true) {
                patFacilitySwitch = returnData.data.patFacilitySwitch;
              }

              if (!patFacilitySwitch) {
                returnData.success = false;
                mainDeferred.resolve(returnData);
              }
            });

          });
        }
      }
      else if (proSite != null) {
        siteId = proSite[0].id;
        var siteFacId = null;

        var getSiteIdDeferred = MCS.VIALogin.getPatSiteFacId(siteId, false);

        $.when(getSiteIdDeferred).done(function (returnData) {
          if (returnData.success !== true) {
            alert("failed TMP Site check, Unable to perform VIA login ");
            returnData.success = false;
            mainDeferred.resolve(returnData);
          }
          else if (returnData.success === true) {
            siteFacId = returnData.data.patSiteFacId;
          }

          var proFacilitySwitch = true;
          var getUseVistaIntrafacilityDeferred = MCS.VIALogin.GetUseVistaIntrafacility(siteFacId);

          $.when(getUseVistaIntrafacilityDeferred).done(function (returnData) {
            if (returnData.success === true) {
              proFacilitySwitch = returnData.data.patFacilitySwitch;
            }

            if (!proFacilitySwitch) {
              returnData.success = false;
              mainDeferred.resolve(returnData);
            }
          });
        });
      }
    }

    return mainDeferred.promise();
  };

  //Checking for switches in the following order
  //MAIN H/M --> MAIN SFT --> Record Specialty --> Record Specialty SubType --> Record Provider Facility --> MAIN Interfacility --> Record Patient Facility
  MCS.VIALogin.AppointmentTypeSwitchCheck = function (hmConfig, ifcConfig, sftConfig, Xrm) {
    var switchCheckDeferred = $.Deferred();
    var returnData = {
      success: true,
      data: {}
    };

    console.log("Begin MCS.VIALogin.AppointmentTypeSwitchCheck");
    var patFacId = null;
    var proFacId = null;
    var subSpecialty = null;
    var specialty = null;
    var isInterFacility = false;
    var tsaId = null;
    var isHomeMobile = null;
    var isSft = null;

    //check for Appt vs BR
    var typeCode = Xrm.Utility.getGlobalContext().getQueryStringParameters().etc;
    var entityTypeName = Xrm.Utility.getGlobalContext().getQueryStringParameters().entityTypeName;
    typeCode = entityTypeName === "serviceappointment" ? 4214 : 4201;

    //BR
    if (typeCode == 4201 && Xrm.Page.getAttribute("cvt_serviceactivityid").getValue() != null) {
      //Retrieve the SA, so we can get the tsa
      Xrm.WebApi.retrieveRecord("ServiceAppointment", Xrm.Page.getAttribute("cvt_serviceactivityid").getValue()[0].id, "?$select=cvt_relatedschedulingpackage, cvt_type, cvt_telehealthmodality").then(
        function success(result) {

          if (result != null) {
            //Set the TSA ID
            if (result.cvt_relatedschedulingpackage != null) tsaId = result.cvt_relatedschedulingpackage.Id;

            //Set H/M
            if (data.d.cvt_Type != null) isHomeMobile = result.cvt_Type;
            else isHomeMobile = false;

            //Set SFT
            if (data.d.cvt_TelehealthModality != null) isSft = result.cvt_telehealthmodality;
            else isSft = false;

            var appointmentTypeSwitchCheckHelperDeferred = MCS.VIALogin.AppointmentTypeSwitchCheckHelper(tsaId, isHomeMobile, hmConfig, sftConfig, isSft);

            $.when(appointmentTypeSwitchCheckHelperDeferred).done(function (returnData) {
              console.log(' MCS.VIALogin.AppointmentTypeSwitchCheck ' + returnData.success);
              switchCheckDeferred.resolve(returnData);
            });
          }
        },
        function (error) {
          console.log("Failure at 'MCS.VIALogin.AppointmentTypeSwitchCheck' error is: " + error.message);
          returnData.success = false;
          switchCheckDeferred.resolve(returnData);
        }
      );
    }

    //Appt so use this record
    else if (typeCode == 4214) {
      {
        if ((Xrm.Page.getAttribute("cvt_relatedschedulingpackage")) && (Xrm.Page.getAttribute("cvt_relatedschedulingpackage").getValue() != null))
          tsaId = Xrm.Page.getAttribute("cvt_relatedschedulingpackage").getValue()[0].id;

        isHomeMobile = Xrm.Page.getAttribute("cvt_type").getValue();
        isSft = Xrm.Page.getAttribute("cvt_telehealthmodality").getValue();

        var appointmentTypeSwitchCheckHelperDeferred = MCS.VIALogin.AppointmentTypeSwitchCheckHelper(tsaId, isHomeMobile, hmConfig, sftConfig, isSft);

        $.when(appointmentTypeSwitchCheckHelperDeferred).done(function (returnData) {
          switchCheckDeferred.resolve(returnData);
        });
      }
    }

    return switchCheckDeferred.promise();
    //tsa obj is null
  };
}

// ************************Helper methods to retrieve Integration Settings (URLS) *************************************
{
  //Helper method to get any integration setting with the passed in name
  MCS.VIALogin.GetSetting = function (name) {
    console.log("Begin MCS.VIALogin.GetSetting");
    var deferred = $.Deferred();
    var returnData = {
      success: true,
      data: {}
    };
    var value = "";
    var filter = "mcs_name eq '" + name + "'";

    Xrm.WebApi.retrieveMultipleRecords("mcs_integrationsetting", "?$select=mcs_value" + "&$filter=" + filter).then(
      function success(results) {
        if (results && results.entities && results.entities != null && results.entities.length > 0) {
          value = results.entities[0].mcs_value;
          returnData.data.value = value;
        }

        deferred.resolve(returnData);
      },
      function (error) {
        returnData.success = false;
        deferred.resolve(returnData);
      }
    );

    return deferred.promise();
  };

  //Helper Method to get SAML Token URL
  MCS.VIALogin.GetStsUrl = function () {
    console.log("Begin MCS.VIALogin.GetStsUrl");
    var deferred = $.Deferred();

    var returnData = {
      success: true,
      data: {}
    };

    var defaultUrl = "";// "https://dev.services.eauth.va.gov:9301/STS/RequestSecurityToken";
    var url = "";

    var urlDeferred = MCS.VIALogin.GetSetting("IAM SAML Token Endpoint");

    $.when(urlDeferred).done(function (returnData) {
      if (returnData.data && returnData.data.value) url = returnData.data.value;
      returnData.data.url != "" ? url : defaultUrl;
      MCS.VIALogin.StsUrl = returnData.data.url;
      deferred.resolve(returnData);
    });

    return deferred.promise();
  };
}

//HTML functions
{
  //Hide all Message Divs
  MCS.VIALogin.hideAll = function () {
    console.log("Begin MCS.VIALogin.hideAll");
    MCS.VIALogin.ToggleIcon("SAML", " ", "");
    MCS.VIALogin.ToggleIcon("PatDuz", " ", "");
    MCS.VIALogin.ToggleIcon("ProDuz", " ", "");

    $("#PatAV").hide();
    $("#ProAV").hide();
    if (Xrm.Page.getAttribute("cvt_relatedschedulingpackage") != null) {
      Xrm.Page.getAttribute("cvt_relatedschedulingpackage").removeOnChange(MCS.VIALogin.Login);
      Xrm.Page.getAttribute("cvt_relatedschedulingpackage").addOnChange(MCS.VIALogin.Login);
    }
    else if (parent.Xrm.Page.getAttribute("cvt_relatedschedulingpackage") != null) {
      parent.Xrm.Page.getAttribute("cvt_relatedschedulingpackage").removeOnChange(MCS.VIALogin.Login);
      parent.Xrm.Page.getAttribute("cvt_relatedschedulingpackage").addOnChange(MCS.VIALogin.Login);
    }

    if (Xrm.Page.getAttribute("createdon") != null) {
      Xrm.Page.getAttribute("createdon").removeOnChange(MCS.VIALogin.LoginOnCancelAppointment);
      Xrm.Page.getAttribute("createdon").addOnChange(MCS.VIALogin.LoginOnCancelAppointment);
    }
    else if (parent.Xrm.Page.getAttribute("createdon") != null) {
      parent.Xrm.Page.getAttribute("createdon").removeOnChange(MCS.VIALogin.LoginOnCancelAppointment);
      parent.Xrm.Page.getAttribute("createdon").addOnChange(MCS.VIALogin.LoginOnCancelAppointment);
    }
  }
};

MCS.VIALogin.checkFacilityType = function () {
  debugger;
  var deferred = $.Deferred();
  var returnData = {
    success: true,
    data: {
    }
  };


  var schedulingPackage = null;

  returnData.data.isPatientSiteFacCerner = false;
  MCS.VIALogin.isPatientSiteFacCerner = false;
  returnData.data.isProviderSiteFacCerner = false;
  MCS.VIALogin.isProviderSiteFacCerner = false;
  returnData.data.isBothSiteFacCerner = false;
  MCS.VIALogin.isBothSiteFacCerner = false;
  MCS.VIALogin.hideAll();



  //if activitytypecode==4214 its an appointment so
  //else we have to get the related appointment (if there is one) and get the SPid from that

  var typeCode = Xrm.Utility.getGlobalContext().getQueryStringParameters().etc;
  var entityTypeName = Xrm.Utility.getGlobalContext().getQueryStringParameters().entityTypeName;
  typeCode = entityTypeName === "serviceappointment" ? 4214 : 4201;

  var spId = null;
  if (typeCode == 4214) {
    console.log("setting up for a serviceactivity");
      schedulingPackage = Xrm.Page.getAttribute("cvt_relatedschedulingpackage");

      if (schedulingPackage.getValue() == null) { return; }

    spId = schedulingPackage.getValue()[0].id;

    var _completeFacilityTypeCheck = MCS.VIALogin.CompleteFacilityTypeCheck(spId);
    $.when(_completeFacilityTypeCheck).done(function (returnCFTC) {
      console.log("This is the completed FacilityTypeCheck");
      if (returnCFTC.data.isProviderSiteFacCerner != null && returnCFTC.data.isProviderSiteFacCerner != "") {
        returnData.data.isProviderSiteFacCerner = returnCFTC.data.isProviderSiteFacCerner;
      }
      if (returnCFTC.data.isPatientSiteFacCerner != null && returnCFTC.data.isPatientSiteFacCerner != "") {
        returnData.data.isPatientSiteFacCerner = returnCFTC.data.isPatientSiteFacCerner;
      }

      deferred.resolve(returnData);
    });

  }
  else if (typeCode == 4201) {
    var relatedSA = null;
    console.log("setting up for a reserve resource");
    var rrid = Xrm.Utility.getGlobalContext().getQueryStringParameters().id.replace("{", "").replace("}", "");

    var _relatedSA = MCS.VIALogin.GetRelatedSA(rrid);
    $.when(_relatedSA).done(function (returnSA) {
      console.log("This is a Reserve Resource.  Related SA is: " + returnSA.data.SAID);
      relatedSA = returnSA.data.SAID;

      var _relatedSP = MCS.VIALogin.GetRelatedSchedulingPackage(relatedSA);
      $.when(_relatedSP).done(function (returnSP) {
        console.log("This is a Reserve Resource.  Related SP is: " + returnSP.data.schedulingPackage);
        schedulingPackage = returnSP.data.schedulingPackage;
        spId = returnSP.data.spId;

        var _completeFacilityTypeCheck = MCS.VIALogin.CompleteFacilityTypeCheck(spId);
        $.when(_completeFacilityTypeCheck).done(function (returnCFTC) {
          console.log("This is the completed FacilityTypeCheck");
          if (returnCFTC.data.isProviderSiteFacCerner != null && returnCFTC.data.isProviderSiteFacCerner != "") {
            returnData.data.isProviderSiteFacCerner = returnCFTC.data.isProviderSiteFacCerner;
          }
          if (returnCFTC.data.isPatientSiteFacCerner != null && returnCFTC.data.isPatientSiteFacCerner != "") {
            returnData.data.isPatientSiteFacCerner = returnCFTC.data.isPatientSiteFacCerner;
          }

          deferred.resolve(returnData);
        });

      });

    });
  }

  return deferred.promise();

};

MCS.VIALogin.CompleteFacilityTypeCheck = function (schedulingPackageId) {
  var spId = schedulingPackageId;
  var deferredCFTC = $.Deferred();
  var returnDataCFTC = {
    success: true,
    data: {
    }
  };

  Xrm.WebApi.online.retrieveMultipleRecords("cvt_participatingsite", "?$select=cvt_locationtype,cvt_name,cvt_participatingsiteid&$expand=cvt_facility($select=cvt_facilitytype)&$filter=_cvt_resourcepackage_value eq " + spId).then(
    function success(resCFTC) {

      for (var i = 0; i < resCFTC.entities.length; i++) {
        var cvt_locationtype = resCFTC.entities[i]["cvt_locationtype"];
        if (cvt_locationtype == 917290000 && resCFTC.entities[i].cvt_facility.cvt_facilitytype == 917290000)//Provider & Cerner
        {
          returnDataCFTC.data.isProviderSiteFacCerner = true;
          MCS.VIALogin.isProviderSiteFacCerner = true;
          MCS.VIALogin.ToggleIcon("ProDuz", "success", "Cerner Sites do not require a Duz");
        }
        if (cvt_locationtype == 917290001 && resCFTC.entities[i].cvt_facility.cvt_facilitytype == 917290000)//Patient & Cerner
        {
          returnDataCFTC.data.isPatientSiteFacCerner = true;
          MCS.VIALogin.isPatientSiteFacCerner = true;
          MCS.VIALogin.ToggleIcon("PatDuz", "success", "Cerner Sites do not require a Duz");
        }
      }

      if (MCS.VIALogin.isPatientSiteFacCerner && MCS.VIALogin.isProviderSiteFacCerner) {
        MCS.VIALogin.ToggleButton(false);
        MCS.VIALogin.ToggleIcon("SAML", " ", "");
        returnDataCFTC.data.isBothSiteFacCerner = true;
        MCS.VIALogin.isBothSiteFacCerner = true;
      }

      deferredCFTC.resolve(returnDataCFTC);
    },
    function (error) {
      Xrm.Utility.alertDialog(error.message);
    }
  );
  return deferredCFTC.promise();
}



MCS.VIALogin.GetRelatedSchedulingPackage = function (serviceappintment) {
  var schedulingPackageSP = $.Deferred();
  var returnDataSP = {
    success: true,
    data: {
    }
  };

  Xrm.WebApi.online.retrieveMultipleRecords("serviceappointment", "?$select=subject,activityid,_cvt_relatedschedulingpackage_value&$top=1&$filter=activityid eq " + serviceappintment).then(
    function success(res) {
      console.log("*****RETRIEVED VALUES FROM SA*****");
      //_cvt_relatedschedulingpackage_value
      schedulingPackage = res.entities[0]["_cvt_relatedschedulingpackage_value@OData.Community.Display.V1.FormattedValue"];
      spId = res.entities[0]["_cvt_relatedschedulingpackage_value"];
      returnDataSP.data.schedulingPackage = schedulingPackage;
      returnDataSP.data.spId = spId;
      schedulingPackageSP.resolve(returnDataSP);
    },
    function (error) {
      console.log("*****ERROR IN VIALOGIN AT 2300***" + error.message);
      returnData.success = false;
      schedulingPackageSP.resolve(returnDataSP)
    }
  );

  return schedulingPackageSP.promise();
}

MCS.VIALogin.GetRelatedSA = function (ReserveResourceId) {
  //debugger;
  var reserveResourceRR = $.Deferred();
  var returnDataRR = {
    success: true,
    data: {
    }
  };

  Xrm.WebApi.online.retrieveMultipleRecords("appointment", "?$select=subject,activityid,_cvt_serviceactivityid_value&$top=1&$filter=activityid eq " + ReserveResourceId).then(
    function success(res1) {
      console.log("*****GOT DATA at 2287*****");
      returnDataRR.data.SAID = res1.entities[0]["_cvt_serviceactivityid_value"];
      reserveResourceRR.resolve(returnDataRR);
    },
    function (error) {
      console.log("*****ERROR IN VIALOGIN AT 2287*****" + error.message);
      reserveResourceRR.resolve(returnDataRR);
    }
  );
  return reserveResourceRR.promise();
}

//SAML, PatDuz, ProDuz
MCS.VIALogin.ToggleIcon = function (area, toggle, message) {
  console.log("Begin MCS.VIALogin.ToggleIcon");
  var icons = [
    $("#msg" + area + "_working"),
    $("#msg" + area + "_success"),
    $("#msg" + area + "_failure")
  ];

  for (i = 0; i < icons.length; i++) {
    if (icons[i].selector.search(toggle) > -1) icons[i].show();
    else icons[i].hide();
  }

  var notification = $("#" + area + "Notification");
  notification.text("");
  notification.append(message);
};

MCS.VIALogin.ToggleButton = function (isVisible) {
  console.log("Begin MCS.VIALogin..ToggleButton");
  $("#LoginButton").attr('disabled', !isVisible);
};

MCS.VIALogin.SetLookup = function (column, targetField) {
  console.log("Begin MCS.VIALogin.SetLookup");
  if (targetField != null) {
    var obj = { id: column.Id, entityType: column.LogicalName, name: column.Name }
    if (obj.name == null) targetField.setValue(null);
    else targetField.setValue([obj]);
  }
};