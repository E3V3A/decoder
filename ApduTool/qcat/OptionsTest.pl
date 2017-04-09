#----------------------------------------------------------------------------
# OptionsTest QCAT 6.x Automation Example
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT Automation Object
#    * Display the Application window
#    * Set/Get the 'Use PC Time' option.
#    * Set/Get the 'Use T53 Standard' option.
#    * Set/Get the 'Split Events on Load' option.
#    * Hide the Application window
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
# Options
#----------------------------------------------------------------------------
my $VISIBLE       = TRUE;

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
# Make the QCAT Window Visible
#----------------------------------------------------------------------------
$qcat_app->{Visible} = $VISIBLE;

#----------------------------------------------------------------------------
# Retrieve the Options and display them.
#----------------------------------------------------------------------------
my $t53_option    = $qcat_app->{UseT53Standard};
my $pct_option    = $qcat_app->{UsePCTime}; 
my $ts_fmt_option = $qcat_app->{TimestampFormat};
my $ts_loc_option = $qcat_app->{TimestampLocale};
my $annotateSILK  = $qcat_app->{AnnotateSILK};
my $date_in_debug = $qcat_app->{ShowDateInDebugMessageFile};

print "-------------------------------------------------------------------\n";
print " CURRENT OPTIONS:                                                \n\n"; 
print " Use T53 Standard:     $t53_option                                 \n";
print " Use PC Time:          $pct_option                                 \n";
print " Timestamp Format:     $ts_fmt_option                              \n";
print " Timestamp Locale:     $ts_loc_option                              \n";
print " Annotate SILK:        $annotateSILK                               \n";
print " Date in Debug File:   $date_in_debug                              \n";
print "-----------------------------------------------------------------\n\n";

#----------------------------------------------------------------------------
# Set the Options to new values.
#----------------------------------------------------------------------------
$qcat_app->{UseT53Standard} = !$t53_option;
$qcat_app->{UsePCTime}      = !$pct_option; 
$qcat_app->{TimestampFormat}= "Calendar";
$qcat_app->{TimestampLocale}= "UTC";
$qcat_app->{AnnotateSILK}   = !$annotateSILK;
$qcat_app->{ShowDateInDebugMessageFile} = !$date_in_debug;

print "-------------------------------------------------------------------\n";
print " NEW OPTIONS:                                                    \n\n"; 
print " Use T53 Standard:     $qcat_app->{UseT53Standard}                 \n";
print " Use PC Time:          $qcat_app->{UsePCTime}                      \n";
print " Timestamp Format:     $qcat_app->{TimestampFormat}                \n";
print " Timestamp Locale:     $qcat_app->{TimestampLocale}                \n";
print " Annotate SILK:        $qcat_app->{AnnotateSILK}                   \n";
print " Date in Debug File:   $qcat_app->{ShowDateInDebugMessageFile}     \n";
print "-----------------------------------------------------------------\n\n";

sleep(5);

#----------------------------------------------------------------------------
# Restore the Options
#----------------------------------------------------------------------------
$qcat_app->{UseT53Standard} = $t53_option;
$qcat_app->{UsePCTime}      = $pct_option; 
$qcat_app->{TimestampFormat}= $ts_fmt_option;
$qcat_app->{TimestampLocale}= $ts_loc_option;
$qcat_app->{AnnotateSILK}   = $annotateSILK;
$qcat_app->{ShowDateInDebugMessageFile} = $date_in_debug;

print "-------------------------------------------------------------------\n";
print " RESTORED OPTIONS:                                               \n\n"; 
print " Use T53 Standard:     $qcat_app->{UseT53Standard}                 \n";
print " Use PC Time:          $qcat_app->{UsePCTime}                      \n";
print " Timestamp Format:     $qcat_app->{TimestampFormat}                \n";
print " Timestamp Locale:     $qcat_app->{TimestampLocale}                \n";
print " Annotate SILK:        $qcat_app->{AnnotateSILK}                   \n";
print " Date in Debug File:   $qcat_app->{ShowDateInDebugMessageFile}     \n";
print "-----------------------------------------------------------------\n\n";

#----------------------------------------------------------------------------
# Hide the QCAT Application Window
#----------------------------------------------------------------------------
$qcat_app->{Visible} = FALSE;

#----------------------------------------------------------------------------
# Clean-up
#----------------------------------------------------------------------------
$qcat_app = NULL;
