#从入参中提取出备用文件名，所有全局变量都在此定义
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


# 过滤日志
use ApduISFAddItem;
#ApduISFAddItem($srcisf, $rawisf);

# 转为Text
use ApduSimpleParse;
#ApduSimpleParse($rawisf, $rawtxt);

# 文本分析
use ApduDecodeText;
ApduDecodeText($rawtxt, $msgtxt, $apdutxt);

