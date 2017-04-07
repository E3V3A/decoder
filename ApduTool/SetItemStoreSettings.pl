# NOTE: This script must be run from Perl in a command box,
# i.e.  Perl ISFSetFileSize.pl

# This example demonstrates Setting the ISF File Size
# Also api to get ISF File.
# Currently this API sets the ISF size to 10 MB

use HelperFunctions4;

# Global variable
my $QXDM;

# Initialize application
sub Initialize
{
   # Assume failure
   my $RC = false;

   # Create QXDM object
   $QXDM = QXDMInitialize();
   if ($QXDM == null)
   {
      print "\nError launching QXDM";

      return $RC;
   }

   SetQXDM( $QXDM );

   # Success
   $RC = true;
   return $RC;
}

# Main body of script
sub Execute
{
   # Launch QXDM
	my $RC = Initialize();
	if ($RC == false)
	{
		return;
	}

	# Get QXDM version
	my $Version = $QXDM->{AppVersion};
	print "\nQXDM Version: " . $Version;

	#Enable ISF Advanced Mode/Standard mode: set true for Advance mode enable
	$QXDM->EnableISFFAdvancedMode(true);

	#Get ISF Advanced Mode//Standard mode
	$RC = $QXDM->IsISFAdvancedModeEnabled();
	if ($RC == true)
	{
		print "\nItem Store Advance mode enabled\n";
	}
	else
	{
		print "\nItem Store Advance mode disabled \n"
	}

	#Enable ISF Auto Save when Max limits hit.true = to enable auto ISF save
	$QXDM->SetAutoSaveISF(true);

	# Get Auto Save ISF
	$RC = $QXDM->GetAutoSaveISF();
	if ($RC == true)
	{
		print "\nAuto ISF save is enabled\n";
	}
	else
	{
		print "\nAuto ISF save is disabled\n";
	}	

	#set base file name
	$QXDM->SetBaseISFFileName("APS"); 
   
	#Get base file name
	my $NewBaseISFFileName =  $QXDM->GetBaseISFFileName();
	print "\nVerifying: Current  Base ISF File Name is:". "$NewBaseISFFileName";   
	
		
	#set ISF Dir
	$QXDM->SetISFDirPath("C:/temp"); 
	   
	#Get base file name
	my $NewISFDIRPath =  $QXDM->GetISFDirPath();
	print "\nVerifying: Current ISF Dir path is:". "$NewISFDIRPath";  

	
	#set value in mega bytes
	print "\nSet ISF File size to 10 Megabytes:\n";
	$QXDM->SetISFFileSize(2); 
   
	#Get ISF File Size (megabytes)
	my $NewISFFileSize =  $QXDM->GetISFFileSize();

	print "\nVerifying: Current ISF File Size in Megabytes:". "$NewISFFileSize";   
		

	#Set ISF File max duration (in minutes: 30 minutes below)
	$QXDM->SetISFMaxDuration(2);

	my $NewISFFileDuration = $QXDM->GetISFMaxDuration();
	print "\nVerifying: Current ISF Duration:". "$NewISFFileDuration";   

	
	#Set ISF File max archives (10 archives max)
	$QXDM->SetMaxISFArchives(10);

	#Get ISF File max archives 
	my $NewISFMaxArchives = $QXDM->GetMaxISFArchives();
	print "\nVerifying: Current ISF Archive size:". "$NewISFMaxArchives";   
		
	
	#set ISF post processing command and argruements. Currently set to empty string
	$QXDM->SetISFPostProcessingCmd(""); 
   
	#Get ISF post processing command and argruements
	my $NewISFProcessingCmd =  $QXDM->GetISFPostProcessingCmd();
	print "\nVerifying: Current ISF Post Processing command:". "$NewISFProcessingCmd";  		

	
	#Common API to set Item Store Settings.
	#api SetItemStoreAdvanceOptions(bool enable AdvanceMode,uint MaxISFSizeInMB,uint maxISFDuration,bool autosave,uint maxArchives);

	$QXDM->SetItemStoreAdvanceOptions(true, 2 , 2 , true, 10);
}

Execute();