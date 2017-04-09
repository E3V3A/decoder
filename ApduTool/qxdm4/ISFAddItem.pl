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

# Get input file name from script folder path
sub GetFileName
{
   my $FileName = "";
   my $FolderPath = GetPathFromScript();

   # Item store file name
   $FileName = $FolderPath . "Example.isf";
   return $FileName;
}

# Get output file name from script folder path
sub GetOutputFileName
{
   my $FileName = "";
   my $FolderPath = GetPathFromScript();

   # Item store file name
   $FileName = $FolderPath . "output.isf";
   return $FileName;
}

# Add Items
sub AddItem
{
   # Generate item store file name
   my $FileName = GetFileName();
   if ($FileName eq "")
   {
      return;
   }

   # Generate output item store file name
   my $FileNameOutput = GetOutputFileName();
   if ($FileNameOutput eq "")
   {
      return;
   }

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
   # Add desired item types: log, 0x413F
   $IConfig->AddItem($LOG);
   $IConfig->AddLog(0x413F);

   # Add desired item type: Diag request, log
   $IConfig->AddItem($DIAG_REQ);
   $IConfig->AddDIAGRequest(000);

   $IConfig->CommitConfig();

   # Populate the client with all instances of supported logs
   $IClient->PopulateClients();

   $IClient->CopyAllClientsItems($FileNameOutput);

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

   AddItem();
}

Execute();