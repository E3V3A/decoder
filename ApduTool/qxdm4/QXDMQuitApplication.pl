# NOTE: This script must be run from a command box,
# i.e.  Perl QXDMQuitApplication.pl

# This script demostrates usage of the QXDM automation
# interface method QuitApplication

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

   print "\nQuitting QXDM ... ";

   $QXDM->QuitApplication();

   print "closed\n";
}

Execute();