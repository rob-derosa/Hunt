#!/bin/bash

solutionDir=
token=
distributionGroup=
releaseNotes=
configFilePath=

# read in the input arguments
while getopts "s:t:g:r:c:" object; do
	case "${object}" in
		s) solutionDir="${OPTARG}" ;;
		t) token="${OPTARG}" ;;
		g) distributionGroup="${OPTARG}" ;;
		r) releaseNotes="${OPTARG}" ;;
		c) configFilePath="${OPTARG}" ;;
	esac
done
shift $((OPTIND-1))

buildSolution="$solutionDir/Hunt.Mobile.sln"
projectName="Hunt.Mobile.iOS"
buildProjectName=${projectName//./_}
projectDir="$solutionDir/$projectName"
buildDir="$solutionDir/Builds"
binaryOutputPath="$buildDir/$projectName.ipa"
infoPlist="$projectDir/Info.plist"

echo "Solution directory:	$solutionDir"
echo "Solution to build:	$buildSolution"
echo "Project directory:	$projectDir"
echo "Project name:		$buildProjectName"
echo "Binary output path:	$binaryOutputPath"

# the config.json file will be replaced right before compilation so keys/tokens are included
originalConfigFilePath="$solutionDir/Hunt.Mobile.Common/config.json"
tempConfigFilePath="$buildDir/config.json"

# read out the current version build numbers
newVersionNumber=
versionNumber=$(/usr/libexec/PlistBuddy -c "Print :CFBundleShortVersionString" $infoPlist)
buildNumber=$(/usr/libexec/PlistBuddy -c "Print :CFBundleVersion" $infoPlist)
echo "Current version:	$versionNumber"
echo "Current build:		$buildNumber"

IncrementVersionNumber ()
{
    version=$1
	a=( ${version//./ } )			# replace points, split into array
	((a[1]++))						# increment revision (or other part)
	newVersionNumber="${a[0]}.${a[1]}"		# compose new version
}

# increment the version build numbers
IncrementVersionNumber $versionNumber

echo "New version:		$newVersionNumber"
echo "New build:		$newVersionNumber"

read -p "Does everything look good?"

/usr/libexec/PlistBuddy -c "Set :CFBundleShortVersionString $newVersionNumber" $infoPlist
/usr/libexec/PlistBuddy -c "Set :CFBundleVersion $newVersionNumber" $infoPlist

# copy the tokenized config file into the project to be compiled
yes | cp -rf "$originalConfigFilePath" "$tempConfigFilePath"
yes | cp -rf "$configFilePath" "$originalConfigFilePath"

binaryUploadPath="$buildDir/com.microsoft.hunt.$newVersionNumber.ipa"

nuget restore $buildSolution

# build the iOS app
msbuild $buildSolution /t:$buildProjectName /p:IpaPackageDir=$buildDir /p:OutputPath="bin/iPhone/Release/" /p:Configuration=Release /p:Platform=iPhone /p:BuildIpa=true

cp "$binaryOutputPath" "$binaryUploadPath"
mv "$tempConfigFilePath" "$originalConfigFilePath"

echo "Build complete: $binaryUploadPath"

appcenter distribute release -g $distributionGroup -f $binaryUploadPath -a "Hunt-App/Hunt" -r "$releaseNotes" --token $token --debug

echo "Release of Hunt for iOS v$newVersionNumber to distribution group '$distributionGroup' complete!"