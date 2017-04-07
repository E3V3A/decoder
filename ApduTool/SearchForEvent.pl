# NOTE: This script must be run with Perl in a command box, 
# i.e.  Perl QXDMRequestItem.pl <COM Port Number>

# This script demostrates usage of the QXDM2 automation
# interface method RequestItem

use HelperFunctions4;

# Global variables
my $QXDM;

# COM port to be used for communication with the phone by QXDM
my $PortNumber = "";

# Process the argument - port number
sub ParseArguments
{
   # Assume failure
   my $RC = false;
   my $Help = "Syntax: Perl RequestItem.pl <COM Port Number>\n"
            . "Eg:     Perl RequestItem.pl 5\n";

   if ($#ARGV < 0)
   {
      print "\n$Help\n";
      return $RC;
   }

   $PortNumber = $ARGV[0];
   if ($PortNumber < 1 || $PortNumber > 100)
   {
      print "\nInvalid port number\n";
      print "\n$Help\n";
      return $RC;
   }

   # Success
   $RC = true;
   return $RC;
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

   # Success
   $RC = true;
   return $RC;
}

# Schedule requests to be sent
sub SearchForEvents
{
   my $ReqID = $QXDM->SearchForEvent(2525 , 0, "");
   if ($ReqID == 0)
   {
      print "SearchForEvent timed out\n";
      return;
   }
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
   my $RC = Initialize();
   if ($RC == false)
   {
      return;
   }

   # Get QXDM version
   my $Version = $QXDM->{AppVersion};
   print "\nQXDM Version: " . $Version . "\n";

   # Connect to our desired port
   $RC = Connect( $PortNumber );
   if ($RC == false)
   {
      return;
   }

   # Schedule requests using "RequestItem"
   SearchForEvents();

   # Disconnect phone
   Disconnect();
}

Execute();