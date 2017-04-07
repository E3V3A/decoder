# NOTE: This script must be run with Perl in a command box,
# i.e.  perl QXDMWaitEvent.pl

# This script demostrates usage of the QXDM automation
# interface method WaitEvent

# SETUP INSTRUCTIONS
#   * Make sure device has wifi ENABLED before running this
#     (No need to configure a network)

use HelperFunctions4;
use Win32::OLE::Variant qw(:DEFAULT nothing);

# Global variables
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

    # Get QXDM version
    my $Version = $QXDM->{AppVersion};
    print "\nQXDM Version: $Version";
    
    $QXDM->SetComPort(4);
    my $serverState = $QXDM->GetServerState();
    
    print "\nServer State $serverState";
    if ($serverState == 2)
    {
	my $payload = $QXDM->WaitEvent(1665, 30000); # wait 30 seconds

	if ((defined $payload) && ($payload ne ""))
	{
	    my $variant = Variant(VT_ARRAY | VT_UI1, length($payload));
	    $variant->Put($payload);
	    $mySummary = $QXDM->GetSummary(1665, $variant);
	    
	    print "\nSummary $mySummary";
	}
	else
	{
	    print "\nEvent not found";
	}
	$QXDM->Close();
    }
    
    $QXDM->Quit();
}

Execute();
