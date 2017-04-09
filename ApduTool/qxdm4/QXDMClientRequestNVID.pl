# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl QXDMClientRequestNVID.pl <COM Port Number>

# This script demostrates usage of the QXDM2 automation
# interface method ClientRequestItem

use HelperFunctions4;

# Global variables
my $QXDM;
my $QXDM2;

# COM port to be used for communication with the phone by QXDM
my $PortNumber = "";

# Process the argument - port number
sub ParseArguments
{
   # Assume failure
   my $RC = false;
   my $Help = "Syntax: Perl QXDMClientRequestNVID.pl <COM Port Number>\n"
            . "Eg:     Perl QXDMClientRequestNVID.pl 5\n";

   if ($#ARGV < 0)
   {
      print "\n$Help\n";
      return $RC;
   }

   $PortNumber = $ARGV[0];
   if ($PortNumber < 1 || $PortNumber > 100)
   {
      print "\nInvalid port number\n"
          . "\n$Help\n";
      return $RC;
   }

   # Success
   $RC = true;
   return $RC;
}

# Get file name from QXDM installation path
sub GetFileName
{
   my $FileName = "";
   my $QXDMFolderPath = GetPathFromScript();
   if (QXDMFolderPath eq "")
   {
      print "\nUnable to obtain QXDM path";
      return $FileName;
   }

   # The above returns the path to the QXDM executable so we need to 
   # go up one folder
   $QXDMFolderPath =~ s/\\bin/\\Automation/i;

   # Emulate handset keypress request file name
   $FileName = $QXDMFolderPath."InfoButtonPress.txt";
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

   # Create QXDM2 interface
   $QXDM2 = $QXDM->GetIQXDM2();
   if ($QXDM2 == null)
   {
      print "\nQXDM does not support required interface";

      return $RC;
   }

   SetQXDM ( $QXDM );
   SetQXDM2 ( $QXDM2 );

   # Success
   $RC = true;
   return $RC;
}

# Schedule requests to be sent
sub ScheduleRequests
{
   # Get a QXDM client
   my $ReqHandle = $QXDM2->RegisterQueueClient( 256 );
   if ($ReqHandle == 0xFFFFFFFF)
   {
      print "\nUnable to create client\n";

      return;
   }

   # Schedule version number request with 1000 ms timeout
   my $RequestName = "Electronic Serial Number";
   my $NVID = $QXDM2->ClientRequestNVID( $ReqHandle,
                                           $RequestName
                                         );
   if ($NVID == -1)
   {
      print "ClientRequestNVID failed\n";

      $QXDM2->UnregisterClient( $ReqHandle );

      return;
   }

   print "ID = '$NVID'\n";


   $QXDM2->UnregisterClient( $ReqHandle );
}

# Main body of script
sub Execute
{
   # Parse out arguments
   my $RC = ParseArguments();
   if ($RC == false)
   {
      return;
   }

   # Launch QXDM
   $RC = Initialize();
   if ($RC == false)
   {
      return;
   }

   # Get QXDM version
   my $Version = $QXDM->{AppVersion};
#   print "\n QXDM Version: " . $Version . "\n";

   # Connect to our desired port
   $RC = Connect( $PortNumber );
   if ($RC == false)
   {
      return;
   }

   # Schedule requests using "ClientRequestItem"
   ScheduleRequests();

   # Disconnect phone
   Disconnect();
}

Execute();