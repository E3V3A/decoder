#���������ȡ����־·�����ļ�����
my $isffile = shift;
$_ = $isffile;
m/(.*)\.(\w+)$/;
$basename = $1;
$suffix = $2;
print $basename.$suffix;

#���ù���ApduIsfExtractor������־
#����ApduIsf2TxtתΪText
#����ApduTxtAnalyzer�����ı�����
