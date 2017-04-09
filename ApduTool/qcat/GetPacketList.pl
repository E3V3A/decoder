#----------------------------------------------------------------------------
# GetPacketList.pl
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Retrieve the QCAT Version information
#    * Retrieve the SILK Version information
#    * Print the list of packet types supported by QCAT
#    * Release the QCAT Automation Object
#
# Copyright (c) 2005-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;
use constant NULL => 0;

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
# Retrieve the SILK Version and print it
#----------------------------------------------------------------------------
print "\nSILK Version: $qcat_app->{SILKVersion}\n\n";

#----------------------------------------------------------------------------
# Print the list of supported packets
#----------------------------------------------------------------------------
my $packet_list = $qcat_app->{SupportedPackets};
foreach my $type (@$packet_list)
{
   printf("0x%X -- %s\n", $type, $qcat_app->GetPacketTypeName($type));
}

#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$qcat_app = NULL;

