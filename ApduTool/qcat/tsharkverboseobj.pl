#----------------------------------------------------------------------------
# APEX 6.x tsharkverboseobj.pl
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
use IO::File;
package tsharkverboseobj;

sub new {
   my $class = shift @_;
   my $this = {};
   bless $this, $class;
   return $this;        
}

sub open {
   my $FrameLine;
   my $tsharkTxtLine;
   my $time;
   my $fh;

   my $this = shift @_;
   $$this{file_name} = shift @_;
   $$this{time_stamp} = -1;

   $fh = new IO::File $$this{file_name}, "r";
   if (!defined $fh) {
       return 1;
   }
   $$this{fh}=$fh;
   $FrameLine = $fh->getline();

   if($FrameLine =~ /^Frame /) {
      $tsharkTxtLine  = $fh->getline(); 

      if($tsharkTxtLine =~ /([0-9]{2}):([0-9]{2}):([0-9]{2}\.[0-9]{1,9})/) {
             $time=(3600*$1 + 60*$2 + $3)*1000;
             $$this{time_stamp}=$time + $$this{time_offset};
             $$this{packet} = ">>>>>".$$this{file_name}.":\n"
                            . $FrameLine
                            . $tsharkTxtLine;
             for(;;) {
                $FrameLine = $fh->getline();
                last if ($FrameLine =~ /^Frame /);
                $$this{packet} = $$this{packet} . $FrameLine;
             }
             $$this{frame}=$FrameLine;
      }
   }
   return 0;
}

sub get_filename {
   my $this = shift @_;
   return $$this{file_name};
}

sub get_timestamp {
   my $this = shift @_;
   return $$this{time_stamp};
}

sub get_packet {
   my $this = shift @_;
   return $$this{packet};
}

sub set_timeOffset {
   my $this = shift @_;
   $$this{time_offset} = shift @_;
}

sub set_source{
   my $this = shift @_;
   $$this{filter} = shift @_;
   }

sub next {
   my $this = shift @_;
   my $tsharkTxtLine; 
   my $FrameLine;
   my $time;
   my $fh;

   $fh = $$this{fh};
   $FrameLine = $$this{frame};

   while($tsharkTxtLine = $fh->getline()) 
   {
      if($tsharkTxtLine =~ /([0-9]{2}):([0-9]{2}):([0-9]{2}\.[0-9]{1,9})/) {
             $time=(3600*$1 + 60*$2 + $3)*1000;
             $$this{time_stamp}=$time + $$this{time_offset};
             $$this{packet} = ">>>>>".$$this{file_name}.":\n"
                            . $FrameLine
                            . $tsharkTxtLine;
             for(;;) {
                $FrameLine = $fh->getline();
                if ($FrameLine) {
                   last if ($FrameLine =~ /^Frame /);
                   $$this{packet} = $$this{packet} . $FrameLine;
                }
                else {
                   $$this{time_stamp}=-1;
                   return;
                }
             }
             $$this{frame}=$FrameLine;
             return;    
      } 
      next;     
   }
   $$this{time_stamp}=-1;
}

1;

