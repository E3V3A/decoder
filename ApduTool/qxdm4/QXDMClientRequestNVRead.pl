# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl QXDMClientRequestNVRead.pl <COM Port Number>

# This script demostrates usage of the QXDM2 automation
# interface method ClientRequestNVRead()

use HelperFunctions4;
use Win32::OLE::Variant;

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
   my $Help = "Syntax: Perl QXDMClientRequestNVRead.pl <COM Port Number>\n"
            . "Eg:     Perl QXDMClientRequestNVRead.pl 5\n";

   if ($#ARGV < 0)
   {
      print "\n$Help\n";
      return $RC;
   }

   $PortNumber = $ARGV[0];
   if ($PortNumber < 1)
   {
      print "\nInvalid port number\n"
          . "\n$Help\n";
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

# Schedule request to be sent
sub ScheduleRequest
{
   # Create a client
   my $Handle = $QXDM2->RegisterQueueClient( 256 );
   if ($Handle == 0xFFFFFFFF)
   {
      print "\nUnable to create client\n";

      return;
   }

   #my $nvID = 0;
   my $nvID = 70209;
	my $errorMsg = Variant(VT_BSTR | VT_BYREF,"");
	my $RequestName = Variant(VT_BSTR | VT_BYREF,"");
	my $ReqID1 = $QXDM2->ClientRequestNVName( $Handle,
                                              $nvID,
                                             $RequestName);
   
   if ($ReqID1 == 0)
   {
      print "Unable to schedule NV name request - '$RequestName'\n";
	  
	  $QXDM2->UnregisterClient( $Handle );
	 
      return;
   }
   print "The NV Item Name  -  $RequestName \n";
	# Schedule esn request with 1000 ms timeout
	#my $RequestName = "esn";
	my $ReqID2 = $QXDM2->ClientRequestNVRead( $Handle,
                                            $RequestName,
                                            "",
                                            true,
                                            2000,
											0,
											0,
											$errorMsg	);
   if ($ReqID2 == 0)
   {
      print "Unable to schedule NV read request - '$RequestName'\n";
	  $QXDM2->UnregisterClient( $Handle );
	  print "Error Message - '$errorMsg'\n";
      return;
   }

   my $annotation = "NV read request '$RequestName' scheduled by QXDM\n";
   print $annotation;
   $QXDM2->SendScript("echo \"$annotation\"");

   # Wait for response 
   sleep(2);
   print "\n";

   # Check item count. If 2, response received
   my $ItemCount = $QXDM2->GetClientItemCount( $Handle);
   print "Number of items in client: $ItemCount\n";


   # Get response
   my $Item = $QXDM2->GetClientItem( $Handle,1 );
   if ($Item == null)
   {
     print "Unable to retrieve client item $Item\n";
     $QXDM2->UnregisterClient( $Handle );
     return ;
   }


   # Print Response information  
   print "Print response information\n";
   my $ItemTS =  $Item->GetItemTimestampText( false, true );
   my $ItemName = $Item->GetItemName();
   my $ItemSummary = $Item->GetItemSummary();
   print "\tItemName: \t" . $ItemName ."\n\tTime: \t\t" . $ItemTS . "\n\tSummary: \t" . $ItemSummary . "\n";


  
   #Print Item fields
   my $ItemFields = $Item->GetItemFields($nvID);

   my $fieldcount = $ItemFields->GetFieldCount();
   print "\t---------------------------------------------\n";
   print "\tFields:\n";

   for(my $index = 0; $index < $fieldcount ; $index ++)
   {
      my $fieldName = $ItemFields->GetFieldName($index, 1);
      my $fieldValue = $ItemFields->GetFieldValue($index);
      my $fieldValueText = $ItemFields->GetFieldValueText($index);
      print "\t\t\t$fieldName = $fieldValueText \n";
   }

   print "\n";

   # Unregister the client
   $QXDM2->UnregisterClient( $Handle );
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

   # Get QXDM version
   my $Version = $QXDM->{AppVersion};
   print "\nQXDM Version: " . $Version . "\n";

   # Connect to our desired port
   $RC = Connect( $PortNumber );
   if ($RC == false)
   {
      return;
   }

   # Schedule request using "ClientRequestNVRead"
   ScheduleRequest();

   # Disconnect phone
   Disconnect();
}

Execute();
