# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl QXDMExportViewText.pl

# This script demostrates usage of the QXDM2 automation
# interface method ExportViewText

use HelperFunctions4;

# Global variable
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
   }

   SetQXDM( $QXDM );

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
   $FileName = $FolderPath . "Example.isf";
   return $FileName;
}
  
# Export all items from item view to a text file
sub ExportViewText
{
   # Assume failure
   my $RC = false;

# Generate item store file name
   my $FileName = GetFileName();
   if ($FileName eq "")
   {
      return;
   }

   # Load the item store file
   my $Handle = $QXDM->LoadItemStore( $FileName );
   if ($Handle == 0xFFFFFFFF)
   {
      print "\nUnable to load ISF:\n$FileName\n";
      return;
   }
   

   # Get path for QXDM installation
   my $FileName = GenerateFileName( "", ".txt" );
   if ($FileName eq "")
   {
      return;
   }

   # Export all items from item view
   $RC = $QXDM->ExportViewText( "Item View", $FileName );
   if ($RC == false)
   {
      print "\nUnable to export items, 'Item View' not found\n";

      return;
   }

   print "\nItems exported to item store file:\n"
       . "$FileName\n";
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

   # Export all items from item view to a text file
   ExportViewText();
}

Execute();