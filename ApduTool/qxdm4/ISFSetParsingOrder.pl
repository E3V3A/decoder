# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl ISFSetParsingOrder.pl

# This script demostrates usage of the IItemFields automation
# interface method SetParsingOrder()

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

# Get file name from script folder path
sub GetFileName
{
   my $FileName = "";
   my $FolderPath = GetPathFromScript();

   # Item store file name
   $FileName = $FolderPath . "ParsedTextExample.isf";
   return $FileName;
}


# Dump the full parsed text of an item
sub DumpItemParsedText
{
   # Generate item store file name
   my $FileName = GetFileName();
   if ($FileName eq "")
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

   my $DLLFirst = 1;      # DLL first,then DB, UDB, then QCat
   my $QcatFirst = 4;     # Qcat first,Parsing DLL, then default DB
   my $Order = $QcatFirst;
   SetParsingOrder($Order);


   # Retrieve item from example ISF
   my $Item = $IISF->GetItem( $Handle, 0 );
   if ($Item == null)
   {
      print "\nUnable to retrieve item\n";
      return;
   }

   my $ParsedText = $Item->GetItemParsedText();
   if ($ParsedText eq "")
   {
      print "\nUnable to retrieve item parsed text\n";
      return;
   }

   print "\nFull (DM DB) parsed text for item is:\n\n" . $ParsedText . "\n";
}


# Set the parsing order
# ===============Valid value to be used are:================================
# 0 -- Prefer parsing DLLs (order set elsewhere), then default DB
# 1 -- Prefer parsing DLLs (order set elsewhere), then user DB, DDB
# 2 -- Prefer default DB, then last user DB
# 3 -- Prefer last user DB, tehn default DB
# 4 -- Prefer QCAT, parsing DLLs (order set elsewhere), then default DB
# 5 -- Prefer QCAT, parsing DLLs (order set elsewhere), then user DB
# 6 -- Prefer QCAT, default DB, then last user DB
# 7 -- Prefer QCAT, last user DB, tehn default DB
#============================================================================
sub SetParsingOrder
{
   my $order = shift;
   my $Handle = $IISF->SetParsingOrder($order);
   if ($Handle == 0xFFFFFFFF)
   {
      print "\nUnable to set parsing order:\n$order\n";
      return;
   }
   print"\nParsing Preference is set to: $order\n";
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

   # Dump the full parsed text of an item
   DumpItemParsedText();
}

Execute();