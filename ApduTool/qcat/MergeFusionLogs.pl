#----------------------------------------------------------------------------
# MergeFusionLogs.pl
# 
# Description:
#     * Merges QCAT generated parsed outputs
#
# Copyright (c) 2010-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use Getopt::Long;
use Win32::OLE;
use Win32::Registry;
use File::Basename;

use constant FALSE => 0;
use constant TRUE  => 1;
use constant NULL  => 0;

my $script_path = getRequireScriptsPath($0);
require $script_path."QcatParsedFileObj.pl";

my $mrgExt = "mrg.txt";		# merged file extention
my $help;
my $validApexName = "APEX";
my $validQcatName = "QCAT";
my $outputDir = "C:\\Tmp";		# Default output directory
my @objects;					# object files
my $mergeFile;					# The merged file
my $qcat_app;					# Application to run
my $ts_fmt_option;			# to restore the time stamp format in QCAT/APEX
my $logFiles;					# list of files to merge
my $app_name;					# parsing application
my @log_files;					# list of files to merge
my $timeOffset;				# Fusion timestamps offset in ms
my $MERGE_FH;					#Merged File handle

print "\nRunning merge script......  Please wait.\n";

processCmdOptions();

processLogFiles(); 

mergeParsedOutputs();

finalize();

print ("\nScript is done.\n");

1;

#-------------------------------------------------------------------------
##Generate QCAT ASCII text file to merge with
#-------------------------------------------------------------------------
sub processLogFiles()
{
   my $parsed_output;
   
   ##Get an array of files to merge
   @log_files = split (/\,/, $logFiles);
      
   print ("\nFiles for merging: $logFiles\n");
   print ("\nTime Offset: $timeOffset\n");
      
   if(! scalar @log_files )
   {
	 die "\nError: no files to merge. Please provide and run again.\n";
   }
   
   openMergeFile();
   ##First object has time offset 0, 
   ##the rest of objects will have the same time offset
   my $curTimeOffset = 0;
   foreach my $file (@log_files) 
   {		
       ##Check if file a txt file
	   if($file =~ "\.txt")
	   {
	      $parsed_output = $file;

          if(!( -f $parsed_output))
	      {
	         die "Error: File $parsed_output does not exist. Please provide and try again.\n";
	      }
	   }
	   else ##if it is a dlf or isf file need to run QCAT
	   {
		  initQcat();
	      my $rc = runQcat($file, $parsed_output);	   
	      if($rc)
	      {
		     die "Error: QCAT faild to generate parsed output\n";
	      }
	   }
	   	   
	   createQcatObject($parsed_output, $curTimeOffset );
	   $curTimeOffset = $timeOffset;
   }
}

#-------------------------------------------------------------------------
##Open merge file
#-------------------------------------------------------------------------
sub openMergeFile()
{
	my $name;
	my $name_i;
	my $dir;
	my $ext;
	my $logFile;
	
	foreach my $file (@log_files)
	{
	   ($name_i, $dir, $ext) = fileparse($file,'\..*');
	   $name .= $name_i."_";
	}
	$mergeFile = $outputDir."\\".$name.$mrgExt;	
	
	$MERGE_FH = FileHandle->new();

	open $MERGE_FH, "+>>$mergeFile" or die "\nError: cannot open $mergeFile for writing :$!\n";
	
	#print "\nOpen file for merging $mergeFile\n";
}

#-------------------------------------------------------------------------
##Create qcatObject 
#-------------------------------------------------------------------------
sub createQcatObject()
{
	my $parsed_file = $_[0];
	my $time_offset = $_[1];
	
	print "Creating Object for $parsed_file ........";
	
	my $object = new QcatParsedFileObj;
	$object->setOutput($MERGE_FH);
	$object->setTimeOffset($time_offset);
	my $rc = $object->open($parsed_file);

	if ($rc) 
	{
	  die "Error: Can't open $parsed_file file\n";
	}
	else
	{
		print "completed.\n";
	}

	###Store QCAT object
	push @objects, $object;
}

#-------------------------------------------------------------------------
##Merging parsed output
#-------------------------------------------------------------------------
sub mergeParsedOutputs()
{
	print "Merging........";

	my $more=1;

	while($more) 
	{
		my $i;
		my $file;
		my $mintime;
		my $i_min;
		$more=0;
	        
		$mintime = 24*3600*1000;
		$i_min = -1;
		for($i=0; $i <= $#objects; $i++) 
		{
			$file = @objects[$i];
			if ($file->getTimestamp() != -1) 
			{
				#printf "\nMerge: Object%d packet:%s", $i, $file->getPacket();
				if ($file->getTimestamp() < $mintime) 
				{
					$mintime = $file->getTimestamp();
					$i_min = $i;
				}   
				$more = 1;
			}
		}
		if ($i_min != -1) 
		{
			print $MERGE_FH @objects[$i_min]->getPacket();
			@objects[$i_min]->next();
		}
	}
	print ("completed.\n");
}


#-----------------------------------------------------------------------------
#  getRequireScriptsPath()                                                
#-----------------------------------------------------------------------------
sub getRequireScriptsPath() {   
   my $mergeScriptName = shift;
   my $scriptPath;   
   my @split_path = split (/\\/,$mergeScriptName);
  
   for( my $i= 0; $i < (@split_path)-1; $i++) 
   {
      $scriptPath = $scriptPath.@split_path[$i]."\\\\";
   }
   return $scriptPath;
}

#-----------------------------------------------------------------------------
#  finalize()                                                
#-----------------------------------------------------------------------------
sub finalize()
{  
   foreach my $object (@objects) 
   {
	  $object->closeFile();
	  $object->closeMergeFile();
   }
   
   close $MERGE_FH;
  
   $qcat_app->{TimestampFormat}= $ts_fmt_option;   
   
   $qcat_app = NULL;
}

#-----------------------------------------------------------------------------
#  help                                                 
#-----------------------------------------------------------------------------
sub help()
{
   my $help = <<END_HELP;

MergeFusionLogs.pl [-h] [-app <ARG>] [-outputDir <ARG>] [-timeOff <ARG>] -log <ARG> ... <ARG>

Options:
  -h		 Help
  -app       QCAT or APEX optional, default QCAT 
  -outputDir Output dir optional, default c:\\Tmp.
  -timeOff   Timestamps Offset in ms. Can be positive or negative.
  -logs		 Comma separated list of files to merge

Examples:  
  perl MergeFusionLogs.pl -h
  perl MergeFusionLogs.pl -app APEX -logs c:\\log1.dlf,c:\\log2.dlf
  perl MergeFusionLogs.pl -app APEX -timeOff -45000 -logs c:\\log1.dlf,c:\\log2.dlf
  perl MergeFusionLogs.pl -logs c:\\log1.isf,c:\\log2.isf 
  perl MergeFusionLogs.pl -logs c:\\log1.txt,c:\\log2.txt 

END_HELP
   print "$help\n";
}

#-----------------------------------------------------------------------------
#  processCmdOptions                                                  
#-----------------------------------------------------------------------------
sub processCmdOptions()
{
   ###Process the command line option
   GetOptions(
			   "h"  => \$help,
			  "app=s" => \$app_name,
           "outputDir=s" => \$outputDir,
           "logs=s"  => \$logFiles,
           "timeOff=s" => \$timeOffset);        
   
   ###Run Help
   if ($help)
   {
      help();
      exit;
   }
    
   if(!defined($app_name))
   {
	  $app_name = $validQcatName;
   } 
   
   ### Error message if one of the not optional parametrs is missing  
   if(!defined($logFiles)) 
   {
      print "\nError: No files for merging. Please provide and try again";
      exit;
   }
   
   if(!defined($timeOffset))
   {
	  $timeOffset = 0;
   }   
         
 }
 
#-----------------------------------------------------------------------------
### Instantiate a parsing application object                                                 
#-----------------------------------------------------------------------------
 sub initQcat()
 {
   if(!defined($app_name) || $app_name =~ /^\s*$validQcatName\s*$/i)
   {
	  $qcat_app  = new Win32::OLE 'QCAT5.Application';
	
	   if(!$qcat_app)   
	   {
		   print "Error: Unable to invoke QCAT. Please Install.\n";
	   }
	}

	if ($app_name =~ /^\s*$validApexName\s*$/i) 
	{
		$qcat_app = new Win32::OLE 'APEX5.Application';
		
		if(!$qcat_app)   
		{
		  #Invoke a different one before die
		  $qcat_app = new Win32::OLE 'QCAT5.Application';
		  die "ERROR: Unable to invoke APEX or QCAT. Please Install.\n";
		 }
	}
   
   ###Retrieve Options.  
   $ts_fmt_option = $qcat_app->{TimestampFormat};
   
   ###Set the Options to new values.   
   $qcat_app->{TimestampFormat}= "Calendar";
   
   ###Retrieve Application Version and print it  
   print "\n$app_name Version: $qcat_app->{AppVersion}\n";
   
   ###Make an Application Window Visible.  
   $qcat_app->{Visible} = FALSE;
}
 

#----------------------------------------------------------------------------
# runQcat
# Run application to generate parsed  outputs
#----------------------------------------------------------------------------
sub runQcat()
{ 
   my $LOG_FILE  = $_[0];
   my $filepath  = Win32::GetFullPathName($LOG_FILE);
   my @pathsplit = split (/\./,$filepath);
   my $TXT_FILE  = "$pathsplit[0].txt";
   
   ## Sets the $parsedFile path 
   $_[1] = $TXT_FILE;
      
   ###Check that input file exists  
   if( not -e $LOG_FILE )   
   {
      die "Error: this Script requires the exsistence of a log file: $LOG_FILE\n";
   }
   
   ###Parse Log File.   
   print "Parsing $LOG_FILE........";
   if(!$qcat_app->Process($LOG_FILE, $TXT_FILE, FALSE, FALSE))   
   {
      print "Error: $qcat_app->{LastError}\n";
      return 1;
   }
   else   
   {
      print "completed.\n";
      return 0;
   }   
}
