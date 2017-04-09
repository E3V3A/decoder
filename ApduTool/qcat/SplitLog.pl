#----------------------------------------------------------------------------
#  QCAT 6.x   Split Log Script
# 
# Description:
#    This script splits a log file into x equal parts where x is input by
#    the user
#
# Copyright (c) 2012-2017 Qualcomm Technologies,Inc Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;
use Getopt::Long;  # for GetOptions

use constant FALSE => 0;
use constant TRUE  => 1;
use constant NULL  => 0;

my $LOG_FILE      = "";
my $NUM_CHUNKS    = 0;
my $SECONDS       = 0;

GetOptions(
   "file=s" => \$LOG_FILE,
   "sec=s" => \$SECONDS,
   "chunks=s" => \$NUM_CHUNKS
);

#----------------------------------------------------------------------------
# Check that input file exists
#----------------------------------------------------------------------------
if( not -e $LOG_FILE )
{
   die "$LOG_FILE does not exist\n";
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
# Open the Log File.
#----------------------------------------------------------------------------
if(!$qcat_app->OpenLog($LOG_FILE))
{
   print "Error: $qcat_app->{LastError}\n";
   die;
}

#----------------------------------------------------------------------------
# Sort by time so all the packets go into the rigth chunks
#----------------------------------------------------------------------------
if(!$qcat_app->SortByTime())
{
   print "Error: $qcat_app->{LastError}\n";
   die;
}

my $logFilePrefix = substr($LOG_FILE, 0, rindex($LOG_FILE, '.'));

if(0 != $SECONDS)
{
   #----------------------------------------------------------------------------
   # Set the packet window using $SECONDS for each window
   #----------------------------------------------------------------------------
   my $time = 0;
   my $chunks = ($qcat_app->{LogDuration} / $SECONDS) + 1;
   for(my $i = 0; $i < $chunks; ++$i, $time += $SECONDS)
   {
      $qcat_app->SetTimeWindow($time, $SECONDS);
      $qcat_app->SaveAsDLF("$logFilePrefix$i.dlf");
   }
}
elsif(0 != $NUM_CHUNKS)
{
   #----------------------------------------------------------------------------
   # Set the packet window using num packets / num chunks for each window
   #----------------------------------------------------------------------------
   my $numPackets = $qcat_app->{PacketCount};
   my $packetChunkSize = $numPackets / $NUM_CHUNKS;
   my $i = 0;
   for(; $i < $NUM_CHUNKS - 1; ++$i)
   {
      $qcat_app->SetPacketWindow($i * $packetChunkSize, $packetChunkSize);
      $qcat_app->SaveAsDLF("$logFilePrefix$i.dlf");
   }

   # Save all remaing packets in last file
   $qcat_app->SetPacketWindow($i * $packetChunkSize, 2 * $packetChunkSize);
   $qcat_app->SaveAsDLF("$logFilePrefix$i.dlf");

}
else
{
   print "No splitting option selected";
}

#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$qcat_app= NULL;
