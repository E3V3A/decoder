#----------------------------------------------------------------------------
#  QCAT 6.x   Packet Filtering and Iteration Example Script
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Open log file "C:\\Temp\\Sample.dlf"
#    * Set the packet filter to only load debug messages
#    * Loop through packets, printing text, until ^Z is signalled on STDIN
#    * Release the QCAT Automation Objects
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
my $VISIBLE       = FALSE;
my $SPLIT_EVENTS  = TRUE;
my $HEX_DUMP      = FALSE;
my $USE_3X_NAMES  = FALSE;
my $LOG_FILE      = "C:\\Temp\\Sample.dlf";
my $OUTPUT_PATH   = "C:\\Temp\\Sample.txt";

#Filter so that only these packets show up
my @PACKET_TYPES = ( 0x1018, 0x8000, 0x1FEC, 0x1FEB );

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
print "\nQCAT Version: $qcat_app->{AppVersion}\n\n";

#----------------------------------------------------------------------------
# Set Options
#----------------------------------------------------------------------------
$qcat_app->{Visible}    = $VISIBLE;
$qcat_app->{SplitEvents}= $SPLIT_EVENTS;

#----------------------------------------------------------------------------
# Get the filter
#----------------------------------------------------------------------------
my $filter = $qcat_app->{PacketFilter};
if(!$filter)
{
   print "ERROR: Unable to retrieve the Filter Object.\n";
   die;
}

#----------------------------------------------------------------------------
# Set the filter
#----------------------------------------------------------------------------
$filter->SetAll(FALSE);           #Disable everything
foreach my $packet ( @PACKET_TYPES )
{
   $filter->Set($packet, TRUE);   #Enable 
}
$filter->Commit(); #Commit the filter

#----------------------------------------------------------------------------
# Open the Log File.
#  Alternatively, we could process this filtered log file to text using:
#     $qcat_app->Process($LOG_FILE, $OUTPUT_PATH, $HEX_DUMP, $USE_3X_NAMES);
#----------------------------------------------------------------------------
print "Loading...";
if(!$qcat_app->OpenLog($LOG_FILE))
{
   print "Error: $qcat_app->{LastError}\n\n";
   die;
}
print "Complete.\n";
print "$qcat_app->{PacketCount} packets.\n\n";

#----------------------------------------------------------------------------
# Get the packet text until ^Z (eof on stdin) is signalled
#----------------------------------------------------------------------------
print "\"Ctrl-Z...Enter\" to exit. \"Enter\" key to continue.\n\n";
my $packet = $qcat_app->{FirstPacket};
do
{
   print "$packet->{Text}\n";
} while($packet->Next() && <STDIN>);

#----------------------------------------------------------------------------
# Hide the QCAT Application Window
#----------------------------------------------------------------------------
$qcat_app->{Visible} = FALSE;

#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$packet = NULL;
$filter  = NULL;
$qcat_app= NULL;
