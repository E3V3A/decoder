#----------------------------------------------------------------------------
# APEX 6.x Qcatobj.pl
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
package Qcatobj;

sub new {
   my $class = shift @_;
   my $this = {};
   bless $this, $class;
   return $this;        
}

sub set_output {
   my $this = shift @_;
   my $fn  = shift @_;
   open MERGE_FH, ">$fn" or die "Cannot open $fn for writing !";
   return;
}

sub close_fh {
   close TXT_FILEHANDLE;
}

sub set_enh {
   my $this = shift @_;
   $$this{use_enh} = shift @_;
}
   

sub open {
   my $QcatTxtLine;
   my $t;
   my $count=0;

   my $this = shift @_;
   $$this{file_name} = shift @_;
   $$this{time_stamp} = -1;

   open TXT_FILEHANDLE, $$this{file_name} or return 1;
 
   ## Find first time stamp
   while($QcatTxtLine = <TXT_FILEHANDLE>)
   {
      last if($QcatTxtLine =~ /([0-9]{2}:[0-9]{2}:[0-9]{2}\.[0-9]{1,9}\s+\[.{2,8}\]\s+0x....)/);
      print MERGE_FH $QcatTxtLine;
   }
   close MERGE_FH;

   while(1) { 
      #if this is a line with a TimeStamp "21:54:54.449 [192/0xC0] 0x1EEB" or "21:54:54.449 [C0] 0x1EEB"
      if($QcatTxtLine =~ /([0-9]{2}):([0-9]{2}):([0-9]{2}\.[0-9]{1,9})\s+\[.{2,8}\]\s+(0x....)/) {
         if ($count==0) {
             $t=(3600*$1 + 60*$2 + $3)*1000;
             $$this{time_stamp}=$t;
             $$this{packet} = $QcatTxtLine;
             $$this{code} = $4;
             $count++;
         }
         else {            
             last;
         } 
      }
      else {
         $$this{packet} = $$this{packet}.$QcatTxtLine;
      }
      $QcatTxtLine = <TXT_FILEHANDLE>;
   }  
   $$this{QcatTxtLine} = $QcatTxtLine;
   
   return 0;
}

sub get_filename {
   my $this = shift @_;
   return $$this{file_name};
}

sub get_timestamp {
   my $this = shift @_;
   if ($$this{time_stamp}==-1) {
       return -1;
   }
   
   if($$this{use_enh}==1)
   {
      if (($$this{code}=~"0x1113") ||
          ($$this{code}=~"0x1114") ||
          ($$this{code}=~"0x1115") ||
          ($$this{code}=~"0x1123") ||
          ($$this{code}=~"0x1124") ||
          ($$this{code}=~"0x1125") ||
          ($$this{code}=~"0x1133") ||
          ($$this{code}=~"0x1134") ||
          ($$this{code}=~"0x1135") ||
          ($$this{code}=~"0x1143") ||
          ($$this{code}=~"0x1144") ||
          ($$this{code}=~"0x1145") ||
          ($$this{code}=~"0x109C") ||
          ($$this{code}=~"0x109D") ||
          ($$this{code}=~"0x11EB") )
      {
         return $$this{time_stamp};
      }
      return 0;
   }
   
   else {
    if (($$this{code}=~"0x1FFC") ||  
          ($$this{code}=~"0x1FEA") ||
          ($$this{code}=~"0x1FFD") ||  
          ($$this{code}=~"0x1FF0") )  
      {
           return 0;
      }
      return $$this{time_stamp};
  }     
}

sub get_packet {
   my $this = shift @_;
   return $$this{packet};
}

sub next {
   my $this = shift @_;
   my $QcatTxtLine = $$this{QcatTxtLine};
   my $t;
   my $count=0;
 
   if (!$QcatTxtLine) {
        $$this{time_stamp}=-1;
        return;
   }
   
   while(1) { 
      #if this  is a line with a TimeStamp "21:54:54.449 [192/0xC0] 0x1EEB" or "21:54:54.449 [C0] 0x1EEB"
      if($QcatTxtLine =~ /([0-9]{2}):([0-9]{2}):([0-9]{2}\.[0-9]{1,9}) \[.{2,8}\]\s+(0x....)/) {
         if ($count==0) {
             $t=(3600*$1 + 60*$2 + $3)*1000;
             $$this{time_stamp}=$t;
             $$this{packet} = $QcatTxtLine;
             $$this{code} = $4;
             $count++;
         }
         else {
             last;
         } 
      }
      else {
         $$this{packet} = $$this{packet}.$QcatTxtLine;
      }
      $QcatTxtLine = <TXT_FILEHANDLE>;
      if (!$QcatTxtLine) {
         last;
      }         
   }
   $$this{QcatTxtLine} = $QcatTxtLine;
}

1;