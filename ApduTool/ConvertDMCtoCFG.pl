# NOTE: This script must be run from Perl in a command box,
# i.e.  Perl LoadConfiguration.pl

# This example demonstrates loading the QXDM configuration
# from an example .DMC file

use HelperFunctions4;
use Win32::OLE::Variant;

# Global variable
my $QXDM;

# Get file name from script folder path
sub GetDMCFileName
{
   my $FileName = "";
   my $FolderPath = GetPathFromScript();

   # Configuration file name
   $FileName = $FolderPath."convert_demo.dmc";
   return $FileName;
}

# Get file name from script folder path
sub GetCFGFileName
{
   my $FileName = "";
   my $FolderPath = GetPathFromScript();

   # Configuration file name
   $FileName = $FolderPath."convert_demo_fromDMC.cfg";
   return $FileName;
}

# Initialize application
sub Initialize
{
   # Assume failure
   my $RC = false;

   # Create QXDM object
   $QXDM =  QXDMInitialize();
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
   # print "\n\n";
  # print $DMCFileName ;
   print "\n\n";
   
  my $bLogging = true;
   
   # Load existing configuration
   #my $ReqID = $QXDM->ConvertDMCtoCFG( $DMCFileName,"",1,$bLogging);
   my $ReqID = $QXDM->ConvertDMCtoCFG( $DMCFileName,$CFGFileName,1,$bLogging); 
   
   # Load existing configuration
#   my $ReqID = $QXDM->ConvertDMCtoCgFG( $DMCFileName,"",1 );
   if ($ReqID == 0)
   {
      print "Unable to convert DMC file '$DMCFileName' to '$CFGFileName'\n";
      return;
   } 
   else
   {
		print "\n Convert DMC configuration file:\n"
       . "'$DMCFileName' to '$CFGFileName' - Succeed! \n";
   }	   
}

Execute();