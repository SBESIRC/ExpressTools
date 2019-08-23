#建立自证书
& "./makecert.exe" -sv "TianhuaAi.PVK" -n "CN=TIANHUA-AI,E=ai@thape.com.cn" TianhuaAi.cer -r -b 01/01/2019 -e 01/01/2050
& "./cert2spc.exe" TianhuaAi.cer TianhuaAi.spc
& "./pvk2pfx.exe" -pvk TianhuaAi.PVK -pi 123456 -spc TianhuaAi.spc -pfx TianhuaAi.pfx -f

$script:folder_start = "..\build\bin"
$script:THDLL_file_name = "Th"
$script:fileList = Get-ChildItem $folder_start -recurse *$THDLL_file_name* | %{$_.FullName}
Foreach($script:file in $fileList)
{
    if($file -like "*\Th*.dll")
    {
        #添加时间戳的命令，添加时间戳速度非常慢且可能失败
        #& "./signtool.exe" sign /f TianhuaAi.pfx /t http://timestamp.verisign.com/scripts/timstamp.dll /p 123456 $file
        #没有添加时间戳的命令
        & "./signtool.exe" sign /f TianhuaAi.pfx /p 123456 $file
        $count++
    }
}
echo "complete $count file"
