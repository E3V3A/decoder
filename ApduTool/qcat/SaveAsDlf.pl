#----------------------------------------------------------------------------
#  QCAT 6 Automation Example Script
#
# 1) takes 2 arguments: input file and output file
# 2) loads the input file
# 3) saves the output file (DLF format)
#
# Copyright (c) 2010-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use warnings;
use Win32::OLE;
use constant FALSE   => 0;
use constant TRUE    => 1;
use constant NULL    => 0;

#----------------------------------------------------------------------------
# Script Entry Point
#----------------------------------------------------------------------------
if (2 != scalar(@ARGV)) # if input and output names are not supplied
{
	die "Provide input and output file names\n";
}

my ($input, $output) = @ARGV;

# Verify that the input file exists
if(not -e $input)
{
	die "Input file $input does not exist\n";
}

# QCAT Application
my $qcat_app = new Win32::OLE 'QCAT6.Application';
if(!$qcat_app)
{
  print "ERROR: Unable to invoke the QCAT application.\n";
  die;
}

# Load input file
print "\nOpen Input File ($input)...";
if(!$qcat_app->OpenLog($input))
{
  print "\nERROR: $qcat_app->{LastError}\n";
  die;
}
print "Complete.\n";

# Save output file
print "Save Output File ($output)...";
if(!$qcat_app->SaveAsDLF($output))
{
  print "\nERROR: $qcat_app->{LastError}\n";
  die;
}
print "Complete.\n";

# All done, Exit
if($qcat_app != NULL)
{
  $qcat_app->{Visible} = FALSE;
  $qcat_app = NULL;
}
exit(0);
