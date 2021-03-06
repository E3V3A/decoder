# NOTE: This script must be run from Perl in a command box,
# i.e. Perl PhoneOperations.pl <Port Number>

# This script demonstrates manipulating the phone state

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
   my $Help = "Syntax: Perl PhoneOperations.pl <COM Port Number>\n"
            . "Eg:     Perl PhoneOperations.pl 5\n";

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
   $RC = Initialize();
   if ($RC == false)
   {
      return;
   }

   # Setup QShrink4.0 database files server path
   $QXDM->LoadQShrink4DBFile("\\\\harv-vivekj\\QXDM\\msg_hash_21b53407-05ac-ec79-f111-2b578765ddc5.qsr4");
$QXDM->LoadQShrink4DBFile("\\\\harv-vivekj\\QXDM\\msg_hash_c59b7014-4689-03e6-dc08-2280db698066.qsr4");


   # Get QXDM version
   my $Version = $QXDM->{AppVersion};
   print "\nQXDM Version: ".$Version."\n";

   # Connect to our desired port
   $RC = Connect( $PortNumber );
   if ($RC == false)
   {
      return;
   }

   # Disconnect phone
  # Disconnect();
}

Execute();

