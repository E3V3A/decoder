# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl QXDMGetItem.pl

# This script demostrates usage of the QXDM2 automation
# interface method GetItem

use HelperFunctions4;

# Global variables
my $QXDM;
my $QXDM2;

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
   $QXDM2 = $QXDM;
   if ($QXDM2 == null)
   {
      print "\nQXDM does not support required interface";

      return $RC;
   }

   # Success
   $RC = true;
   return $RC;
}

# Obtain and dump details of last item in the QXDM item store
sub DumpLastItem
{
   my $FeildPadding = 5;

   # Get number of items in item store
   my $ItemCount = $QXDM2->GetItemCount();
   if ($ItemCount == 0)
   {
      print "\nNo items in the QXDM item store\n";
      return;
   }

	$ItemCount = $ItemCount - 1;
	print "\nGetting Item $ItemCount\n";
	# Get an item from the QXDM item store
	my $Item = $QXDM2->GetItem($ItemCount);
	if ($Item == null)
	{
		print "\nGetItem failed\n";
		
		return;
	}

   print "\nGetItem succeeded" . "\nDisplaying item # " . $ItemCount . ":\n";

   DumpItemDetails( $Item, $FieldPadding );
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

   # Dump out the last item
   DumpLastItem();
}

Execute();