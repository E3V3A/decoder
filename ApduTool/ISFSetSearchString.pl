# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl ISFSetSearchString.pl

# This script demostrates usage of the IIClientConfig automation
# interface method 
#                 SetSearchString()
#                 AddSearchContent()

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

# Set match string and 
sub MatchItems
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
   # Add desired item types: Diag response/request, event, log, message, string
   $IConfig->AddItem(1);
   $IConfig->AddItem(2);
   $IConfig->AddItem(4);
   $IConfig->AddItem(5);
   $IConfig->AddItem(6);
   $IConfig->AddItem(7);

   # Add Search Content: type, key, name, timestamp, summary
   $IConfig->AddSearchContent(0);
   $IConfig->AddSearchContent(1);
   $IConfig->AddSearchContent(2);
   $IConfig->AddSearchContent(3);
   $IConfig->AddSearchContent(4);

   # Add Search string
   $IConfig->SetSearchString("0x413F"); 
   #$IConfig->SetCaseSensitiveSearchFlag(false);  
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

   # Get the number of parsed fields for an item
   MatchItems();
}

Execute();