# NOTE: This script must be run from a command box,
# i.e.  Perl QXDMSendDmIcdPacketEx.pl

# This script demostrates usage of the QXDM automation
# interface method SendDmIcdPacketEx

use HelperFunctions4;
use Win32::OLE::Variant;

# Global variable
my $QXDM;
# COM port to be used for communication with the phone by QXDM
my $PortNumber = "";

# Process the argument - port number
sub ParseArguments
{
   # Assume failure
   my $RC = false;
   my $Help = "Syntax: Perl QXDMSendDmIcdPacketEx.pl <COM Port Number>\n"
            . "Eg:     Perl QXDMSendDmIcdPacketEx.pl 5\n";

   if ($#ARGV < 0)
   {
      print "\n$Help\n";
      return $RC;
   }

   $PortNumber = $ARGV[0];
   if ($PortNumber < 1 || $PortNumber > 100)
   {
      print "\nInvalid port number\n";
      print "\n$Help\n";
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

   # Create QXDM object
   $QXDM = QXDMInitialize();
   if ($QXDM == null)
   {
      print "\nError launching QXDM";
   }

   SetQXDM( $QXDM );

   # Success
   $RC = true;
   return $RC;
}

# Main body of script
sub Execute
{   
	my $RC = ParseArguments();
   if ($RC == false)
   {
      return;
   }
   
   # Launch QXDM
   my $RC = Initialize();
   if ($RC == false)
   {
      return;
   }
   
   # Connect to our desired port
   $RC = Connect( $PortNumber );
   if ($RC == false)
   {
      return;
   }
   
   $timeout_in_ms = 2000;

   # Send Version Number command: Diag [0].
   my @DIAG_COMM_F = (0); 			#raw data buffer
   print "Send Diag Command: @DIAG_COMM_F \n";

   $diag_cmd = pack 'C*', @DIAG_COMM_F;		#Pack the buffer   

   # Build command as variant array of bytes.
   $diag_request_var = Variant(VT_ARRAY | VT_UI1, length $diag_cmd);
   $diag_request_var->Put($diag_cmd);
   $diag_reply = $QXDM->SendDmIcdPacketEx($diag_request_var, $timeout_in_ms);
   if (defined $diag_reply)
   {
     @reply_values = unpack "Ca11a8a11a8a8CCCSCS", $diag_reply;
     $mob_model = $reply_values[8];
     print "Read from response: mob_model = $mob_model\n";
   }
   else
   {
     print "No reply to DIAG_VERNO_F\n";
   }
}

Execute();