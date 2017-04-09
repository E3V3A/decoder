# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl QXDMGetClientItem.pl

# This example demonstrates the usage of the QXDM2 automation
# interface method GetClientItem()

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
      print "\nError launching QXDM\n";

      return $RC;
   }

   # Create QXDM2 interface
   $QXDM2 = $QXDM->GetIQXDM2();
   if ($QXDM2 == null)
   {
      print "\nQXDM does not support required interface\n";

      return $RC;
   }

   SetQXDM ( $QXDM );
   SetQXDM2 ( $QXDM2 );

   # Success
   $RC = true;
   return $RC;
}

# Demonstrate getting a client item
sub GetClientItem
{
   # Register a QXDM client
   $Handle = $QXDM2->RegisterQueueClient( 256 );
   if ($Handle == 0xFFFFFFFF)
   {
      print "\nUnable to create client\n";

      return;
   }

   my $Client = $QXDM2->ConfigureClientByKeys( $Handle );
   if ($Client == null)
   {
      print "\nUnable to configure client by keys\n";

      $QXDM2->UnregisterClient( $Handle );
      return;
   }

   # Register for strings
   $Client->AddItem( ITEM_TYPE_STRING );
   $Client->CommitConfig();

   print "\nAdding five strings to item store";

   # Add strings to QXDM item store
   my $Str = "";
   for (my $i = 0; $i < 5; $i++)
   {
      $Str = "Test String " . $i;
      $QXDM->QXDMTextOut( $Str );
   }

   my $Item = $QXDM2->GetClientItem( $Handle, 3 );
   if ($Item == null)
   {
      print "\nUnable to retrieve client item #3\n";

      $QXDM2->UnregisterClient( $Handle );
      return;
   }

   print "\nClient item #3:";

   my $ItemTS =  $Item->GetItemTimestampText( false, true );
   my $ItemName = $Item->GetItemName();
   my $ItemSummary = $Item->GetItemSummary();

   print "\n   " . $ItemTS . ", " . $ItemName . ", " . $ItemSummary . "\n";

   # Unregister the client
   $QXDM2->UnregisterClient( $Handle );
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

   # Demonstrate getting a client item
   GetClientItem()
}

Execute();