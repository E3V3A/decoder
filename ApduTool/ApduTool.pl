#从入参中提取出日志路径和文件短名
my $isffile = shift;
$_ = $isffile;
m/(.*)\.(\w+)$/;
$basename = $1;
$suffix = $2;
print $basename.$suffix;

#调用工具ApduIsfExtractor过滤日志
#调用ApduIsf2Txt转为Text
#调用ApduTxtAnalyzer进行文本分析
