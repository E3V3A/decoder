# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl ISFGetItemName.pl

# This script demostrates usage of the IColorItem automation
# interface method GetItemName()

use HelperFunctions4;

# Global variable
my $IISF;

# Initialize application
sub Initialize
{
   # Assume failure
   my $RC = false;

   # Create the item store file interface
   $IISF = QXDMInitialize();
   if (!$IISF)
   {
      print "\nUnable to obtain ISF interface\n";

      return $RC;
   }

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
   
	my $Version = $IISF->AppVersion();
   print "\nVersion is " . "'$Version'\n";
}

Execute();