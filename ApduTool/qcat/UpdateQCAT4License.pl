#----------------------------------------------------------------------------
# UpdateQCAT4License.pl
# 
# Description:
#  Updates the QCAT 4.x license from the QCAT 6.x license
#  Requires both QCAT 4.x and 6.x to be installed.
#
# Copyright (c) 2006-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;
use Win32::Registry;
use File::Spec;
use constant NULL => 0;
use constant QCAT4_DEFAULT_PATH => "C:\\Program Files\\Qualcomm\\QCAT 4.x\\Bin\\QCAT.exe";

print <DATA>;

my $qcat6Path = GetToolPathFromRegistry("QCAT", "QCAT6.Application");
my $qcat4Path = GetToolPathFromRegistry("QCAT", "QCAT4.Application");
if(not -e $qcat4Path)
{
   $qcat4Path = Win32::GetShortPathName(QCAT4_DEFAULT_PATH);
}

$qcat6Path =~ s/QCAT.exe/license.txt/i;

my $errorMsg;
if(not -e $qcat6Path)
{
   $errorMsg .= "Could not find QCAT 6 installation,\n\tplease verify that QCAT 6 is installed.\n";
}

if(not -e $qcat4Path)
{
   $errorMsg .= "Could not find QCAT 4,\n\tplease (re)install QCAT 4 to \"" . QCAT4_DEFAULT_PATH . "\"\n";
}

if(defined($errorMsg))
{
   die "Error:\n$errorMsg\n";
}

#Delete the old QCAT 4 license file
my $qcat4License = $qcat4Path;
$qcat4License =~ s/QCAT.exe/license.txt/i;
unlink($qcat4License);
`copy $qcat6Path $qcat4License`;
if(0 != $?)
{
   die "Copy Failed: $qcat6Path to $qcat4License";
}
else
{
   print "License Successfully Updated\nAround ";
   print GetLicenseTimeLeft($qcat4License) . " days remaining";
   
   #Register QCAT 4
   print "\n\nRegistering QCAT 4.x...";
   `$qcat4Path -register`;
   if(0 == $?)
   {
      print "Success.\n";
   }
   else
   {
      print "Failed.\n";
   }
}

#----------------------------------------------------------------------------
# GetToolPathFromRegistry
#
# Gets the executable filepath from the registry
#----------------------------------------------------------------------------
sub GetToolPathFromRegistry {
   my $tool = uc(shift);
   my $progId = shift;
   return undef if (!defined($tool) || !defined($progId));


   # get the registry path
   my ($classId, $progPath);
   $::HKEY_CLASSES_ROOT->QueryValue ($progId . "\\CLSID", $classId) or return undef;
   $::HKEY_CLASSES_ROOT->QueryValue ("\\CLSID\\$classId\\LocalServer32", $progPath) or return undef;
   return "Not Installed" if (!$progPath);

   #Must have non-quoted, short format pathname
   $progPath =~ s/\"//g;
   $progPath = Win32::GetShortPathName($progPath);

   return $progPath;
} # end of GetToolPathFromRegistry

#------------------------------------------------------------------------------
# Get the approximate number of days left on a license for a tool with 
# the given prog ID
#------------------------------------------------------------------------------
sub GetLicenseTimeLeft($)
{
   my ($path) = @_;
   
   open(LICENSE, "<$path");
   foreach my $line (<LICENSE>)
   {
      if($line =~ /End.+(\d{4})\s+(\d{2})\s+(\d{2}).*$/)
      {
         my $expYear = $1;
         my $expMonth= $2;
         my $expDay  = $3;
         
         my ($sec,$min,$hour,$mday,$mon,$year,$wday,$yday,$isdst) =
            gmtime(time);
            
         $expYear -= $year + 1900;
         $expMonth-= $mon + 1;
         $expDay  -= $mday;
         $expDay  += $expYear * 365 + $expMonth * 30;
         
         close(LICENSE);
         return $expDay;
      }
   }
   close(LICENSE);
   return undef;
}

__DATA__
#----------------------------------------------------------------------------
# QCAT 4.x License Update
#----------------------------------------------------------------------------
This script updates the license file for QCAT 4.x.

The QCAT team strongly suggests using QCAT 6.x instead.  

QCAT 6.x has signifigant reliability and capacity improvements.

For scripts that automate QCAT directly, simply replace
   $qcat_app = new Win32::OLE 'QCAT4.Application';

With

   $qcat_app = new Win32::OLE 'QCAT6.Application';

For ASIA users, please see the ASIA documentation for how to switch 
from QCAT 4.x to QCAT 6.x.

#----------------------------------------------------------------------------

This script requires that both QCAT 4.x and QCAT 6.x are installed.

