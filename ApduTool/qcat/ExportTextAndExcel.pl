#----------------------------------------------------------------------------
# ExportTextAndExcel QCAT 6.x Example Script
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Load a workspace
#    * Load a log file
#    * Get the workspace automation object
#    * Select some analyzers in the workspace
#    * Export the analyzers to text
#    * Export the analyzers to Excel
#    * Release the QCAT Automation Object
#
# Copyright (c) 2006-2017 Qualcomm Technologies,Inc Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;
use constant FALSE => 0;
use constant TRUE  => 1;
use constant NULL  => 0;
use constant ENABLE  => 1;
use constant DISABLE => 0;
use constant NEW_EXCEL_FILE => 0;
use constant APPEND_TO_EXCEL_FILE => 1;

#----------------------------------------------------------------------------
# Options
#----------------------------------------------------------------------------
my $VISIBLE       = TRUE;
my $SPLIT_EVENTS  = TRUE;
my $HEX_DUMP      = FALSE;
my $USE_3X_NAMES  = TRUE;
my $LOG_FILE      = "C:\\Temp\\Sample.dlf";
my $OUTPUT_PATH   = "C:\\Temp";

# Which analyses should be exported?
my @ENABLED_OUTPUTS  = (";WCDMA;Time Grids");

#----------------------------------------------------------------------------
# Check that input file exists
#----------------------------------------------------------------------------
if( not -e $LOG_FILE )
{
   die "This Script requires the exsistence of a file: $LOG_FILE\n";
}

#----------------------------------------------------------------------------
# Start-up the QCAT Application
#----------------------------------------------------------------------------
my $qcat_app = new Win32::OLE 'QCAT6.Application';
if(!$qcat_app)
{
   print "ERROR: Unable to invoke the QCAT application.\n";
   die;
}

#----------------------------------------------------------------------------
# Make the QCAT Window Visible.
#----------------------------------------------------------------------------
$qcat_app->{Visible} = $VISIBLE;

#----------------------------------------------------------------------------
# Open Workspace
#----------------------------------------------------------------------------
print "\nOpen Workspace...";
if(!$qcat_app->LoadWorkspace("")) #empty string loads previous workspace, 
#if(!$qcat_app->LoadWorkspace("C:\\temp\\sample.aws")) # use full path to open specific workspace
{
   print "\nERROR: $qcat_app->{LastError}\n";
   die;
}
print "Complete.\n";

#----------------------------------------------------------------------------
# Load DLF file
#----------------------------------------------------------------------------
print "\nOpen Log File...";
if(!$qcat_app->OpenLog($LOG_FILE))
{
   print "\nERROR: $qcat_app->{LastError}\n";
   die;
}
print "Complete.\n";


#----------------------------------------------------------------------------
# Enable All Selected Displays
#
#  The workspace delimiter is the ';' 
#  The root is identified as ";"
#----------------------------------------------------------------------------
my $workspace = $qcat_app->{Workspace};         #get the workspace object
$workspace->SelectOutput(";", DISABLE);         #deselect all (";" is the root -- so this deselects the root)
foreach my $output ( @ENABLED_OUTPUTS ) 
{
   $workspace->SelectOutput($output, ENABLE);   #select all outputs in the ENABLED_OUTPUTS array
}

#----------------------------------------------------------------------------
#export all selected outputs to tab-delimited text
#----------------------------------------------------------------------------
print "\nExporting Analyzers to Text...";
if(!$workspace->ExportToText($OUTPUT_PATH, "\t"))
{
   print "\nERROR: $qcat_app->{LastError}\n";
}
print "Complete.\n";

#----------------------------------------------------------------------------
#If Excel export desired, export all selected outputs to Excel
#----------------------------------------------------------------------------
print "\nExporting Analyzers to Excel...";
if(!$workspace->ExportToExcel("$OUTPUT_PATH", NEW_EXCEL_FILE)) #Second parameter: 0 = New File; 1 = Append to Existing
{
     print "\nERROR: $qcat_app->{LastError}\n";
}
print "Complete.\n";

print "-" x 60;
print "\n";

#----------------------------------------------------------------------------
#Release the workspace object
#----------------------------------------------------------------------------
$workspace = NULL;

sleep(10);

$qcat_app->{Visible} = FALSE;

#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$qcat_app = NULL;

