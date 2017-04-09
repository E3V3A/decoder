print "enabling QDSS\n";
&EnableQDSSLogging();
print "enabling QDSS done";

sub EnableQDSSLogging
{    
    print "******** Start EnableQDSS *********\n";
	system 1, "adb root";
    system 1,"adb shell setprop persist.sys.usb.config diag,qdss,adb";
	sleep(5);
    system 1,'adb shell "echo 0 > /sys/bus/coresight/devices/coresight-modem-etm0/enable"';
    sleep(5);
    my $comport = &GetQpstPort();
    sleep(10);	
	if (defined $comport)
	{
		print "\ncomport is $comport \n";	
		print "================ Start  Configuring USB as the current QDSS trace sink ===================\n";	
		eval
		{
		  system 1,'adb shell "echo usb > /sys/bus/coresight/devices/coresight-tmc-etr/out_mode"';
		  system 1,'adb shell "echo 1 > /sys/bus/coresight/devices/coresight-tmc-etr/curr_sink"';
		  system 1,'adb shell "echo 1 > /sys/bus/coresight/devices/coresight-stm/enable"';      
	          #system 1, "diag-on-qxdm4.bat $comport 50\n";	
		  print "================ END Configuring===================\n";			
		}; 
		die "XXXX ERROR: FAILED Diag on QXDM execution\n" if $@;
		# or do {
		# my $e = $@;
		# print "******** Diag on QXDM execution Failed: $e*********\n";
		# };
		#sleep(5); #QDSSdiag crashed
		print "\n================ senddata===================\n";
		&SendData($comport);		
	}
	else
	{
		my $errmsg = "Error in getQpstPort";
		print "Error in getQpstPort\n";
	}    
    print "******** END EnableQDSSLogging *********\n";
}



sub GetQpstPort
{
my $prod_id = "QPSTAtmnServer.Application";
my $qpst;

print "Initializing QPST instance\n";
eval
{
  $qpst = Win32::OLE->GetActiveObject($prod_id)
};

die "$prod_id not installed" if $@;

unless (defined $qpst)
{
  $qpst = Win32::OLE->new($prod_id, sub {$_[0]->Quit;}) or die "Oops, cannot start $prod_id";
  sleep (10);
  print "done qpst\n";
}

# Translate phone mode to string.
my %phone_mode_list = qw (
  0 none
  2 download
  3 diag_online
  4 diag_offline
  6 stream_download
  12 sahara);

# Translate phone status to string.
my %phone_status_list = qw (
  0 no_phone
  1 init_state
  2 port_not_present
  3 phone_not_present
  4 phone_not_supported
  5 ready);

if (defined $qpst)
{
  my $port_list = $qpst->GetPortList();
  my $phone_count = $port_list->PhoneCount;

  for (my $i = 0 ; $i < $phone_count ; ++$i)
  {
    my $port_name = $port_list->PortName($i);  
    my $phone_mode = $phone_mode_list{$port_list->PhoneMode($i)};
    my $phone_status = $phone_status_list{$port_list->PhoneStatus($i)};

    print "\n";
    print "port name    [$i] : $port_name\n";
    print "port status  [$i] : $phone_status\n";
    print "port mode    [$i] : $phone_mode\n";

    if (("ready" eq $phone_status) || ("diag_online" eq $phone_mode) )
    {
       undef $port_list;
       undef $qpst; 
       return  $port_name;   
    }    
  }
  undef $port_list; 
}
else{
print "*****QPST is not defined*****\n"
}

# Release the automation server.
undef $qpst;
return undef;
}

our $QXDM;
# Initialize QXDM4 application
sub Initialize
{
   # Assume failure
   my $RC = 0;
   print "Initializing QXDM application\n";

   # Create QXDM object
   $QXDM = QXDMInitialize();
   sleep(10);    
   print "sleep 10 in qxdm\n";   
   if (! defined($QXDM))
   {
      print "\nError launching QXDM";
   }

   SetQXDM( $QXDM );

   # Success
   $RC = 1;
   return $RC;
}


sub SendData
{

my $port=shift;
use HelperFunctions4;
use Win32::OLE::Variant;
$port=~s/COM//;
# COM port to be used for communication with the phone by QXDM
my $PortNumber = $port;

print "port to senddata is $PortNumber\n\n";

my $RC = Initialize();
   if ($RC == 0)
   {
      return;
   }
   # else {
	# print "*****QXDM initialization failed******\n"
   # }
   
   # Connect to our desired port
   $RC = Connect( $PortNumber );
   if ($RC == 0)
   {
      return;
   }
   
	print "DEBUG::: before sending data\n";
    my $cmdToQXDM = "send_data 0x4B 0x12 0x14 0x02 0x02 0x01 0x01";
    $QXDM->SendScript($cmdToQXDM);
	sleep(10); # we cannot increase sleep timer here, qdss logs wont be captured if delayed
    print "DEBUG::: after sending data\n";	
    my $var = Variant(VT_BOOL, 1);
	print "var is : $var\n";
    
	print "before enableqdss api call\n";
    #sleep(2);	
    $QXDM->EnableQDSS($var);
	print "after enableqdss api call\n";
    #sleep(5);
	
    sleep(35); 
	# this is for disable qdss after the end
    my $var_new = Variant(VT_BOOL, 0);
	print "var_new is : $var_new\n";	
	print "before disableqdss api call\n";
    sleep(5);	
    $QXDM->EnableQDSS($var_new);
    sleep(5);	
	print "after disableqdss api call\n";	
	#Turning oFF QDSS 
    # print "******** Disable qdss before quit *********\n";
	# system 1, "adb root";	
	# system 1,'adb shell "echo 0 > /sys/bus/coresight/devices/coresight-stm/enable"';
    # sleep(30); 	
	print "\nDisabled QDSS in QXDM";		
	$QXDM->QuitApplication();
	print "\nSUCCESS quit QXDM\n";
	

}

