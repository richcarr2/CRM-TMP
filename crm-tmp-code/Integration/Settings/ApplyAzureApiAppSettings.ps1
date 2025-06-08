$fileName = "ihs-lob-nprod-tmp.xml";

Write-Host "Loading configuration file for Azure:" $fileName; 
[xml]$config =  (Get-Content $fileName)

if ($config -eq $null) {
    Write-Host "Error loading configuration file, make sure the file exists." -ForegroundColor Red
    Exit
}
$subscription = $config.Azure.SubscriptionName

Write-Host "Login to the Azure Environment:" -ForegroundColor Green

#login to Azure
#Login-AzureRmAccount -EnvironmentName AzureUSGovernment

Write-Host "Setting the correct Subscription:" -ForegroundColor Green

#get the Azure Subscription
Get-AzureRmSubscription –SubscriptionName $subscription | Select-AzureRmSubscription

foreach($webApp in $config.Azure.AppService){
     
    $appService = '';
    $group = $webApp.ResourceGroup;
    $name = $webApp.Name;
    $slot = $webApp.Slot;

    if(!$slot){
        Write-Host "Applying settings for app service : " $name " in group: " $group -ForegroundColor Green
       $appService = Get-AzureRmWebApp -Name $name -ResourceGroupName $group;
    }else{
      Write-Host "Applying settings for app service : " $name " in group: " $group "in slot: " $slot -ForegroundColor Green
      $appService = Get-AzureRmWebAppSlot -Name $name -Slot $slot -ResourceGroupName $group;
    }

    $appSettings = $appService.SiteConfig.AppSettings

    #setup the current app settings
    $settings = @{}
    ForEach ($setting in $appSettings) {
        $settings[$setting.Name] = $setting.Value
    }

    #adding new settings to the app settigns
    foreach($appSetting in $webApp.AppSettings.AppSetting){
        $settings[$appSetting.Key] = $appSetting.Value;
    }

    if(!$slot){
        $app = Set-AzureRMWebApp -Name $name -ResourceGroupName $group -AppSettings $settings
    }else{
        $app = Set-AzureRMWebAppSlot -Name $name -ResourceGroupName $group -AppSettings $settings -Slot $slot
    }

    Write-Host "Application settings applied to: " $appService.Name -ForegroundColor Green
}
