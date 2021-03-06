#----------------------------------------------------------------------------
# ExtractVocoderData.pl
#
# Requirements:
#	TEMP environment variable must be set correctly
# 
# Description:
#	1) Loads the log file specified by $LOG_FILE
#	2) Extracts the vocoder frame files (may be more than one per stream)
#		to the directory specified by $OUTPUT_DIRECTORY
#	
# Copyright (c) 2010-2017 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#----------------------------------------------------------------------------
use strict;
use Cwd;
use Getopt::Long;  # for GetOptions
use Win32::OLE;
use constant FALSE => 0;
use constant TRUE  => 1;

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

# Options
my $bHELP = FALSE;

# Specify the codec to use to convert vocoder frames to PCM
my $CODEC_NAME;

# The input log file (.isf, .dlf, .dbg, or .qmdl)
my $LOG_FILE;

# The output directory
my $OUTPUT_DIRECTORY;

#options
my $bReverse;
my $bReplace;
my $bRaw;
my $bWav;

GetOptions(
   "h"         => \$bHELP,
   "help"      => \$bHELP,
   "codec=s"   => \$CODEC_NAME,
   "in=s"      => \$LOG_FILE,
   "out=s"     => \$OUTPUT_DIRECTORY,
   "reverse=s" => \$bReverse,
   "replace=s" => \$bReplace,
   "raw=s"     => \$bRaw,
   "wav=s"     => \$bWav
);

# Print the quick help
if($bHELP)
{
   printUsage();
   exit(0);
}

# Check that input file exists
if(not -e $LOG_FILE )
{
   printUsage();
   exit(1);
}

#Convert the codec name to an enum value
if(not defined $CODEC_NAME)
{
   $CODEC_NAME = "auto";
}
my $CODEC = $CODEC_NAMES{lc($CODEC_NAME)};

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
if(not defined $OUTPUT_DIRECTORY)
{
   $OUTPUT_DIRECTORY = getcwd();
}

$OUTPUT_DIRECTORY = _dirFormatValidate($OUTPUT_DIRECTORY);

# Start-up the QCAT Application
my $qcat_app = new Win32::OLE 'QCAT6.Application';
if(!$qcat_app)
{
   print "ERROR: Unable to invoke the QCAT application.\n";
   die;
}

# Retrieve the QCAT Version and print it
my ($maj, $min, $patch0, $patch1, $patchStr);

my $version = $qcat_app->{AppVersion};
if($version =~ /patch/) {#06.30.38 patch 31
   ($maj, $min, $patch0, $patchStr, $patch1) = split(/\.|\s+/, $version);
}
else#06.30.39.00
{
   ($maj, $min, $patch0, $patch1) = split(/\./, $version);
}
print "\nQCAT Version: $qcat_app->{AppVersion}\n\n";

#"Reverse Byte Order", "Replace Dropped Frames", "Raw O/P", & "WAV O/P"
#options are supported on & after QCAT 06.30.38.31 
if($maj < 6 || $min < 30 || $patch0 < 38 || ($patch0 == 38 && $patch1 < 31))
{
   print "Please Install QCAT 06.30.38.31 or greater\n";
   exit;
}

# Open the Log File.
print "Opening log file...";
if(!$qcat_app->OpenLog($LOG_FILE))
{
   print "\nERROR: $qcat_app->{LastError}\n";
   die;
}
print "Complete.\n";

#----------------------------------------------------------------------------
# ExtractVocoderDataFiles(
#    $OUTPUT_DIRECTORY, #-out option for "Output folder" & default to the current path
#    $bReverse, #-reverse option for "Reverse Byte Order" & default to FALSE (LITTLE_ENDIAN)
#    codec, #-codec option & default to "auto" 
#    $bReplace, #-replace option for "Replace Dropped Frames" & default to FALSE
#    $bRaw, #-raw option for "Raw O/P" & default to TRUE
#    $bWav, #-wav option for "WAV O/P" & default to TRUE
# )
#
# Extracts the PCM samples into files (one each for rx and tx)
# Extracts the vocoder frames into files (1 or more for each of rx and tx)
# Converts vocoder to PCM Audio using the selected codec
#----------------------------------------------------------------------------
print "Extracting Vocoder Data Files...";
if(!$qcat_app->ExtractVocoderDataFiles($OUTPUT_DIRECTORY, $bReverse, $CODEC, $bReplace, $bRaw, $bWav))
{
   print "\nERROR: $qcat_app->{LastError}\n";
}
else
{
   print "Complete.\n";
}

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

#----------------------------------------------------------------------------
# Print the quick help
#----------------------------------------------------------------------------
sub printUsage
{
print<<EOM

Usage: perl ExtractVocoderData.pl -in <file> [-out <dir>] [-codec <codec>] [-reverse <0/1>] [-replace <0/1>] [-raw <0/1>] [-wav <0/1>]

Options:

-in\tthe input log file (full-path) either ISF, DLF, or QMDL

-out\tthe output directory (full-path) 

	directory will be created if it does not exist

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
