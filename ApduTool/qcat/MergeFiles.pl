#----------------------------------------------------------------------------
#    QCAT6   A u t o m a t i o n   I n t e r f a c e   S c r i p t
# 
# Description:
#    This script merges all log files in the given directory into a single isf
#
# Copyright (c) 2017 Qualcomm Technologies,Inc Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use Win32::OLE;
use Cwd;

use constant NULL => 0;
use constant FALSE=> 0;
use constant TRUE => 1;


my $directory = getcwd();
my $sort = FALSE;

if ($#ARGV > 0) # if filename is supplied
{
   $directory = $ARGV;
}

$directory = "$directory/";
my @files = (<*.dlf>, <*.isf>, <*.qmdl>, <*.qmdl2>);
my $newlist;
foreach my $file (@files)
{
   $newlist = join "|", map { $directory . $_ } @files;
}
print "Files to merge: $newlist\n";


my $qcat_app = new Win32::OLE 'QCAT6.Application';
if(!$qcat_app)
{
   print "ERROR: Unable to invoke the QCAT application.\n";
   die;
}

$qcat_app->{Visible}    = TRUE;
$qcat_app->OpenLog("$newlist");

if($sort)
{
   $qcat_app->SortByTime();
}

$qcat_app->SaveAsISF("MergedFile.isf");

$qcat_app= NULL;
