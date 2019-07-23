# ThAutoUpdate
.NET Assembly auto update with NetSparkle
## server appcast.xml
### **location**
http服务默认页位置，当然也能放在自己能找到的位置
### **content**
>**title** 软件名

>**link** 暂时不知道有什么用，默认指向的是本文件地址

>**language** 语言

>**item**  
>>**sparkle:releaseNotesLink**  
>>>版本信息存放链接，存放在html文件中用来显示一些更新信息  
>>  
>>**pubDate**  
>>>版本发布日期  
>>  
>>**enclosure**  
>>>  
>>>**url** 放置安装包的位置，http服务默认页位置，当然也能放在自己能找到的位置  
>>>  
>>>**sparkle:version** 安装包版本，主要匹配的就是这个字段  
>>>  
>>>**length**  此属性没用，无论是多少netsparkle都正常工作  
>>>  
>>>**type** 此属性默认  
>>>  
>>>**sparkle:dsaSignature** 此属性是DSA*私钥*，匹配程序中公钥使用  

## server release-note.html
### **location**
http服务默认页位置，当然也能放在自己能找到的位置
### **content**
任意编辑，会以html格式显示在更新信息的地方  

## server xxxx.exe/xxxx.msi
### **location**
http服务默认页位置，当然也能放在自己能找到的位置
### **details**
这是安装程序

## client
**AppcastUrl** 请指向server的appcast.xml的地址如（http://your_server_ip/appcast.xml）
**DSAPublicKey** DSA*公钥*，使用openssl生成，公钥放在这个位置，和appcast.xml中**sparkle:dsaSignature**属性匹配使用