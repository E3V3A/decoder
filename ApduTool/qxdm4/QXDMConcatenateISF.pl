# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl QXDMConcatenateISF.pl

# This script demostrates usage of the ColorItem automation
# interface method QXDMConcatenateISF()

use HelperFunctions4;


my $FolderPath = GetPathFromScript();
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
# Get file name from script folder path
sub GetFileName1
{
   my $FileName = "";
   
   # Item store file name
   $FileName = $FolderPath . "Example.isf";
   return $FileName;
}

# Get file name from script folder path
sub GetFileName2
{
   my $FileName = "";
 
   # Item store file name
   $FileName = $FolderPath . "Example_2.isf";
   return $FileName;
}
sub ConcatenateISF
{
	 # Generate item store file name
    my $FileName1 = GetFileName1();
	my $FileName2 = GetFileName2();
    if ($FileName1 eq "" || $FileName2 eq "")
    {
       return;
    }
	
	my $OutputFileName = $FolderPath. "Concatenate_Sample_Output.isf";
	
	my $RC = 0;
	$RC = $QXDM2->ConcatenateISF( $FileName1,
							    $FileName2,
							    $OutputFileName);
	if ( $RC == 0 )
	{
		print "\nConcatenate ISF files failed";
		
	}
	else
	{
		print "\nConcatenate ISF fils succeed!: "
			. "\n'$FileName1' concatenate with '$FileName2' \n"
			. " Output File '$OutputFileName' \n";
	}
	return;
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

   # Get the name of a parsed field
   ConcatenateISF();
   
   return;
}

Execute();