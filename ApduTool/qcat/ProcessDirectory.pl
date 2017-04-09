#----------------------------------------------------------------------------
#  QCAT 6.x Automation Example Script
#
#  Process either a single log file or all the "dlf" files in the current 
#  working directory.
#
#  Creates the parsed text file.
#
# Requirements:
#  Must be run from a directory containing log files with a "dlf" extension
#  Or a full path to a "dlf" file must be supplied on the command line
#
# Copyright (c) 2005-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;
use Cwd;
use constant FALSE   => 0;
use constant TRUE    => 1;
use constant NULL    => 0;
use constant ENABLE  => 1;
use constant DISABLE => 0;

#----------------------------------------------------------------------------
# Settings
#----------------------------------------------------------------------------
my $HEX_DUMP         = FALSE;
my $QCAT_3X_NAMING   = FALSE;
my $SHOW_UI          = FALSE;
my $USE_PC_TIME      = FALSE;
my $SORT_BY_TIME     = FALSE;
my $SPLIT_EVENTS     = TRUE;
my $DO_SAVE_AS_TEXT  = TRUE;    #Should parsed text file be saved
my $PREFIX_WITH_LOG_NAME = FALSE; # TRUE => TextOutputs will be prefixed with the name of the log file
                                  # FALSE=> TextOutputs will be named only with the name of the analyzer

#----------------------------------------------------------------------------
# QCAT Application
#----------------------------------------------------------------------------
my $qcat_app = NULL;

#----------------------------------------------------------------------------
# Script Entry Point
#----------------------------------------------------------------------------
print "Split Events: $SPLIT_EVENTS\n";
print "Sort by Time: $SORT_BY_TIME\n";
print "Hex Dump: $HEX_DUMP\n";
print "3x Names: $QCAT_3X_NAMING\n";
print "PC Time:  $USE_PC_TIME\n";
print "Show UI:  $SHOW_UI\n";

InitQCAT();

if ($#ARGV > -1) # if filename is supplied
{
   foreach my $file (@ARGV) 
   {
      if( -d $file )
      {
         ProcessDirectory($file, getcwd());
      }
      elsif( -e $file )
      {
         ProcessSingleFile($file, getcwd());
      }
      else
      {
         print "\"$file\" is not a vaild file or directory.\n";
      }
   }
}
else # process all logs in the current directory
{
   my $directory = getcwd();
   print "Processing: \"$directory\"\n";

   #----------------------------------------------------------------------------
   # Entry Point if no filename (Process all directory entries)
   #----------------------------------------------------------------------------
   ProcessDirectory($directory, $directory);
}

CloseQCAT();


#----------------------------------------------------------------------------
# Startup QCAT
#----------------------------------------------------------------------------
sub InitQCAT()
{
   $qcat_app = new Win32::OLE 'QCAT6.Application';
   if(!$qcat_app)
   {
      print "ERROR: Unable to invoke the QCAT application.\n";
      die;
   }

   #----------------------------------------------------------------------------
   # Retrieve the QCAT Version and print it
   #----------------------------------------------------------------------------
   print "QCAT Version: $qcat_app->{AppVersion}\n";
   print "SILK Version: $qcat_app->{SILKVersion}\n\n";

   #----------------------------------------------------------------------------
   # Set QCAT settings
   #----------------------------------------------------------------------------
   $qcat_app->{SplitEvents}      = $SPLIT_EVENTS;
   $qcat_app->{Visible}          = $SHOW_UI;
   $qcat_app->{UsePCTime}        = $USE_PC_TIME;
   $qcat_app->{PrefixLogName}    = $PREFIX_WITH_LOG_NAME;
   $qcat_app->{V3xCompatibility} = $QCAT_3X_NAMING;
   $qcat_app->{ShowHexDump}      = $HEX_DUMP;
}

#----------------------------------------------------------------------------
# Close down QCAT
#----------------------------------------------------------------------------
sub CloseQCAT()
{
   if($qcat_app != NULL)
   {
      $qcat_app->{Visible} = FALSE;
      $qcat_app = NULL;
   }
}

#----------------------------------------------------------------------------
# ProcessDirectory( <input_directory>, <output_directory> )
#  For each .dlf file in <input_directory>
#     - Write the parsed text file to "<output_directory>\<log_file_name>.txt"
#     - Write the Debug Msgs vs. Time analysis to 
#         "<output_directory>\<log_file_name>_Debug Messages vs. Time.txt"
#----------------------------------------------------------------------------
sub ProcessDirectory($$)
{
   my ($input_dir, $output_dir) = @_;

   if(not -d $input_dir)
   {
      print "\nThis script requires that the directory \"$input_dir\" exist and contain at least one \".dlf\" file.\n";
      exit(1);
   }
   else
   {
      print "Input Directory: \"$input_dir\"\n";
   }

   if(not -d $output_dir)
   {
      print "\nThis script requires that the directory \"$output_dir\" exist to receive output files.\n";
      exit(1);
   }
   else
   {
      print "Output Directory: \"$output_dir\"\n";
   }

   my $currentDir = getcwd();
   chdir($input_dir);

   my @files = <*.dlf>;
   foreach my $file (@files)
   {
      my $file_name = $file;
      print "File Name: \"$file_name\"\n";
      $file_name =~ s/.+\\//g;
      $file_name =~ s/.+\///g;
      $file_name =~ s/\./_/g;

      ProcessSingleFile("$input_dir\\$file", "$output_dir\\$file_name\\");
   }
   chdir($currentDir);
}

#----------------------------------------------------------------------------
# ProcessSingleFile( <dlf_file_path>, <output_path> )
#
# Open a log file and:
#  - Write the parsed text file
#  - Export the ENABLED_OUTPUTS to text
#  - If requested, Export the ENABLED_OUTPUTS to Excel
#----------------------------------------------------------------------------
sub ProcessSingleFile($$)
{
   # Create handles for input/output
   my ($logfilepath, $outputpath) = @_;

   print "-" x 60;
   print "\n";

   #----------------------------------------------------------------------------
   #Prepare file names
   #----------------------------------------------------------------------------
   $logfilepath   =~ s/\//\\/g; #Some DOS filesystem commands require backslashes
   if( -f $logfilepath )
   {
      print "Processing File: $logfilepath\n";
   }
   else
   {
      print "ERROR: file \"$logfilepath\" does not exist\n";
      exit(1);
   }

   # destination directory for all outputs
   $outputpath    =~ s/\//\\/g; #Some DOS filesystem commands require backslashes
   print "Output Path: $outputpath\n";

   #----------------------------------------------------------------------------
   # Load DLF file
   #----------------------------------------------------------------------------
   print "\nOpen Log File...";
   if(!$qcat_app->OpenLog($logfilepath))
   {
      print "\nERROR: $qcat_app->{LastError}\n";
      die;
   }
   print "Complete.\n";

   #----------------------------------------------------------------------------
   # Sort the packets
   #----------------------------------------------------------------------------
   if($SORT_BY_TIME)
   {
      print "\nSorting......";
      if(!$qcat_app->SortByTime())
      {
         print "\nERROR: $qcat_app->{LastError}\n";
         die;
      }
      print "Complete.\n";
   }

   #----------------------------------------------------------------------------
   # Save the parsed text file
   #----------------------------------------------------------------------------
   if($DO_SAVE_AS_TEXT)
   {
      print "\nSaving Parsed Text File...";
      if(!$qcat_app->SaveAsText($outputpath))
      {
         print "\nERROR: $qcat_app->{LastError}\n";
         die;
      }
      print "Complete.\n";
   }
   
   print "-" x 60;
   print "\n";
}  #END ProcessSingleFile



