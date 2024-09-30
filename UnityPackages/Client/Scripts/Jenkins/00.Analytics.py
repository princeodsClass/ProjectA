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

oSrcPath = "Doxygen"
oDestPath = f"{oParams['ProjName']}_{oParams['Platform']}_v{oParams['BuildVer']}.zip"

os.system(f"ditto -ck --rsrc --sequesterRsrc \"{oSrcPath}\" \"{oDestPath}\"")
