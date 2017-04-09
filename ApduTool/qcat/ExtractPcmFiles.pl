#----------------------------------------------------------------------------
#    Q C A T6   A u t o m a t i o n   I n t e r f a c e   S c r i p t
# 
# Description:
#    This script demonstrates the following:
#
#    * Create a QCAT6 Automation Object
#    * Process a Log File
#    * Extract PCM Audio files
#    * Generate Vocoder PCM Audio RAW / WAV files
#    * Release the QCAT Automation Object
#
# Copyright (c) 2011-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
use strict;
use Cwd;
use File::Basename;
use Getopt::Long;
use Win32::OLE;
use constant FALSE => 0;
use constant TRUE  => 1;

#----------------------------------------------------------------------------
# Options
#----------------------------------------------------------------------------
my $logFile;
my $outDir;
my $codec;
my $codec_name;
my $bReverse;
my $bReplace;
my $bRaw;
my $bWav;
my $help;
my $name;
my $dir;
my $ext; 
my $qcat_app;
my $validQcatName = "QCAT";

#Constants for selecting the vocoder c-sim
use constant AUTO_MODE => 0; #requires 0x7143/0x7144/0x14D0/0x14D2/0x1804/0x1805/0x1914/0x1915 vocoder packets
use constant AMR_MODE => 1;
use constant EFR_MODE => 2;
use constant FR_MODE => 3;
use constant HR_MODE => 4;
use constant EVRC_MODE => 5;
use constant V13K_MODE => 6;
use constant AMR_WB_MODE => 7;
use constant EVRC_B_MODE => 8;
use constant EVRC_WB_MODE => 9;
use constant EVRC_NW_MODE => 10;
use constant EAMR_MODE => 11;
use constant EVRC_NW2K_MODE => 12;
use constant EVS_MODE => 13;
use constant G711_MODE => 14;

#Codec Names
my %CODEC_NAMES = (
	"auto" => AUTO_MODE,
	"celp13k" => V13K_MODE,
	"evrc" => EVRC_MODE,
	"evrc-b" => EVRC_B_MODE,
	"evrc-wb" => EVRC_WB_MODE,
	"hr" => HR_MODE,
	"fr" => FR_MODE,
	"efr" => EFR_MODE,
	"amr-nb" => AMR_MODE,
	"amr-wb" => AMR_WB_MODE,
        "evrc-nw" => EVRC_NW_MODE,
        "eamr" => EAMR_MODE,
        "evrc-nw2k" => EVRC_NW2K_MODE,
        "evs" => EVS_MODE,
        "g711" => G711_MODE
);

#----------------------------------------------------------------------------
# Print the quick help
#----------------------------------------------------------------------------
sub printUsage
{
print<<EOM

Usage: perl ExtractPcmFiles.pl -in <file> [-out <dir>] [-codec <codec>] [-reverse <0/1>] [-replace <0/1>] [-raw <0/1>] [-wav <0/1>]

Options:

-in\tthe input log file (full-path) either ISF, DLF, or QMDL

-out\tthe output directory (full-path) 

	directory will be created if it does not exist.

        default is the current directory
	
-codec\tthe vocoder to use:

	auto	  -- determine vocoder from log packets (0x7143/0x7144/0x14D2/0x1804/0x1805 only)
	celp13k   -- Code Excited Linear Prediction 13kbps
	evrc	  -- Enhanced Variable Rate CODEC
	evrc-b	  -- Enhanced Variable Rate CODEC B
	evrc-wb   -- Enhanced Variable Rate CODEC Wide-Band
	hr	  -- GSM Half-Rate
	fr	  -- GSM Full-Rate
	efr	  -- GSM Enhanced Full-Rate
	amr-nb    -- Adaptive Multi-Rate (Narrow Band)
	amr-wb	  -- Adaptive Multi-Rate (Wide-Band)
        evrc-nw   -- Enhanced Variable Rate Codec, Service Option 73        
        eamr	  -- Enhanced AMR
        evrc-nw2k -- Enhanced Variable Rate Codec, Service Option 77
        evs       -- Enhanced Voice Service
        g711      -- G711 Service

        default is \"auto\"

-reverse Vocoder UI "Reverse Byte Order" option (default is 0)

-replace Vocoder UI "Replace Dropped Frames" option (default is 0)

-raw\tVocoder UI "RAW O/P" option (default is 1)

-wav\tVocoder UI "WAV O/P" option (default is 1)

EOM
;
}

# Process the command line option
GetOptions( "h"         => \$help,             
            "out=s"	=> \$outDir,
            "in=s"      => \$logFile,
            "log=s"     => \$logFile,
            "codec=s"   => \$codec_name,
            "reverse=s" => \$bReverse,
            "replace=s" => \$bReplace,
            "raw=s"     => \$bRaw,
            "wav=s"     => \$bWav); 
              
#----------------------------------------------------------------------------
# Check that input file exists
#----------------------------------------------------------------------------
if(not -e $logFile )
{
   print "\nThis Script requires existing log file $logFile\n";
   printUsage();
   exit;
}

if(!defined($qcat_app))
{
   $qcat_app = $validQcatName;
} 

if(!defined($codec_name))
{
   $codec_name = "auto";
} 
#Convert the codec name to an enum value
$codec = $CODEC_NAMES{lc($codec_name)};

#handle options
if(!defined $bReverse)
{
   $bReverse = FALSE;
}
elsif($bReverse == 1 || lc($bReverse) eq "true")
{
   $bReverse = TRUE;
}
else
{
   $bReverse = FALSE;
}

if(!defined $bReplace)
{
   $bReplace = FALSE;
}
elsif($bReplace == 1 || lc($bReplace) eq "true")
{
   $bReplace = TRUE;
}
else
{
   $bReplace = FALSE;
}

if(!defined $bRaw)
{
   $bRaw = TRUE;
}
elsif($bRaw == 0 || lc($bRaw) eq "false")
{
   $bRaw = FALSE;
}
else
{
   $bRaw = TRUE;
}

if(!defined $bWav)
{
   $bWav = TRUE;
}
elsif($bWav == 0 || lc($bWav) eq "false")
{
   $bWav = FALSE;
}
else
{
   $bWav = TRUE;
}

#Default the output directory to the current path if not defined
if(!defined $outDir)
{
   $outDir = getcwd();
}

$outDir = _dirFormatValidate($outDir);

#----------------------------------------------------------------------------
# Start-up the QCAT Application
#----------------------------------------------------------------------------
print "\nInvoking QCAT6.Application\n";

$qcat_app = new Win32::OLE 'QCAT6.Application';
if(!$qcat_app)
{
   print "ERROR: Unable invoke QCAT application.\n";
   die;
}

#----------------------------------------------------------------------------
# Retrieve the QCAT Version and print it
#----------------------------------------------------------------------------
my ($maj, $min, $patch0, $patch1, $patchStr);

my $version = $qcat_app->{AppVersion};
if($version =~ /patch/) {#06.30.38 patch 31
   ($maj, $min, $patch0, $patchStr, $patch1) = split(/\.|\s+/, $version);
}
else#06.30.39.00
{
   ($maj, $min, $patch0, $patch1) = split(/\./, $version);
}
print "QCAT Version: $qcat_app->{AppVersion}\n";

#"Reverse Byte Order", "Replace Dropped Frames", "Raw O/P", & "WAV O/P"
#options are supported on & after QCAT 06.30.38.31 
if($maj < 6 || $min < 30 || $patch0 < 38 || ($patch0 == 38 && $patch1 < 31))
{
   print "Please Install QCAT 06.30.38.31 or greater\n";
   exit;
}

#----------------------------------------------------------------------------
# Open the Log File.
#----------------------------------------------------------------------------
print "\nOpening log file $logFile. Please wait ....\n";
if(!$qcat_app->OpenLog($logFile))   
{
   print "Error: $qcat_app->{LastError}\n";
   die;
}
print "Complete.\n";

#----------------------------------------------------------------------------
# Generate Vocoder PCM Audio TX/RX raw files from the processed LOG File
#----------------------------------------------------------------------------
#GenerateVocoderPCM API will create the specified Tx/Rx directories if they don't exist
my $txOutDir = $outDir;
my $rxOutDir = $outDir;

print "\nGenerating Vocoder PCM Audio files at $outDir. Please wait... \n";
if(!$qcat_app->GenerateVocoderPCM($txOutDir, $rxOutDir, $codec, $bReverse, $bReplace, $bRaw, $bWav))
{
   print "Error: $qcat_app->{LastError}\n";
   die;
}
print "Complete.\n";

#----------------------------------------------------------------------------
# Extract PCM Audio TX/RX raw files from the processed LOG File
#----------------------------------------------------------------------------
#ExtractPcmAudio API will create the specified Tx/Rx directories if they don't exist
print "\nExtracting PCM Audio files at $outDir. Please wait .... \n";
if(!$qcat_app->ExtractPcmAudio($txOutDir, $rxOutDir, $bReverse, $bReplace, $bRaw, $bWav))
{
   print "Error: $qcat_app->{LastError}\n";
   die;
}
print "Complete.\n";

#=========================================================================
#  SYNOPSIS 
#     _dirFormatValidate($dir)
#
#  DESCRIPTION
#     Internal sub used to make sure desired directory have right format
#     (no new line / white space in the leading / trailing and with '\' 
#      at end).  
#  
#  INPUT PARAMETERS
#     $dir: directory specified 
#
#  RETURN VALUES
#     $dir - desired directory format 
#     
#=========================================================================
sub _dirFormatValidate
{
   my $dir = shift;

   # eliminate any possible leading/trailing new line
   $dir =~ s/^\n*(.*?)\n*$/$1/;

   # eliminate any possible leading/trailing white space
   $dir =~ s/^\s*(.*?)\s*$/$1/;

   # eliminate any possible trailing slash(es)
   $dir =~ s/(.*?)\\*$/$1/;
   
   # make sure root has '\' at end
   if ($dir !~ /\\$/) {
      $dir .= '\\';
   }

   return $dir;
}

1;

