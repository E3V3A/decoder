# NOTE: This script must be run from a command box,
# i.e.  Perl DisconnectFromComport.pl [COM Port Number]

# This script demostrates usage of the QXDM automation
# interface method COMPort

use HelperFunctions4;

# Global variable
my $QXDM;

# COM port to be used for communication with the phone by QXDM

# Process the argument - port number
sub ParseArguments
{
   # Assume failure
   my $RC = false;
   my $Help = "Syntax: Perl GetName.pl \n"
            . "Eg:     Perl GetName.pl \n";

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
   }

   SetQXDM( $QXDM );

   # Success
   $RC = true;
   return $RC;
}

# Obtain and dump out the COM port status
sub GetName
{
# Obtain event name with eventId
	$eventName = $QXDM->GetName(329); 
   print "\n Event name:" . $eventName . "\n";
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
   print "\nQXDM Version: " . $Version . "\n";


   # Wait for change in COM port
   sleep( 2 );

   # Obtain event name with eventId
   GetName();
}

Execute();