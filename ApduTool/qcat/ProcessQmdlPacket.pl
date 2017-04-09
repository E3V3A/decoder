#----------------------------------------------------------------------------
# ProcessQmdlPacket QCAT 6.x Automation Example
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Retrieve the QCAT Version information
#    * Retrieve a list of the supported packet types
#    * Parse a single packet using the ProcessQmdlPacket
#
# Copyright (c) 2005-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;
use Win32::OLE::Variant;

my $LogPacket = 
'92 02 03 00 03 6C B5 C7 43 00 00 00 F0 02 00 00 
 04 00 00 00 E8 F2 09 82 00 00 00 00 00 00 00 00 
 00 00 00 00 DD 9F 7E';

# Setup a handler to catch any OLE errors we get.

my $ExpOLEError = "NONE";
Win32::OLE->Option(Warn => \&OLEWarnHandler );

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
# Retrieve the QCAT/SILK Version and print it
#----------------------------------------------------------------------------
print "QCAT Version: $qcat_app->{AppVersion}\n";
print "SILK Version: $qcat_app->{SILKVersion}\n\n";

#----------------------------------------------------------------------------
# Print the list of supported packets
#----------------------------------------------------------------------------
my $packet_list = $qcat_app->{SupportedPackets};
foreach my $type (@$packet_list)
{
   printf("0x%X -- %s\n", $type, $qcat_app->GetPacketTypeName($type));
}

#----------------------------------------------------------------------------
# Pack QCAT HexDump to Binary Packet
#----------------------------------------------------------------------------
my @Bytes = split(/[ \n][ \n]*/, $LogPacket);
my @HexBytes;
foreach (@Bytes)
{
   push @HexBytes,eval "0x$_";
}
my $Packet = pack("C39", @HexBytes);

my $VarPacket = Variant(VT_UI1, $Packet);

#----------------------------------------------------------------------------
# This packet cannot be parsed correctly without knowing the Mode
#----------------------------------------------------------------------------
$qcat_app->{Mode} = 1; #CDMA mode
my $Obj = $qcat_app -> ProcessQmdlPacket($VarPacket);
if( !defined($Obj) )
{
	print "$qcat_app->{LastError}\n";
}
else
{
	print $Obj . "\n";
	print $Obj->Text();
}

print "\nDone\n";

sub OLEWarnHandler
{
   print "$qcat_app->{LastError}\n";

   # Die if error isn't expected

   die "Unexpected OLE Error\n$_[0]\n" if $ExpOLEError eq "NONE";

   # Die if this is the wrong error

   die "Wrong OLE Error ($ExpOLEError)\n$_[0]\n" if ($_[0] !~ $ExpOLEError);

   # Clear expected error flag and return

   $ExpOLEError = "NONE";
}
