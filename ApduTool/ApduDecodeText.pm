use 5.010;

my $rawfile;
my $msgfile;
my $apdufile;
my $APDU_FILE = undef;

my @item;
my $index = 0;
my $time = null;
my $type = 0;
my $description = null;

sub writeFile
{
    my $fh = shift;
    my $summary = shift;
    my $newtime = shift;

    if($newtime == undef)
    {
        $newtime = $time;
    }

    #if($type == 0x1feb)
    {
        say $fh sprintf "%-7d %s 0x%4X %s %s", $index, $newtime, $type, $description, $summary;
    }
}

sub writeApdu
{
    my $summary = shift;
    if($APDU_FILE eq undef)
    {
        say "open $apdufile";
        #暂时不考虑关闭文件
        open($APDU_FILE, ">$apdufile")||die "open file $apdufile failed";
    }
    writeFile($APDU_FILE, $summary);
}

sub handle1098
{
    print "\nhandle1098";
}

sub parseSelect
{
    my $fid = $item[5];

    my $sfi;
    foreach my $i (13..$#item)
    {
        if($item[$i] =~ /  Short File Identifier        : (.*)/)
        {
            $sfi = "SFI: ".$1;
        }
    }
    return $fid." ".$sfi;
}

sub parseBinary
{
}

sub parseRecord
{
}
sub parseBinary
{
}
sub parseSTK
{
}
sub parseEnvelop
{
}


my %apduhash =(
	"SELECT", \&parseSelect,
	"READ BINARY", \&parseBinary,
	"UPDATE BINARY", \&parseBinary,
	"READ RECORD", \&parseRecord,
	"UPDATE RECORD", \&parseRecord,
	"FETCH", \&parseSTK,
	"TERMINAL RESPONSE", \&parseSTK,
	"ENVELOPE", \&parseEnvelop
    );

sub apduParsing
{
    splice(@item, 0, 1);
  
    my $line = shift @item;
    if(index($line, "incomplete APDU") != -1)
    {
        $line = shift @item;
    }
    chomp($line);

    my $extra;
    if($apduhash{$line} ne undef)
    {
        $extra = $apduhash{$line}->();
        if($extra ne undef)
        {
            return "Summary: ".$line.", ".$extra;
        }
    }
    return "Summary: ".$line;
}



sub handle14ce
{
    my $direction;
    my $slot;
    my $apdu;
    my $found = false;
    #say "handle14ce";

    foreach my $i (1..@item)
    {
        my $line = shift @item;
        
        if($line =~ /\|   ([TR]X) Data\|   (\d)\|  (..)\|/)
        {
            $direction = $1;
            $slot = $2;
            $apdu .= $3." ";
            next;
        }

        if(index($line, "APDU Parsing", 3) != -1)
        {
            $found = true;
            last;
        }
    }
    
    writeApdu("SLOT".$slot." ".$direction.": ".$apdu);

    if($found)
    {
        say "found";
        writeApdu("SLOT".$slot." ".apduParsing);
    }
}

my %typehash = (
    0x14ce, \&handle14ce,
    0x1098, \&handle1098);

sub processItem
{
    #say $index;
    if($time == null || @item == 0)
    {
        return;
    }

    #say "processitem";
    chomp(@item);
    if($typehash{$type} ne undef)
    {
        $typehash{$type}->();
    }
}

sub ApduDecodeText
{
    ($rawfile, $msgfile, $apdufile) = @_;
    
    open(FILE, $rawfile)||die"cannot open the file: $!\n";

    my $current = 0;
    while (<FILE>){
        $current++;

        #print "\n$_";
        if(/^\d{4} .{6}  (..:..:..\....)  .{4}  0x(....)  (.*)/)
        {
            processItem;
            #print "ApduDecodeText";
            undef(@item);
            $index = $current;
            $time = $1;
            $type = hex($2);
            $description = $3;
        }
        else
        {
            push(@item, $_);
        }
        
    }
    processItem;
    close FILE;
}

1;
