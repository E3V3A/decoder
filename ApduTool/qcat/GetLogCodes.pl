#----------------------------------------------------------------------------
#  QCAT 6.x   Get packets used by an analyzer example
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Query the packets and events used by an analyzer
#
# Copyright (c) 2014-2017 Qualcomm Technologies, Inc Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;

use Time::HiRes qw(usleep);

use constant FALSE => 0;
use constant TRUE  => 1;
use constant NULL  => 0;

#----------------------------------------------------------------------------
# Start-up the QCAT Application
#----------------------------------------------------------------------------
my $qcat_app = new Win32::OLE 'QCAT6.Application';

if(!$qcat_app)
{
   print "ERROR: Unable to invoke the QCAT application.\n";
   die;
}

my $workspace = $qcat_app->{Workspace};

my $arr = $workspace->GetRequiredPackets(";LTE;Summary;LTE Carrier Agg Summary");

my $count = @{$arr};
print "\tPackets: $count \tValues: ";
my $index = 0;
while($index < $count)
{
   my $value = $arr->[$index];
   print sprintf("0x%2X", $value);
   print " ";
   $index++;
}

$arr = $workspace->GetRequiredEvents(";LTE;Summary;LTE Carrier Agg Summary");

$count = @{$arr};
print "\n\tEvents: $count \tValues: ";
$index = 0;
while($index < $count)
{
   my $value = $arr->[$index];
   print "$value ";
   $index++;
}


#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$workspace = NULL;
$qcat_app= NULL;
