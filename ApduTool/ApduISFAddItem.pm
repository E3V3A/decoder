# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl ISFAddItem.pl

# This script demostrates usage of the IIClientConfig automation
# interface method 
#                 AddItem()
# It is equivalent to Refilter Items in Select Refiltering Config dialog in QXDM

use HelperFunctions4;

# Global variable
my $IISF;

my $FileName;
my $FileNameOutput;

# Initialize application
sub Initialize
{
   # Assume failure
   my $RC = false;

   # Create the item store file interface
   $IISF =  QXDMInitialize();
   if (!$IISF)
   {
      print "\nUnable to obtain ISF interface\n";

      return $RC;
   }

   # Success
   $RC = true;
   return $RC;
}

# Add Items
sub AddItem
{
   # Load the item store file
   my $Handle = $IISF->LoadItemStore( $FileName );
   if ($Handle == 0xFFFFFFFF)
   {
      print "\nUnable to load ISF:\n$FileName\n";
      return;
   }   
 
   my $IClient = $IISF->GetClientInterface( $Handle );
   if (!$IClient)
   {
      $Txt = "Unable to obtain ISF client interface";
      print "\n$Txt";

      $IISF->CloseItemStore( $Handle );
      return $RC;
   }

   my $ClientHandle = $IClient->RegisterClient( true );
   if ($ClientHandle == 0xFFFFFFFF)
   {
      $Txt = "Unable to register ISF client";
      print "\n$Txt";

      $IISF->CloseItemStore( $Handle );
      return $RC;
   }

   my $IConfig = $IClient->ConfigureClient( $ClientHandle );
   if (!$IConfig)
   {
      $Txt = "Unable to configure ISF client";
      print "\n$Txt";

      $IClient->UnregisterClient( $ClientHandle );
      $IISF->CloseItemStore( $Handle );
      return $RC;
   }

   $Txt = "Processing ISF file...";
   print "\n$Txt";

   # Configure the client
   #log packet
   $IConfig->AddItem(ITEM_TYPE_LOG);  	
   my @loglist = (0x1098, 0x14ce, #APDU
                0x1544, 0x138e, 0x138f, 0x1390, 0x1391, 0x1392, 0x1393, 0x1394, 0x1395, 0x1396, 0x1397, 0x1398, 0x1399, 0x139a, 0x139b, 0x139c, 0x139d, 0x139e, 0x139f, 0x13a0, 0x13a1, 0x13a2, 0x13a3, 0x13a4, 0x13a5, 0x13a6, 0x13a7, 0x13a8, 0x13a9, 0x13aa, 0x13ab, 0x13ac, 0x13ad, #QMI
                0xb0c0, 0xb0e2, 0xb0e3, 0xb0ec, 0xb0ed, #OTA LTE
                0x713a, 0x7b3a, 0xd0e3, 0x412f, 0x5b2f,  #OTA  UMTS, TDS, W, GSM
                0x1004, 0x1005, 0x1006, 0x1007, 0x1008, #OTA 1X
                0x156e, 0x1830, 0x1831, 0x1832, #IMS
            );
   foreach $id (@loglist)
   {
	$IConfig->AddLog($id);   	
   }

   #message packet
   $IConfig->AddItem(ITEM_TYPE_MESSAGE);  	
   my @msglist = (21, 6039);#UIM, PBM
   foreach $id (@msglist)
   {
       foreach $level (0..4)
	{
	    $IConfig->AddMessage($id, $level);
	}
   }

   #diag response
   $IConfig->AddItem(ITEM_TYPE_DIAG_RX);
   my @diaglist = (0, 124);
   foreach $id (@diaglist)
   {
     	    $IConfig->AddDIAGResponse($id);
   }
   
   #string
   $IConfig->AddItem(ITEM_TYPE_STRING);

   #sub sys dispatch response
   $IConfig->AddItem(ITEM_TYPE_SUBSYS_RX);
   my @syslist = ([8, 1], [4, 15]);
   foreach $id (@syslist)
   {
        $IConfig->AddSubsysResponse(@$id[0], @$id[1]);
   }

   $IConfig->CommitConfig();

   # Populate the client with all instances of supported logs
   $IClient->PopulateClients();

   $IClient->CopyAllClientsItems($FileNameOutput);

}

# Main body of script
sub ApduISFAddItem
{
   # Launch QXDM
   ($FileName, $FileNameOutput)=@_;

   my $RC = Initialize();
   if ($RC == false)
   {
      return;
   }

   AddItem();
}

1;
