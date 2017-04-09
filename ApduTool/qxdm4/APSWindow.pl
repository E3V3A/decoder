# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl QXDMGetServerState.pl

# This script demostrates usage of the QXDM2 automation
# interface method GetServerState

use HelperFunctions4;

# Global variables
my $QXDM;

# Initialize application
sub Initialize
{
   # Assume failure
   my $RC = false;

   # Create QXDM object
   $QXDM = QXDMAPSWindow();
   if ($QXDM == null)
   {
      print "\nError launching QXDM";

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

	$nullArray = pack("s s s", 0, 0, 0);
	
	print "Null Array: " . $nullArray;
	
	$myName = $QXDM->GetName(1820);
	print "\nName " . $myName;
	
	$mySummary = $QXDM->GetSummary(1820, $nullArray);
		
	print "\nSummary " . $mySummary;

	$QXDM->Close();

	
	$QXDM->Quit();
}

Execute();