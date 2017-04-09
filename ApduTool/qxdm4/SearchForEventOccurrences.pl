# NOTE: This script must be run with perl in a command box, 
# i.e.  perl SearchForEventOccurrences.pl <COM Port Number>

# This script demostrates usage of the QXDM2 automation
# interface method SearchForEventOccurrences

use HelperFunctions4;
use Win32::OLE::Variant qw(:DEFAULT nothing);

# Global variables
my $QXDM;

# COM port to be used for communication with the phone by QXDM
my $PortNumber = "";

# Process the argument - port number
sub ParseArguments
{
   # Assume failure
   my $RC = false;
   my $Help = "Syntax: perl SearchForEventOccurrences.pl <COM Port Number>\n"
            . "Eg:     perl SearchForEventOccurrences.pl 5\n";

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
   $QXDM = QXDMInitializeInterface2();
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
sub SearchForEventOccurrences
{
    my $eventList = $QXDM->SearchForEventOccurrences(1665, "");
    if (!defined($eventList))
    {
	print "\nFailed to retrieve event list\n";
	return;
    }

    my $numEvents = $eventList->GetEventCount();
    print "\nFound $numEvents matching events\n";
    for (my $index = 0; $index < $numEvents; $index++)
    {
	my $eventText = $eventList->GetEventText($index);
	print "$eventText\n";
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

   sleep(10);
   
   # Schedule requests using "SearchForEventOccurrences"
   SearchForEventOccurrences();

   # Disconnect phone
   Disconnect();
}

Execute();
