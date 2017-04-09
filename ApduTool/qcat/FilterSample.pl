#----------------------------------------------------------------------------
#  QCAT 6.x   Packet Filtering Example Script
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Display the Application window
#    * Set the packet filter to only load debug messages
#    * Process the Log File and Save as Text
#    * Set the packet filter to only show event messages
#    * Process the Log File and Save as Text again
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
my $VISIBLE       = FALSE;
my $HEX_DUMP      = FALSE;
my $USE_3X_NAMES  = FALSE;
my $SAVE_EVENT_DLF= TRUE;
my $LOG_FILE      = "C:\\Temp\\Sample.dlf";
my $DEBUG_FILE    = "C:\\Temp\\Sample_DebugMsgs.txt";
my $EVENTS_FILE   = "C:\\Temp\\Sample_Events.txt";

my @DEBUG_PACKETS = ( 0x1018, 0x1FEC, 0x1FEB );
my @EVENT_PACKETS = ( 0x1FFB );

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
# Make the QCAT Window Visible.
#----------------------------------------------------------------------------
$qcat_app->{Visible}    = $VISIBLE;

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
foreach my $packet ( @DEBUG_PACKETS )
{
   $filter->Set($packet, TRUE);   #Enable Debug messages
}
$filter->Commit(); #Commit the filter

#----------------------------------------------------------------------------
# Process the Log File.
#----------------------------------------------------------------------------
print "Parsing debug messages...";
if(!$qcat_app->Process($LOG_FILE, $DEBUG_FILE, $HEX_DUMP, $USE_3X_NAMES))
{
   print "\nError: $qcat_app->{LastError}\n";
}
else
{
   print "Complete.\n";
}
print "\n";

#----------------------------------------------------------------------------
# Reset the filter
#----------------------------------------------------------------------------
$filter->SetAll(FALSE);
foreach my $packet ( @EVENT_PACKETS )
{
   $filter->Set($packet, TRUE);   #Enable event messages
}
$filter->Commit(); #Commit the filter

#----------------------------------------------------------------------------
# Save the events only as a new DLF file
#----------------------------------------------------------------------------
if($SAVE_EVENT_DLF)
{
   if(!$qcat_app->SaveAsDLF("C:\\temp\\events.dlf"))
   {
      print "Error: $qcat_app->{LastError}\n";
      die;
   }
}

#----------------------------------------------------------------------------
# Process the Log File again
#----------------------------------------------------------------------------
print "Parsing events...";
if(!$qcat_app->SaveAsText($EVENTS_FILE))
{
   print "\nError: $qcat_app->{LastError}\n";
}
else
{
   print "Complete.\n";
}
print "\n";

#----------------------------------------------------------------------------
# Hide the QCAT Application Window
#----------------------------------------------------------------------------
$qcat_app->{Visible} = FALSE;

#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$filter  = NULL;
$qcat_app= NULL;

system("notepad $DEBUG_FILE");
system("notepad $EVENTS_FILE");