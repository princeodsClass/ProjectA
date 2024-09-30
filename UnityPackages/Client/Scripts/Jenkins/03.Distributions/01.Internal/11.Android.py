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

oBuildSrcPath = f"{oParams['ProjPath']}/{oParams['BuildOutputPath']}"
oBuildDestPath = f"{oParams['ProjName']}/{oParams['PipelineName']}/{oParams['Branch']}/{oParams['Platform']}/{oParams['ProjName']}_{oParams['Platform']}_{oParams['BuildMode']}_v{oParams['BuildVer']}_{oParams['BuildNumber']}.{oParams['BuildFileExtension']}"

oSymbolsSrcPath = f"{oParams['ProjPath']}/{os.path.dirname(oParams['BuildOutputPath'])}/{oParams['Platform']}BuildOutputSymbols.zip"
oSymbolsDestPath = f"{oParams['ProjName']}/{oParams['PipelineName']}/{oParams['Branch']}/{oParams['Platform']}/{oParams['ProjName']}_{oParams['Platform']}_{oParams['BuildMode']}_v{oParams['BuildVer']}_Symbols_{oParams['BuildNumber']}.zip"

oDirPath = f"{oParams['ProjName']}/{oParams['PipelineName']}/{oParams['Branch']}/{oParams['Platform']}"

# 디렉토리가 없을 경우
if not os.path.exists(oDirPath):
	os.makedirs(oDirPath)

os.system(f"cp \"{oBuildSrcPath}\" \"{oBuildDestPath}\"")
os.system(f"cp \"{oSymbolsSrcPath}\" \"{oSymbolsDestPath}\"")
