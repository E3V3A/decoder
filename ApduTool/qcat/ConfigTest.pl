#----------------------------------------------------------------------------
# ConfigTest QCAT 6.x Automation Example
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Retrieve the QCAT Version information
#    * Print the list of all configuration properties
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
# Retrieve and print the list of all Available configuration properties
#----------------------------------------------------------------------------
my $PRINT_PROPERTY_VALUES = TRUE;
print $qcat_app->GetPropertyList($PRINT_PROPERTY_VALUES);

#----------------------------------------------------------------------------
# Retrieve and print the Annotate SILK property
#----------------------------------------------------------------------------
my $value = $qcat_app->GetProperty("Annotate SILK");
print "\n $value";
print "\n $qcat_app->{LastError}\n\n";

#----------------------------------------------------------------------------
# Set the Annotate SILK property
#----------------------------------------------------------------------------
my $success = $qcat_app->PutProperty("Annotate SILK", TRUE);
if(!$success)
{
   print "\n " . Win32::OLE->LastError();
   print "\n $qcat_app->{LastError}\n";
}

#----------------------------------------------------------------------------
# Retrieve and print the Annotate SILK property
#----------------------------------------------------------------------------
$value = $qcat_app->GetProperty("Annotate SILK");
print "\n $value";
print "\n $qcat_app->{LastError}\n\n";

#----------------------------------------------------------------------------
# Set the Annotate SILK property
#----------------------------------------------------------------------------
$success = $qcat_app->PutProperty("Annotate SILK", FALSE);
if(!$success)
{
   print "\n " . Win32::OLE->LastError();
   print "\n $qcat_app->{LastError}\n";
}

#----------------------------------------------------------------------------
# Retrieve and print the Annotate SILK property
#----------------------------------------------------------------------------
$value = $qcat_app->GetProperty("Annotate SILK");
print "\n $value";
print "\n $qcat_app->{LastError}\n\n";

#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$qcat_app = NULL;

