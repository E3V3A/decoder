use Win32::OLE;
use Win32::Registry;
use Win32;
use File::Spec;

my $version = GetToolRegistryVersion('QCAT6.Application');
my $daysLeft = GetLicenseTimeLeft('QCAT6.Application');
print("Version = $version, $daysLeft days left\n");

$version = GetToolRegistryVersion('QCAT6.Application');
$daysLeft = GetLicenseTimeLeft('QCAT6.Application');
print("Version = $version, $daysLeft days left\n");

#------------------------------------------------------------------------------
# Find the disk location of a tool given its automation program ID
#------------------------------------------------------------------------------
sub GetToolPath($)
{
   my ($progId) = @_;
   return undef if (!defined($progId));

   # get the registry path
   my ($classId, $progPath);
   $::HKEY_CLASSES_ROOT->QueryValue ($progId . "\\CLSID", $classId) or return undef;
   $::HKEY_CLASSES_ROOT->QueryValue ("\\CLSID\\$classId\\LocalServer32", $progPath) or return undef;
   return "Not Installed" if (!$progPath);

   #Must have non-quoted, short format pathname
   $progPath =~ s/\"//g;
   $progPath = Win32::GetShortPathName($progPath);

   # get version
   if(!-e $progPath)
   {
      return undef;
   }
   else
   {
      return $progPath;
   }
}

#------------------------------------------------------------------------------
# Get the approximate number of days left on a license for a tool with 
# the given prog ID
#------------------------------------------------------------------------------
sub GetLicenseTimeLeft($)
{
   my ($progId) = @_;
   $path = GetToolPath($progId);
   if(!defined($path))
   {
      return undef;
   }

   $path =~ s/(.+\\).+/$1/i;
   $path .= "license.txt";
   if(! -e $path)
   {
      return undef;
   }
   
   open(LICENSE, "<$path");
   foreach my $line (<LICENSE>)
   {
      if($line =~ /End.+(\d{4})\s+(\d{2})\s+(\d{2}).*$/)
      {
         my $expYear = $1;
         my $expMonth= $2;
         my $expDay  = $3;
         
         my ($sec,$min,$hour,$mday,$mon,$year,$wday,$yday,$isdst) =
            gmtime(time);
            
         $expYear -= $year + 1900;
         $expMonth-= $mon + 1;
         $expDay  -= $mday;
         $expDay  += $expYear * 365 + $expMonth * 30;
         
         close(LICENSE);
         return $expDay;
      }
   }
   close(LICENSE);
   return undef;
}

#------------------------------------------------------------------------------
# Get the number of days left on a license for a tool with the given prog ID
#------------------------------------------------------------------------------
sub GetToolRegistryVersion($)
{
   my ($progId) = @_;
   $progPath = GetToolPath($progId);
   if(!defined($progPath))
   {
      return undef;
   }
   
   # get version
   return "Unknown" if !-e $progPath;
   $progPath = File::Spec->rel2abs($progPath) if $progPath;
   my $fso = Win32::OLE-> new('Scripting.FileSystemObject');
   return "Unknown" unless $fso;
   my $ver = $fso-> GetFileVersion($progPath);
   undef $fso;
   return "Unknown" if !$ver;
   return $ver;
} # end of GetToolRegistryVersion