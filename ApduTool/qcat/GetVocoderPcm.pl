#----------------------------------------------------------------------------
# QCAT 6.x Automation Example
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Display the Application window
#    * Generate the vocoder PCM output files
#    * Hide the Application window
#    * Release the QCAT Automation Object
#
# Copyright (c) 2006-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;
use constant FALSE => 0;
use constant TRUE  => 1;
use constant NULL  => 0;

use constant AUTO_MODE => 0; #requires 0x7143/0x7144/0x14D2/0x1804/0x1805 vocoder packets
use constant AMR_MODE => 1;
use constant EFR_MODE => 2;
use constant FR_MODE => 3;
use constant HR_MODE => 4;
use constant EVRC_MODE => 5;
use constant V13K_MODE => 6;
use constant AMR_WB_MODE => 7;
use constant EVRC_B_MODE => 8;
use constant EVRC_WB_MODE => 9;
use constant EVRC_NW_MODE => 10;
use constant EAMR_MODE => 11;
use constant EVRC_NW2K_MODE => 12;
use constant EVS_MODE => 13;
use constant G711_MODE => 14;

#----------------------------------------------------------------------------
# Options
#----------------------------------------------------------------------------
my $VISIBLE       = FALSE;
my $SPLIT_EVENTS  = TRUE;
my $HEX_DUMP      = FALSE;
my $USE_3X_NAMES  = TRUE;
my $LOG_FILE      = "C:\\Temp\\Sample.dlf";
my $RX_OUTPUT_FILE= "C:\\Temp\\Sample_rx.pcm";
my $TX_OUTPUT_FILE= "C:\\Temp\\Sample_tx.pcm";

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
# Open the Log File.
#----------------------------------------------------------------------------
print "Opening log file...";
if(!$qcat_app->OpenLog($LOG_FILE))
{
   print "\nError: $qcat_app->{LastError}\n";
   die;
}
print "Complete.\n";

#----------------------------------------------------------------------------
# Get the PCM files
#----------------------------------------------------------------------------
print "Generating PCM files...";
if(!$qcat_app->GenerateVocoderPCM($TX_OUTPUT_FILE, $RX_OUTPUT_FILE, AMR_MODE))
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