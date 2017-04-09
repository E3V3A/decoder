#----------------------------------------------------------------------------
#    Q C A T6   A u t o m a t i o n   I n t e r f a c e   S c r i p t
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT 6 Automation Object
#    * Open a log file
#    * Retrieve the QCAT Version
#    * Close the log file
#    * Release the QCAT Automation Object
#
# Copyright (c) 2012-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;
use Win32::OLE::Variant;
use Win32::Registry;
use File::Spec;

my $num_args = $#ARGV + 1;
if ($num_args != 1) 
{
	 print "Error!  Expecting 1 command line argument (the file to be opened)\n";
	 die;
}

use constant NULL => 0;
use constant FALSE => 0;
use constant TRUE  => 1;

my constant $VALID_LOG_FILE = $ARGV[0];

my $success = 0;
my $failure = 0;

#----------------------------------------------------------------------------
# Start-up the QCAT Application
#----------------------------------------------------------------------------
my $qcat_app = new Win32::OLE 'QCAT6.Application';

if(!$qcat_app)
{
   print "ERROR: Unable to invoke the QCAT application.\n";
   die;
}

if (!$qcat_app->OpenLog($VALID_LOG_FILE))
{
	 print "ERROR: Unable to open log file at location: $VALID_LOG_FILE \n";
	 die;
}
 
#----------------------------------------------------------------------------
# Retrieve the QCAT Version and print it
#----------------------------------------------------------------------------
print "\nQCAT Version: $qcat_app->{appVersion}\n\n";

#----------------------------------------------------------------------------
# Close the log file
#----------------------------------------------------------------------------
$qcat_app->closeFile();
print "\nClose the log file\n\n";

$qcat_app = NULL;
