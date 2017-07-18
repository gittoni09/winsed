# winsed
Windsed v0.5
https://winsed.codeplex.com/

Windows command line utility for text processing, mimicking GNU sed. Ideal for editing text files on Powershell 
remoting sessions.

Currently only the following functionality has been implemented:

- Basic global text replacement: the following command will replace all the ocurrences of "expression" in a text file with "replacement":
	winsed -e "s/expression/replacement/g" InputFile > OutputFile

- Basic single text replacement: only the first ocurrence of "expression" will be replaced with "replacement":
	winsed -e "s/expression/replacement/" InputFile

- Limiting the range of lines to process. E.g. only process lines 44 through 48 of the input file: winsed.exe -e "44,48 s/my expression/my replacement/g" InputTextFile > OutputTextFile

- Single line to process. E.g. process only line 127 of the input file: winsed.exe -e "127 s/my expression/my replacement/g" InputTextFile > OutputTextFile

IMPORTANT NOTES:
	- Regular expressions for expression matching have not been implemented yet

	- winsed will NOT modify the source file. It will produce the modified file on the standard output only.

Systems requirements: 
 - Windows 8.1, Windows 10, Windows 2012, Windows 2012 R2
 - .NET 4.5 or higher
