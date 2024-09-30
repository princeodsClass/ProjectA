import os
import sys

oParams = {
	"ProjName": sys.argv[1],
	"ProjPath": sys.argv[2],
	"BuildOutputPath": sys.argv[3],
	"Platform": sys.argv[4],
	"ProjPlatform": sys.argv[5],
	"BuildVer": sys.argv[6],
	"BuildFunc": sys.argv[7],
	"PipelineName": sys.argv[8],
	"BuildFileExtension": sys.argv[9],
	"BundleID": sys.argv[10],
	"ProfileID": sys.argv[11],
	"IPAExportMethod": sys.argv[12],
	"BuildNumber": sys.argv[13],
	"BuildMode": sys.argv[14],
	"Branch": sys.argv[15]
}

oSrcPath = f"{oParams['ProjPath']}/{oParams['BuildOutputPath']}"
oDestPath = f"{oParams['Platform']}BuildOutput.{oParams['BuildFileExtension']}"

os.system(f"cp \"{oSrcPath}\" \"{oDestPath}\"")
os.system(f"xcrun altool --upload-app -f \"{oDestPath}\" -t \"iOS\" -u \"are2341@nate.com\" -p \"ixed-hszb-kzem-zgpw\" --verbose")
