# NOTE: This script must be run from Perl in a command box,
# i.e.  Perl LoadConfiguration.pl

# This example demonstrates loading the QXDM configuration
# from an example .DMC file

use HelperFunctions4;

# Global variable
my $QXDM;

# Get file name from script folder path
sub GetDMCFileName
{
   my $FileName = "";
   my $FolderPath = GetPathFromScript();

   # Configuration file name
   $FileName = $FolderPath."convert_demo_fromCFG.dmc";
   return $FileName;
}

# Get file name from script folder path
sub GetCFGFileName
{
   my $FileName = "";
   my $FolderPath = GetPathFromScript();

   # Configuration file name
   $FileName = $FolderPath."convert_demo.cfg";
   return $FileName;
}

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

   # Generate output configuration file name
   my $DMCFileName = GetDMCFileName();
   if ($DMCFileName eq "")
   {
      return;
   }
   # Generate output configuration file name
   my $CFGFileName = GetCFGFileName();
   if ($CFGFileName eq "")
   {
      return;
   }
#	my $CFGFileName = "\0";


   print "\n\n";

	# Load existing configuration
	my $ReqID = $QXDM->ConvertCFGtoDMC( $CFGFileName,$DMCFileName,1 );
	if ($ReqID == 0)
	{
      print "Unable to convert DMC file '$DMCFileName' to '$CFGFileName'\n";
      return;
	} 
	else
	{
		  print "\n Convert CFG configuration file:\n"
       . "'$CFGFileName' to '$DMCFileName' - Succeed! \n";
	} 
}

Execute();