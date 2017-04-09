#----------------------------------------------------------------------------
#    Q C A T   A u t o m a t i o n   I n t e r f a c e   S c r i p t # 3
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Display the Application window
#    * Process a Log File and Save as Text
#    * Hide the Application window
#    * Release the QCAT Automation Object
#
# Copyright (c) 2005-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;
use constant FALSE => 0;
use constant TRUE  => 1;
use constant NULL  => 0;

#----------------------------------------------------------------------------
# Options
#----------------------------------------------------------------------------
my $VISIBLE       = TRUE;
my $SPLIT_EVENTS  = TRUE;
my $HEX_DUMP      = FALSE;
my $USE_3X_NAMES  = TRUE;
my $LOG_FILE      = "C:\\Temp\\Sample.dlf";
my $OUTPUT_PATH   = "C:\\Temp\\Sample.txt";

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
# Retrieve the QCAT and SILK Version and print them
#----------------------------------------------------------------------------
print "\n";
print "QCAT Version: $qcat_app->{AppVersion}\n";
print "SILK Version: $qcat_app->{SILKVersion}\n\n";

#----------------------------------------------------------------------------
# Make the QCAT Window Visible.
#----------------------------------------------------------------------------
$qcat_app->{Visible} = $VISIBLE;

#----------------------------------------------------------------------------
# Process the Log File.
#----------------------------------------------------------------------------
print "Processing log file...";
if(!$qcat_app->Process("C:\\Temp\\Sample.dlf", "C:\\Temp\\Sample.txt", 0, 1))
{
   print "\nError: $qcat_app->{LastError}\n";
   die;
}
print "Complete.\n";

#----------------------------------------------------------------------------
# Hide the QCAT Application Window
#----------------------------------------------------------------------------
$qcat_app->{Visible} = FALSE;

#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$qcat_app = NULL;

