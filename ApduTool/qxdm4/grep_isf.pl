###########################################################################################
#
# grep_isf.pl
#
# Command line grep utility for F3 messages in ISF file.
#
# The tool takes an ISF file and search string as parameters and prints all the F3 messages
# which contain the specified string. 
# It uses the built in QXDM "Match Item" utilty. It supports PERL style regular expressions
#
# NOTE: This script must be run with Perl in a command box,
# i.e.  Perl grep_isf.pl <Input_ISF_File.isf> <string_to_be_searched>
#
# Revision History
# --------------------------------------------------------
# 08/10/2011    ld   Initial revision
#
# $Header: //depot/DM/dev/qxdm-qt/LegacyAutomationSamples/grep_isf.pl#1 $
#
# Copyright (c) 2011 by QUALCOMM, Incorporated.  All Rights Reserved.
############################################################################################
#!/usr/bin/perl -w

use strict;
use File::Spec;
use Win32::OLE;
use File::Basename;
use Cwd 'abs_path';
use HelperFunctions4;


# Constants
use constant false     => 0;
use constant true      => 1;

# Global variables
my $QXDM               = 0;
my $SearchString       = "";
my $ISFAbsolutePath    = "";

# Process the argument - ISF file name
sub ParseArguments
{
   # Assume failure
   my $RC = false;
   my $Txt = "";
   my $Help =
      "Syntax: Perl grep_isf.pl <Input_ISF_File.isf> <string_to_be_searched> \n"
    . "Eg:     Perl grep_isf.pl s1.isf \"MC_RESET|jammer in max bin\" \n";

   if ($#ARGV < 0)
   {
      print "\n$Help\n";
      return $RC;
   }

   my $ISFFileName = $ARGV[0];
   if ($ISFFileName eq "")
   {
      $Txt = "Invalid ISF file name\n\n" . $Help;
      print "\n$Txt";

      return $RC;
   }

   $ISFAbsolutePath = GetPathFromScript();
   if ($ISFAbsolutePath eq "")
   {
      $Txt = "Invalid ISF file name\n\n" . $Help;
      print "\n$Txt";

      return $RC;
   }
   else
   {
      $ISFAbsolutePath .= $ISFFileName;
   }

   $SearchString = $ARGV[1];
   if ($SearchString eq "")
   {
      $Txt = "Invalid Search String\n\n" . $Help;
      print "\n$Txt";

      return $RC;
   }

   # Success
   $RC = true;
   return $RC;
}

# Initialize application
sub Initialize
{
   # Assume failure
   my $RC = false;
   my $Txt = "";

   # Create QXDM object
   $QXDM = QXDMInitialize();
   if (not defined $QXDM)
   {
      print "\nError launching QXDM";
      return $RC;
   }

   SetQXDM ( $QXDM );

   # Success
   $RC = true;
   return $RC;
}

# Extracts F3 messages containing a specific string
sub GrepISF
{
   # Assume failure
   my $RC  = false;
   my $Txt = "";
   my $ItemIndex = 0;
   my $ItemCount = 0;
   my $item      = 0; 
   my $ItemSummary      ="";
   my $ItemTS = 0;

   # Load the item store file
   my $Handle = $QXDM->LoadItemStore( $ISFAbsolutePath );
   print "Loaded ISF with file handle " . $Handle . "\n";

   if ($Handle == 0xFFFFFFFF)
   {
      $Txt = "Unable to load ISF:\n" . $ISFAbsolutePath;
      print "\n$Txt\n";

      return $RC;
   }

   my $IClient = $QXDM->GetClientInterface( $Handle );

   if (!$IClient)
   {
      $Txt = "Unable to obtain ISF client interface";
      print "\n$Txt";

      $QXDM->CloseItemStore( $Handle );
      return $RC;
   }

   # Register client and get client handle
   my $ClientHandle = $IClient->RegisterClient( true );
   if ($ClientHandle == 0xFFFFFFFF)
   {
      $Txt = "Unable to register ISF client";
      print "\n$Txt";

      $QXDM->CloseItemStore( $Handle );
      return $RC;
   }
   else
   {
      print "Registered client with client handle " . $ClientHandle . "\n";
   }

   # Configure client and get an AutomationConfigClient obj, $IConfig
   my $IConfig = $IClient->ConfigureClient( $ClientHandle );
   if (!$IConfig)
   {
      $Txt = "Unable to configure ISF client";
      print "\n$Txt";

      $IClient->UnregisterClient( $ClientHandle );
      $QXDM->CloseItemStore( $Handle );
      return $RC;
   }

   # Configure the client for supported log keys
   $IConfig->AddItem( 6 ); # Add all the F3 messages

   if ( $SearchString ne "" )
   {
      # Add summary to search space
      $IConfig->AddSearchContent(4);
      # Configure the search engine
      $IConfig->SetSearchRegExEngine(1); # Set PERL Regular expressions.
      # Search for a string
      $IConfig->SetSearchString($SearchString);
   }

   #Update the Client Configuration
   $IConfig->CommitConfig();

   # Populate the client with all instances of supported logs
   $IClient->PopulateClients();

   ## Get item count
   $ItemCount = $QXDM->GetClientItemCount( $ClientHandle );

   if ( $ItemCount == 0 )
   {
     print "No Messages found. Make sure messages are enabled \n";
     return;
   }

   print "Processing ".$ItemCount. " Msg(s) .........\n";

   for( my $ItemIndex = 0; $ItemIndex < $ItemCount; $ItemIndex++ )
   {
     ## Get an item 
     $item = $QXDM->GetClientItem( $ClientHandle, $ItemIndex );

     ## Get time
     $ItemTS = $item->GetItemSpecificTimestampText(0,1);

     $ItemSummary = $item->GetItemSummary();
     print "   " . $ItemTS . ", " . $ItemSummary . "\n";
   }


   $IClient->UnregisterClient( $ClientHandle );
   $QXDM->CloseItemStore( $Handle );

   $RC = true;
   return $RC;
}



# Main body of script
sub Execute
{
   # Parse out arguments
   my $RC = ParseArguments();
   if ($RC == false)
   {
      return;
   }

   # Initialize ISF automation interface
   $RC = Initialize();
   if ($RC == false)
   {
      return;
   }

   # Obtain erasure rate data from ISF file
   $RC = GrepISF();
   if ($RC == false)
   {
      return;
   }

}

Execute();