# NOTE: This script must be run from Perl in a command box,
# i.e.  Perl GetVersionPacket.pl <Port Number>

# This script demostrates usage of QXDM2 automation
# interface to get "Version Number Response"

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
   my $Help = "Syntax: Perl GetVersionPacket.pl <COM Port Number>\n"
            . "Eg:     Perl GetVersionPacket.pl 5\n";

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
   $QXDM =  QXDMInitialize();
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
   print "\nQXDM Version: ".$Version."\n";

   # Connect to our desired port
   $RC = Connect( $PortNumber );
   if ($RC == false)
   {
      return;
   }

   # Schedule version number request
   SendRequestAndReturnResponse( "Version Number Request", "", 24 );

   # Disconnect phone
   Disconnect();
}

Execute();