#----------------------------------------------------------------------------
#  QCAT 6.x   Packet Filtering and Iteration Example Script
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Open log file "C:\\Temp\\Sample.dlf"
#    * Loop through packets, searching for specified fields
#    * Release the QCAT Automation Objects
#
# Copyright (c) 2014-2017 Qualcomm Technologies,Inc Proprietary
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
my $LOG_FILE      = "C:\\Temp\\Sample.dlf";

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
# Set Options
#----------------------------------------------------------------------------
$qcat_app->{Visible}    = $VISIBLE;

#----------------------------------------------------------------------------
# Open the Log File.
#----------------------------------------------------------------------------
print "Loading...";
if(!$qcat_app->OpenLog($LOG_FILE))
{
   print "Error: $qcat_app->{LastError}\n\n";
   die;
}
print "Complete.\n\n";


my $packet = $qcat_app->{FirstPacket};
if(!$packet)
{
   print "$qcat_app->{LastError}\n\n";
}
else
{
# Different options for getters
# From 0xB115
#   my $logId = 0xB115;
#   my $fieldString = ".\"Detected Cells\".\"CP\"";
#   my $fieldString = ".\"Detected Cells\"[2].\"CP\"";
# From 0xB11D
   my $logId = 0xB11D;
#   my $fieldString = ".\"Timing Offset\"[1]";
#   my $fieldString = ".\"Timing Offset\"";
   my $fieldString = ".\"Records\".\"Timing Offset\"";
   do
   {
      if($packet->{Type} == $logId && $packet->FieldExists($fieldString))
      {
         print "Index: $packet->{SortedIndex} ";

         my $arr = $packet->GetFieldArray($fieldString);
         my $count = @{$arr};
         print "\tCount: $count \tValues: ";
         my $index = 0;
         while($index < $count)
         {
            my $value = $arr->[$index];
            print "$value ";
            $index++;
         }

         print "\n";
      }
   } while($packet->Next());
}


#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$packet = NULL;
$qcat_app= NULL;
