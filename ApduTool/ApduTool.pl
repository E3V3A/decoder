#���������ȡ�������ļ���������ȫ�ֱ������ڴ˶���
my $srcisf = shift;
$_ = $srcisf;
m/(.*)\.(\w+)$/;
$basename = $1;
$suffix = $2;

if ($suffix ne "isf")
{
	die "only isf format is allowed";
}
if (! -e $srcisf)
{
	die "log don't exist";
}
my $rawisf = $basename."_raw".".isf";
my $rawtxt = $basename."_raw".".txt";
my $msgtxt = $basename."_msg".".txt";
my $apdutxt= $basename."_apdu".".txt";


# ������־
use ApduISFAddItem;
#ApduISFAddItem($srcisf, $rawisf);

# תΪText
use ApduSimpleParse;
#ApduSimpleParse($rawisf, $rawtxt);

# �ı�����
use ApduDecodeText;
ApduDecodeText($rawtxt, $msgtxt, $apdutxt);

