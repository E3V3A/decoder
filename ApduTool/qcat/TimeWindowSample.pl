#----------------------------------------------------------------------------
#  QCAT 6.x   Time Windowing Example Script
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Display the Application window
#    * Set the time window the file to a fraction of its original length
#    * Save as Text
#    * Release the QCAT Automation Object
#
# Copyright (c) 2006-2017 Qualcomm Proprietary
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
my $SHOW_UI       = TRUE;
my $HEX_DUMP      = FALSE;
my $QCAT_3X_NAMING= FALSE;
my $USE_PC_TIME   = FALSE;
my $LOG_FILE      = "C:\\Temp\\Sample.dlf";
my $TEXT_FILE     = "C:\\temp\\TimeWindowSample.txt";

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
# Retrieve the QCAT Version and print it
#----------------------------------------------------------------------------
print "QCAT Version: $qcat_app->{AppVersion}\n";
print "SILK Version: $qcat_app->{SILKVersion}\n";

#----------------------------------------------------------------------------
# Set Options
#----------------------------------------------------------------------------
my $WAS_VISIBLE               = $qcat_app->{Visible}; #save the visible state
$qcat_app->{Visible}          = $SHOW_UI;
$qcat_app->{UsePCTime}        = $USE_PC_TIME;
$qcat_app->{V3xCompatibility} = $QCAT_3X_NAMING;
$qcat_app->{ShowHexDump}      = $HEX_DUMP;

#----------------------------------------------------------------------------
# Open the Log File.
#----------------------------------------------------------------------------
if(!$qcat_app->OpenLog($LOG_FILE))
{
   print "Error: $qcat_app->{LastError}\n";
   die;
}

#----------------------------------------------------------------------------
# Get Log Duration Info
#----------------------------------------------------------------------------
my $startTime = $qcat_app->{LogStartTime};
my $time = gmtime($startTime);
print "Start Time: $time\n";
$time = gmtime($qcat_app->{LogEndTime});
print "End Time: $time\n";
print "Duration: $qcat_app->{LogDuration} (sec)\n";

#----------------------------------------------------------------------------
# Set the time window using an offset and length in seconds
#----------------------------------------------------------------------------
my $offset = 0.0; #$qcat_app->{LogDuration} / 2.0;
my $length = $qcat_app->{LogDuration} / 4.0;

print "Set time window offset: $offset (sec) length: $length (sec)\n";
$time = gmtime($startTime + $offset);
print "Offset Time: $time\n";
$time = gmtime($startTime + $offset + $length);
print "End Time: $time\n";

$qcat_app->SetTimeWindow($offset, $length);

#----------------------------------------------------------------------------
# Can also window the file by packet numbers 
# packet numbers refer to packets in time order
#----------------------------------------------------------------------------
#$qcat_app->SetPacketWindow(3, 5);

#----------------------------------------------------------------------------
# Save the text file
#----------------------------------------------------------------------------
if(!$qcat_app->SaveAsText($TEXT_FILE))
{
   print "Error: $qcat_app->{LastError}\n";
   die;
}

#----------------------------------------------------------------------------
# reset QCAT visibility
#----------------------------------------------------------------------------
$qcat_app->{Visible} = $WAS_VISIBLE;

#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$qcat_app= NULL;
