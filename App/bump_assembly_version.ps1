param($folder_start, $assembly_version_old, $assembly_version_new)

$shell_name = $MyInvocation.MyCommand
$version_format = "[0-9]\.[0-9]\.[0-9]"
$AssemblyInfo_file_name = "AssemblyInfo.cs"

$commond_parameter_missing = "please ckeck your commond and use commond like:<$shell_name> <folder_loction> <assembly_version_old> <assembly_version_new>"
$commond_parameter_error = 'assembly_version must like "x.x.x"'

if($assembly_version_new -eq $null)
{
    echo $commond_parameter_missing
}
elseif($assembly_version_old -eq $null)
{
    echo $commond_parameter_missing
}
else
{
    if(!($assembly_version_new -match $version_format))
    {
        echo $commond_parameter_error
    }
    elseif(!($assembly_version_old -match $version_format))
    {
        echo $commond_parameter_error
    }
    else
    {
        $fileList = Get-ChildItem $folder_start -recurse *$AssemblyInfo_file_name | %{$_.FullName}
        $count = 0
        Foreach($file in $fileList)
        {
            if(select-string $assembly_version_old $file)
            {
                (type $file) -replace ($assembly_version_old,$assembly_version_new)|out-file $file
                echo "edit ($file) finished"
                $count++
            }
        }
        echo "complete $count file"
    }
}