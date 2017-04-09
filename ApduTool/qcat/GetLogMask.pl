#----------------------------------------------------------------------------
#  GetLogMask
#
#  Process all log files in the directory and reports what log mask they use
#  
# Copyright (c) 2015-2017 Qualcomm Technologies,Inc Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#----------------------------------------------------------------------------
use strict;
use Win32::OLE;
use Cwd;
use constant FALSE   => 0;
use constant TRUE    => 1;
use constant NULL    => 0;
use constant ENABLE  => 1;
use constant DISABLE => 0;


# Which analyses should be exported?
my @ENABLED_OUTPUTS  = (";Common Displays;Log Mask Selection");

#----------------------------------------------------------------------------
# QCAT Application
#----------------------------------------------------------------------------
my $qcat_app = NULL;

InitQCAT();

my $workspace = $qcat_app->{Workspace};         #get the workspace object
$workspace->SelectOutput(";", DISABLE);         #deselect all (";" is the root -- so this deselects the root)
foreach my $output ( @ENABLED_OUTPUTS ) 
{
   $workspace->SelectOutput($output, ENABLE);   #select all outputs in the ENABLED_OUTPUTS array
}

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
         ProcessExportSingleFile($file, getcwd());
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

$workspace = NULL;
sleep(1);
$qcat_app = NULL;


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

   #----------------------------------------------------------------------------
   # Set QCAT settings
   #----------------------------------------------------------------------------
   $qcat_app->{Visible}          = FALSE;
   $qcat_app->{PrefixLogName}    = TRUE;
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

   my @files = (<*.dlf>, <*.isf>, <*.qmdl>, <*.qmdl2>);
   foreach my $file (@files)
   {
      my $file_name = $file;
      $file_name =~ s/.+\\//g;
      $file_name =~ s/.+\///g;
      $file_name =~ s/\./_/g;

      ProcessExportSingleFile("$input_dir\\$file", "$output_dir\\");
   }
   chdir($currentDir);
}

#----------------------------------------------------------------------------
# ProcessExportSingleFile( <dlf_file_path>, <output_path> )
#
#----------------------------------------------------------------------------
sub ProcessExportSingleFile($$)
{
   # Create handles for input/output
   my ($logfilepath, $outputpath) = @_;

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

   if(!$qcat_app->OpenLog($logfilepath))
   {
      print "\nERROR: $qcat_app->{LastError}\n";
      die;
   }   

   if(!$workspace->ExportToText($outputpath, "\t"))
   {
      print "\nERROR: $qcat_app->{LastError}\n";
   }

}



