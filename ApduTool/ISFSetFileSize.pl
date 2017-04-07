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

	print "\nSet ISF File size to 10 Megabytes:\n";

	#set value in mega bytes
	$QXDM->SetISFFileSize(10); 
   
	#Get ISF File Size (megabytes)
	my $NewISFFileSize =  $QXDM->GetISFFileSize();

	print "\nVerifying: Current ISF File Size in Megabytes:". "$NewISFFileSize";   
	
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
}

Execute();