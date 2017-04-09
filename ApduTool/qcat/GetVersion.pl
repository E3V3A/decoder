#----------------------------------------------------------------------------
#    Q C A T   A u t o m a t i o n   I n t e r f a c e   S c r i p t # 1
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Retrieve the QCAT Version information
#    * Retrieve the SILK Version information
#    * Release the QCAT Automation Object
#
# Copyright (c) 2005-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;
use Win32::Registry;
use File::Spec;
use constant NULL => 0;

#----------------------------------------------------------------------------
# It is often faster to obtain the version of a tool directly from the 
# executable file rather than by automation if no other automation is 
# required.
#
# GetToolRegistryVersion is defined at the end of this file
#----------------------------------------------------------------------------
print "Getting version information from the executable.\n";
my $version = GetToolRegistryVersion("QCAT", "QCAT6.Application");
print("QCAT6 $version\n");

$version = GetToolRegistryVersion("QXDM", "QXDM.Application");
print("QXDM $version\n");

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
# Clean-up
#----------------------------------------------------------------------------
$qcat_app = NULL;

#----------------------------------------------------------------------------
# GetToolRegistryVersion
#
# Gets the executable filepath from the registry, then gets the version info
# from the executable file.
#----------------------------------------------------------------------------
sub GetToolRegistryVersion {
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

   # get version
   return "Unknown" if !-e $progPath;
   $progPath = File::Spec->rel2abs($progPath) if $progPath;
   my $fso = Win32::OLE-> new('Scripting.FileSystemObject');
   return "Unknown" unless $fso;
   my $ver = $fso-> GetFileVersion($progPath);
   undef $fso;
   return "Unknown" if !$ver;
   return $ver;
} # end of GetToolRegistryVersion