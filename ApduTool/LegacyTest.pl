# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl ISFAddItem.pl

# This script demostrates usage of the IIClientConfig automation
# interface method 
#                 AddItem()
# It is equivalent to Refilter Items in Select Refiltering Config dialog in QXDM

use HelperFunctions4;

# Global variable
my $IISF;

# Constants defined in IISFConfigClient.AddItem
my $DIAG_REQ  = 2;
my $LOG = 5;

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
	
	my $version = $IISF->GetQXDMVersion();
	print "QXDM Version:$version\n";
	
	my $daysLeft = $IISF->GetLicenseDaysLeft();
	print "Days Left:$daysLeft\n";
}

Execute();