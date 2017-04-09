#----------------------------------------------------------------------------
# APEX 6.x MergeWithWireshark.pl
# 
# Description:
#     * Creates a QCAT Automation Object and generates QCAT ASCII text file
#     * Runs PCAPGenerator and generates MS pcap files 
#     * Runs tshark/Tethereal on generated MS and provided BS pcap files 
#     * Merges QCAT ASCII with generated tshark/Tethereal txt files 
#
# Copyright (c) 2007-2017 Qualcomm Technologies,Inc Proprietary
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

require $script_path."qcatobj.pl";
require $script_path."tsharkobj.pl";
require $script_path."tsharkverboseobj.pl";

##An array of intermediate files for delition
my @unlink_files;

##Defaults 
my $q = "\""; 
my $tmp = "\\tmp\\";
my $mergExt = "_merge.txt";
my $use_enhancement = 1;

##Script parameters/options
my $help;
my $verbose;
my $logFile;
my $pcapFiles;
my $runPcapGen;
my $application;

##Default settings for optional parametrs 
my $Timeloc = "Local";
my $Timeoffset = 0;
my $outputDir = "c:\\Tmp\\";
my $validApexName = "APEX";
my $validQcatName = "QCAT";
my $rc; # return code


my @fileobjs;  #tshark/Tethereal text files as objects
my $qc;        #QCAT text file as an object  
my $Qcatf;     #QCAT ASCII text file to merge with
my $Mergef;    #Final file
my @file_list; # An array of PCAP files to run with Tthereal

my $rc;

processCmdOptions();

if($logFile =~ "\.txt"){
   $Qcatf = $logFile;
}
else {
   ##Generate QCAT ASCII text file to merge with
   runQcat($Timeloc, $logFile, $Qcatf);
}

push @unlink_files, $Qcatf;

##Open merge file
my $name;
my $dir;
my $ext;
($name,$dir,$ext) = fileparse($logFile,'\..*');
$Mergef = $outputDir."\\".$name.$mergExt;

#print"\n**** Merge file to be about generated: $Mergef\n";

open MERGE_FH, "+>>$Mergef" or die "Error: cannot open $Mergef for writing :$!";

##Create Qcatobj 
$qc = new Qcatobj;
$qc->set_output($Mergef);
$qc->set_enh($use_enhancement);
$rc=$qc->open($Qcatf);

if ($rc) {
  die "\nError: Can't open $Qcatf file\n";
}

push @fileobjs, $qc;
   
##Make a temp directory for the intermediate files
my $tmpDir = $outputDir.$tmp;
mkdir $tmpDir;


##Generate an array of pcap files to process   
if ($runPcapGen) {  
   $rc = runPcapGenerator($logFile,$tmpDir);      
   my @pcap_files = getListPcapGenFiles($tmpDir);
   my @out_files;
      
   foreach my $file (@pcap_files) {   
      my $outFile = cleanFileName($file);                  
      $rc = runTShark($file,$outFile);       
      createTSharkObject("MS",0,$outFile);
      
      push @unlink_files, $file;
      push @unlink_files, $outFile;
      push @out_files,$outFile;      
   } 
   #print "\n****  MS pcap files to merge @out_files\n" 
}

if($pcapFiles)
{ 
   ##Get an array of pcap files to process
   my @pcap_files = split (/\,/, $pcapFiles);
   
   if(! scalar @pcap_files )
   {
	 die "\nError: no PCAP/PPP files were provided to run by tshark/Tethereal. Please provide and run again.\n";
   }
   
   #print "\n++++ Pcap files BS @pcap_files\n";
   
   my @out_files;
   
   foreach my $file (@pcap_files) {
   
      ($name,$dir,$ext) = fileparse($file,'\..*');
      my $outFile = $tmpDir.$name."\.txt";
            
      $rc = runTShark($file,$outFile);
            
      createTSharkObject("BS",$Timeoffset,$outFile);
      
      push @unlink_files, $outFile;
      push @out_files, $outFile;      
   }
   #print "\n****  BS pcap files to merge @out_files\n";    
}

my $more=1;
while($more) {
    my $i;
    my $file;
    my $mintime;
    my $i_min;
    $more=0;
        
    $mintime = 24*3600*1000;
    $i_min = -1;
    for($i=0;$i<=$#fileobjs;$i++) {
        $file = @fileobjs[$i];
        if ($file->get_timestamp() != -1) {
            #printf "%d mintime:%d stamp:%d\n", $i, $mintime, $file->get_timestamp();
            if ($file->get_timestamp() < $mintime) {
                $mintime = $file->get_timestamp();
                $i_min = $i;
            }   
            $more = 1;
        }
    }
    #printf "i_min: %d\n", $i_min;
    if ($i_min != -1) {
        print MERGE_FH @fileobjs[$i_min]->get_packet();
        @fileobjs[$i_min]->next();
    }
}

print ("\n****  Script is done!!!\n");

finalize();

1;

#=============================================================================
#  getRequireScriptsPath()                                                
#=============================================================================
sub getRequireScriptsPath() {   
   my $mergeScriptName = shift;
   my $scriptPath;   
   my @split_path = split (/\\/,$mergeScriptName);
  
   for( my $i= 0; $i < (@split_path)-1; $i++) {
      $scriptPath = $scriptPath.@split_path[$i]."\\\\";
   }
   return $scriptPath;
}

#=============================================================================
#  finalize()                                                
#=============================================================================
sub finalize()
{
   $qc->close_fh();
   close MERGE_FH;
   
   #unless ((my $count = unlink(@unlink_files)) == @unlink_files) {
   #   print "\n++++ Unlink: could only delete $count files\n";
   #}
}

#=============================================================================
#  createTSharkObject()                                                
#=============================================================================
sub createTSharkObject()
{
   my $source = shift;
   my $Timeoffset = shift;
   my $file = shift; 

   my $rc;

   #verbose version tshark/Tethereal obj
   if($verbose){ 
    my $t = new tsharkverboseobj;
    $t->set_timeOffset($Timeoffset);
    $t->set_source($source);
    $rc = $t->open($file);
    push @fileobjs, $t;       
   }
   #Non verbose version tshark/Tethereal obj
   else {    
    my $t = new tsharkobj;
    $t->set_timeOffset($Timeoffset); 
    $t->set_source($source);      
    $rc = $t->open($file);
    push @fileobjs, $t;
   }
      
   if ($rc) {
    die "Error: Can't open $file file\n";
   }
      
}

#=============================================================================
#  cleanFileName                                                
#=============================================================================
sub cleanFileName()
{
   my $file = shift;
   
   my $name;
   my $dir;   
   my $ext;
   
  ($name,$dir,$ext) = fileparse($file,'\..*');
    
   #clean file name from white spaces, and dots
   $name =~ s/\s+/_/g;
   $name =~ s/\./_/g;
 
   #print "*****Input file: $file \n";
    
   my $path = $dir.$name."\.txt";
   
   #print "*****Output file: $path \n";
    
   return $path;
}

#=============================================================================
#  getListPcapGenFiles                                                
#=============================================================================
sub getListPcapGenFiles()
{
   my $tmpDir = shift;
   my @list;
   my $file;
   
   opendir (DIR, $tmpDir) or die $!;
   
   while (defined($file=readdir(DIR)))
   {
      next if $file =~ /^\.\.?$/;
      next if $file =~ /\.txt/;
      next if $file =~ /\.dlf/;
      next if $file =~ /\.isf/;
      if ($file =~ /\.log/) { 
         push @unlink_files, ($tmpDir.$file);
         next;
      }        
             
     push @list,($tmpDir.$file);     
   }
   close(DIR);
   
   if(! scalar @list )
   {
		die "\nWarning: no PCAP/PPP files were generated to run by tshark/Tethereal.\n";
   }
   else
   {   
		print "\n**** Generated files to run by tshark/Tethereal: @list\n";
   } 
     
   return @list;
}

#=============================================================================
#  runTShark                                                 
#=============================================================================
sub runTShark()
{
   my $inFile = shift;
   my $outFile = shift;
   my $tApp;
   my $Params;
  
  if( checkValueFromRegistry(
  		"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Wireshark", 
  		"UninstallString"
  	  )
	)
  { 
  	#Wireshark found, run tshark
   	$tApp =  getValueFromRegistry("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Wireshark", "UninstallString");
	$tApp =~ s/uninstall.exe/tshark.exe/;   	
  }
  else # Wireshark not found, run tethereal (ethereal)
  {
      $tApp =  getValueFromRegistry("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Ethereal", "UninstallString");
      $tApp =~ s/uninstall.exe/tethereal.exe/;
	  print ("\n******* Migration to Wireshark suggested: Please install Wireshark and uninstall Ethereal. *******");      
  }
   
   $Params = "-r \"$inFile\"";
      
   if($verbose){ 
      $Params .= " -V";		## verbose
   }

   $Params .= " -t ad"; ##Time stamps in Calendar format 
      
   print ("\n=======$tApp $Params\n");

   my $rc = system("$tApp $Params > \"$outFile\"");
   
   if($rc)#Error executing tShark/Tethereal
   {
		die "\n======Error executing tShark/Tethereal rc = $rc \n";
   }
    
   return $rc;    
}


#=============================================================================
#  runPcapGenerator                                                 
#=============================================================================
sub runPcapGenerator()
{
   my $path = shift;
   my $tmpDir = shift;
   
   my $name;
   my $dir;
   my $ext;
   
  ($name,$dir,$ext) = fileparse($path,'\..*');

   my $PCAPgen;
   my $PCAPgenParam;   
  
   $PCAPgen=getValueFromRegistry("Software\\Qualcomm\\Tools", "PCAPGenerator");

   $PCAPgenParam = "\"".$logFile."\" \"".$tmpDir."$name" ."\"";

   if($ext =~ "\.dlf") {
      $PCAPgenParam = $PCAPgenParam." -d";## Force to parse dlf, default isf
   }
        
   print ("\n========$PCAPgen $PCAPgenParam\n\n");

   return system("\"$PCAPgen\"" ." " .$PCAPgenParam);
}

#=============================================================================
#  Get value from the Registry                                                 
#=============================================================================
sub getValueFromRegistry()
{
   my ($reg, $askkey) = @_;
   my $key;
   my $value;
   my %values;
   my $regkey;
   my $regvalue;

   if(0 == $HKEY_LOCAL_MACHINE->Open($reg, $key))
   {
   		print ("\nError: Couldn't locate registry entry ($reg\\$askkey)");
		die $!;   
   }

   $key->GetValues(\%values);
   foreach $value (keys(%values))
   {
      $regkey = $values{$value}->[0];
      $regvalue = $values{$value}->[2];
      if ($regkey eq $askkey)
      {
         $key->Close();
         return $regvalue;
      }                
   }
   $key->Close();
}

#=============================================================================
#  check value from the Registry
#=============================================================================
sub checkValueFromRegistry()
{
  my ($reg, $askkey) = @_;
   my $key;
   my $value;
   my %values;
   my $regkey;
   my $regvalue;

   my $keyFound = $HKEY_LOCAL_MACHINE->Open($reg, $key);
   if($keyFound)
   {
	   $key->GetValues(\%values);
	   foreach $value (keys(%values))
	   {
	      $regkey = $values{$value}->[0];
	      $regvalue = $values{$value}->[2];
	      if ($regkey eq $askkey)
	      {
	         $key->Close();
	         return 1;
	      }                
	   }
   	   $key->Close();	   
   }
   return 0;
}

#=============================================================================
#  help                                                 
#=============================================================================
sub help()
{
   my $help = <<END_HELP;

merge.pl [--h] [--V] [--time <ARG>] [--offset <ARG>] [--outputDir <ARG>] [--logFile <ARG>] [--pcapFiles <ARG>]  [--pgm <ARG>]

Options:
  --h   : Help
  --V   : tshark/Tethereal txt in Verbose mode, default summary mode.
  --time       <Time location Local/UTC>, default Local.
  --offset     <Time offset in ms>, default 0.
  --outputDir  <Output directory for a merged file>, default c:\\Tmp.
  --logFile    <Full path of dlf/isf/txt >, if txt then  --pcapFiles must.  
  --pcapFiles  <List comma separated full pathes of ppp/pcap files >
  --pgm        <APEX/QCAT> programm to parse isf/dlf input files.

Examples:  
  perl merge.pl --h
  perl merge.pl --logFile c:\\log.dlf  --time UTC
  perl merge.pl --logFile c:\\log.isf --pgm APEX --V
  perl merge.pl --logFile c:\\parsed.txt --pcapFiles "c:\\binary1.pcap,c:\\binary2.pcap"

END_HELP
   print "$help\n";
}

#=============================================================================
#  processCmdOptions                                                  
#=============================================================================
sub processCmdOptions()
{
   # Process the command line option
   GetOptions("h"           => \$help,
              "V"           => \$verbose,
              "time=s"      => \$Timeloc,
              "offset=s"    => \$Timeoffset,
              "outputDir=s" => \$outputDir,
              "logFile=s"   => \$logFile,
              "pcapFiles=s" => \$pcapFiles,
              "pgm=s"		=> \$application);        
   
   if ($help){
      help();
      exit;
   }
   
   if(defined($pcapFiles)){
	$runPcapGen = FALSE;
   }
   else {
      $runPcapGen = TRUE;
   }

   
   if(!defined($application)){
		$application = $validQcatName;
   } 
     
   if( defined($logFile) || defined($pcapFiles) ) {
      print "\n****  Merging ......  Please wait.  ****\n\n"; 
   } 
   else {
      print "\nError: No text files to merge were provided or generated";
      exit;
   }  
   
   if(defined($pcapFiles)) {
      $use_enhancement = 0; 
   }
      
 }

#----------------------------------------------------------------------------
# runQcat
#----------------------------------------------------------------------------
sub runQcat()
{   
   my $HEX_DUMP = FALSE;
   my $USE_3X_NAMES 	= FALSE;   
   
   my $time_fmt = $_[0];
   my $LOG_FILE  = $_[1];
   my $filepath  = Win32::GetFullPathName($LOG_FILE);
   my @pathsplit = split (/\./,$filepath);
   my $TXT_FILE  = "$pathsplit[0].txt";
   $_[2] = $TXT_FILE;
   
   # Check that input file exists  
   if( not -e $LOG_FILE )   {
      die "Error: this Script requires the exsistence of a log file: $LOG_FILE\n";
   }

   #----------------------------------------------------------------------------
   # Run QCAT/APEX Application
   #----------------------------------------------------------------------------
   my $qcat_app;
   
   if ($application =~ /^\s*$validQcatName\s*$/i) 
   {	
	$qcat_app = new Win32::OLE 'QCAT6.Application';
	
	
	if(!$qcat_app)   
	{
	  print "ERROR: Unable to invoke $application. Please Install.\n";
	  
	  #Invoke a different one before die
	  $qcat_app = new Win32::OLE 'APEX6.Application';
      die "ERROR: Unable to invoke $application or APEX. Please Install.\n";
     }
   }

   if ($application =~ /^\s*$validApexName\s*$/i) 
   {
   		$qcat_app = new Win32::OLE 'APEX6.Application';
		
		
		if(!$qcat_app)   
		{
		  print "ERROR: Unable to invoke $application. Please Install.\n";
		  
		  #Invoke a different one before die
		  $qcat_app = new Win32::OLE 'QCAT6.Application';
		  die "ERROR: Unable to invoke $application or QCAT. Please Install.\n";
		 }
   }	

   
   # Retrieve the Options.  
   my $ts_fmt_option = $qcat_app->{TimestampFormat};
   my $ts_loc_option = $qcat_app->{TimestampLocale};
   
   # Set the Options to new values.   
   $qcat_app->{TimestampFormat}= "Calendar";
   $qcat_app->{TimestampLocale}= $_[0];
   
   # Retrieve the Application Version and print it  
   print "\n=======$application Version: $qcat_app->{AppVersion}\n";
   
   # Make the Application Window Visible.  
   $qcat_app->{Visible} = FALSE;

   #----------------------------------------------------------------------------
   # Process the Log File.
   #----------------------------------------------------------------------------
   print "\nParsing ...";
   if(!$qcat_app->Process($LOG_FILE, $TXT_FILE, $HEX_DUMP, $USE_3X_NAMES))   {
      print "Error: $qcat_app->{LastError}\n";
    }
   else   {
      print "Complete!!\n";
   }
      
   # Restore QCAT Options   
   $qcat_app->{TimestampFormat}= $ts_fmt_option;
   $qcat_app->{TimestampLocale}= $ts_loc_option;
   
   # Clean-up
   $qcat_app = NULL;
}







