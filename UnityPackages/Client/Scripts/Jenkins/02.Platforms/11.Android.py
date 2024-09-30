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

oBuildOutputDirPath = f"{oParams['ProjPath']}/{os.path.dirname(oParams['BuildOutputPath'])}"

for oPath, oDirNames, oFileNames in os.walk(oBuildOutputDirPath):
	for i, oFileName in enumerate(oFileNames):
		oSrcPath = f"{oPath}/{oFileName}"
		oDestPath = f"{oPath}/{oParams['Platform']}BuildOutputSymbols.zip"

		# 심볼 결과가 존재 할 경우
		if os.path.exists(oSrcPath) and oSrcPath.lower().endswith(".zip"):
			# 이전 심볼 결과가 존재 할 경우
			if os.path.exists(oDestPath):
				os.system(f"rm -rf \"{oDestPath}\"")

			os.system(f"mv \"{oSrcPath}\" \"{oDestPath}\"")
