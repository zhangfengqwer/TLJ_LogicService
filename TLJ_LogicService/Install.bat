%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe bin\Debug\TLJ_LogicService.exe
Net Start TLJ_LogicService
sc config TLJ_LogicService start= auto

pause