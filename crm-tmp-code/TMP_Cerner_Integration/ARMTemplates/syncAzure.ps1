param ($inputPath=".",$rg = "EIS-DEVTEST-INT-SOUTHWEST-EIS-RG",$env = "dev")
# Script pulls logic app changes from azure and updates local json files
#USAGE: 
        # .\syncAzure.ps1 #where your cwd is in the folder that you want to update
        # .\syncAzure.ps1 .\Inbound\DVProcessing\va-eis-tmp-inbound-processing-S12.json
        # .\syncAzure.ps1 -inputPath .\Inbound\DVProcessing\va-eis-tmp-inbound-processing-S12.json
        # .\syncAzure.ps1 -inputPath .\Inbound\DVProcessing\va-eis-tmp-inbound-processing-S12.json -env nprod -rg nprod-resource-group



# Original code obtained from https://github.com/PowerShell/PowerShell/issues/2736
# Formats JSON in a nicer format than the built-in ConvertTo-Json does.
# it annoys me I had to use this
function Format-Json {
    param
    (
        [Parameter(Mandatory, ValueFromPipeline)]
        [String]
        $json
    ) 

    $indent = 0;
    $result = ($json -Split '\n' |
            % {
            if ($_ -match '[\}\]]') {
                # This line contains ] or }, decrement the indentation level
                $indent--
            }
            $line = (' ' * $indent * 2) + $_.TrimStart().Replace(': ', ': ')
            if ($_ -match '[\{\[]') {
                # This line contains [ or {, increment the indentation level
                $indent++
            }
            $line
        }) -Join "`n"
    
    # Unescape Html characters (<>&')
    $result.Replace('\u0027', "'").Replace('\u003c', "<").Replace('\u003e', ">").Replace('\u0026', "&")
}

#function pulls the logic app from azure, and attempts to patch the arm template file
function Update-Logicapps {
	param ($rg,$env,$name,$pathToARMJson, $templatejson)
    Write-Host Attemping to update arm template for:
    $name
    $deployname = $name+"-"+$env
    # pull full arm template out of azure and convert to json obj
    $azjsondef = az logic workflow show --resource-group $rg --name $deployname --only-show-errors | ConvertFrom-Json
    if (($azjsondef.definition | ConvertTo-Json -depth 100 -Compress) -eq 
                ($templatejson.resources[0].properties.definition | ConvertTo-Json -depth 100 -Compress)){
        Write-Host $name "is in sync with azure" -ForegroundColor Green
    }
    else{
        Write-Host "PATCHING:" $pathToARMJson -ForegroundColor Cyan
        $templatejson.resources[0].properties.definition = $azjsondef.definition

        $templatejson | convertto-json -depth 100 |Format-Json |set-content $pathToARMJson 
    }
}

#Given a json arm template returns the json object if it's a logic app
#returns nothing if the given app is another type
function Is-LogicApp{
        param ($templatejson)

        #check if the file is a logic app workflow, if so return the json obj
        if ( $templatejson.resources.type -eq "Microsoft.Logic/workflows" )
        {
            return $templatejson
        }
}


# get the full path of the input path
$inputPath = Resolve-Path -Path $inputPath

az cloud set --name AzureUSGovernment
az login

if(!(Test-Path -Path $inputPath)){
    Write-Host "Invalid Input Path" -ForegroundColor Red
    return
}

#pull a list of all the logic apps currently hosted in azure
$deployedlogicappjson = az resource list --resource-type "Microsoft.Logic/workflows" --resource-group $rg | ConvertFrom-Json

# if given path is a folder
if (Test-Path -Path $inputPath -PathType container){
    #cd to $repoPath saving where you are, $repoDirPath is passed in
    Push-Location $inputPath

    # Search the directory you passed in 
    # Find names of json files without param in the name: 
    # Basename is the file name without json, fullname is full dir path
    $table = Get-ChildItem -Recurse -filter *.json | where Name -match '^((?!param).)*$' 

    #for each match, check if the arm template is defining a logic app, then overwrite the definition section
    foreach ($row in $table)
    {
      #convert the arm file to json
      $templatejson = Get-Content $row.FullName | ConvertFrom-Json 
      if($deployedlogicappjson.name -contains $row.Basename+"-"+$env){
        Update-Logicapps -rg $rg -env $env -name $row.Basename -pathToARMJson $row.FullName -templatejson $templatejson
      }
      #check to see if the logic app is named inproperly
      elseif(Is-LogicApp -templatejson $templatejson){
        Write-Host "Logic app is not hosted in Azure, or is misnamed:" $row.FullName -ForegroundColor Red
        
      }

    }

    #return to where you originally were when you ran the script
    Pop-Location
}
# assume it's a json file
else{
    
    $row = Get-ChildItem $inputPath

    #convert the arm file to json
    $templatejson = Get-Content $row.FullName | ConvertFrom-Json 

    if($deployedlogicappjson.name -contains $row.Basename+"-"+$env){
        Update-Logicapps -rg $rg -env $env -name $row.Basename -pathToARMJson $row.FullName -templatejson $templatejson
    }
    #check to see if the logic app is named inproperly
    elseif(Is-LogicApp -templatejson $templatejson){
        Write-Host "Logic app is not hosted in Azure, or is misnamed:" $row.FullName -ForegroundColor Red
         
    }

}

