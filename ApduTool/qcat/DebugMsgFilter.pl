#----------------------------------------------------------------------------
#  QCAT 6.x   Debug Messages Filtering Example Script
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Display the Application window
#    * Set the debug messages filter 
#    * Process the Log File and Save as Text
#    * Optionally save the filtered log file as a new DLF
#    * Hide the Application window
#    * Release the APEX Automation Object
#
# Copyright (c) 2011-2017 Qualcomm Proprietary
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
my $HEX_DUMP      = FALSE;
my $USE_3X_NAMES  = FALSE;
my $SAVE_DLF	  = TRUE;
my $LOG_FILE      = "C:\\Temp\\Sample.dlf";
my $TEXT_FILE     = "C:\\Temp\\Sample_DebugMsgFilter.txt";

##Debug Messages SSID and LEVEL to filter out
my @SSID  = (0,1,2,3);  ## F3 message SubsystemId
my @LEVEL = (0,1,2);    ##  F3 message Level

#----------------------------------------------------------------------------
# Start-up the APEX Application
#----------------------------------------------------------------------------
my $qcat_app = new Win32::OLE 'QCAT6.Application';

if(!$qcat_app)
{
   print "ERROR: Unable to invoke the APEX application.\n";
   die;
}

#----------------------------------------------------------------------------
# Set Options
#----------------------------------------------------------------------------
$qcat_app->{Visible}    = $VISIBLE;

#----------------------------------------------------------------------------
# Get the filter
#----------------------------------------------------------------------------
my $filter = $qcat_app->{DebugMsgFilter};
if(!$filter)
{
   print "ERROR: Unable to retrieve the DebugMsgFilter Object.\n";
   die;
}

#----------------------------------------------------------------------------
# Set the filter
#----------------------------------------------------------------------------
$filter->SetAll(FALSE);           #Disable everything
 
foreach my $subsysId (@SSID) 
{
   foreach my $level (@LEVEL) 
   {
      $filter->Set($subsysId, $level, TRUE);   #Enable Debug Message of type [subsysId/level]
   }
}   

$filter->Commit(); #Commit the filter

#----------------------------------------------------------------------------
# Process the Log File.
#----------------------------------------------------------------------------
if(!$qcat_app->Process($LOG_FILE, $TEXT_FILE, $HEX_DUMP, $USE_3X_NAMES))
{
   print "Error: $qcat_app->{LastError}\n";
   die;
}

#----------------------------------------------------------------------------
# Save the debug messages only as a new DLF file
#----------------------------------------------------------------------------
if($SAVE_DLF)
{
   if(!$qcat_app->SaveAsDLF("C:\\Tmp\\Test_DebugMsgFilter_flt.dlf"))
   {
      print "Error: $qcat_app->{LastError}\n";
      die;
   }
}

#----------------------------------------------------------------------------
# Hide the APEX Application Window
#----------------------------------------------------------------------------
$qcat_app->{Visible} = FALSE;

#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$filter  = NULL;
$qcat_app= NULL;

system("notepad $TEXT_FILE");