#!/bin/bash

solutionDir=
token=
distributionGroup=
releaseNotes=
configFilePath=
keystorePassword=

# read in the input arguments
while getopts "s:t:g:r:c:k:" object; do
	case "${object}" in
		s) solutionDir="${OPTARG}" ;;
		t) token="${OPTARG}" ;;
		g) distributionGroup="${OPTARG}" ;;
		r) releaseNotes="${OPTARG}" ;;
		c) configFilePath="${OPTARG}" ;;
		k) keystorePassword="${OPTARG}" ;;
	esac
done
shift $((OPTIND-1))

buildSolution="$solutionDir/Hunt.Mobile.sln"
projectName="Hunt.Mobile.Android"
buildProjectName=${projectName//./_}
projectDir="$solutionDir/$projectName"
buildDir="$solutionDir/Builds"
binaryOutputPath="$projectDir/bin/Release/com.microsoft.hunt-Signed.apk"
manifestXml="$projectDir/Properties/AndroidManifest.xml"

echo ""
echo "Solution directory:	$solutionDir"
echo "Solution to build:	$buildSolution"
echo "Project directory:	$projectDir"
echo "Project name:		$buildProjectName"
echo "Binary output path:	$binaryOutputPath"

# the config.json file will be replaced right before compilation so keys/tokens are included
originalConfigFilePath="$solutionDir/Hunt.Mobile.Common/config.json"
tempConfigFilePath="$buildDir/config.json"

# read out the current version build numbers
currentVersion=`grep versionName $manifestXml | sed 's/.*versionName="//; s/".*//'`
currentVersionCode=`grep versionCode $manifestXml | sed 's/.*versionCode="//; s/".*//'`
newVersion=

echo "Current version name:	$currentVersion"
echo "Current version code:	$currentVersionCode"

IncrementVersionNumber ()
{
    version=$1
	a=( ${version//./ } )			# replace points, split into array
	((a[1]++))						# increment revision (or other part)
	newVersion="${a[0]}.${a[1]}"		# compose new version
}

# increment the version build numbers
IncrementVersionNumber $currentVersion
newVersionCode=${newVersion//./}

echo "New version name:	$newVersion"
echo "New version code:	$newVersionCode"

read -p "Does everything look good?"

sed -i '' 's/versionCode *= *"'$currentVersionCode'"/versionCode="'$newVersionCode'"/; s/versionName *= *"[^"]*"/versionName="'$newVersion'"/' $manifestXml

#echo $originalConfigFilePath
#echo $tempConfigFilePath
#echo $configFilePath

# copy the tokenized config file into the project to be compiled
yes | cp -rf "$originalConfigFilePath" "$tempConfigFilePath"
yes | cp -rf "$configFilePath" "$originalConfigFilePath"

binaryUploadPath="$buildDir/com.microsoft.hunt.$newVersion.apk"

nuget restore $buildSolution

# build the Android app
msbuild "$projectDir/$projectName.csproj" /t:SignAndroidPackage /p:OutputPath="bin/Release/" /p:Configuration=Release /p:AndroidKeyStore=true /p:AndroidSigningKeyAlias="hunt adhoc key" /p:AndroidSigningKeyPass="$keystorePassword" /p:AndroidSigningKeyStore="$solutionDir/Resources/Assets/Security/Hunt AdHoc Key.keystore" /p:AndroidSigningStorePass="$keystorePassword"

cp "$binaryOutputPath" "$binaryUploadPath"
mv "$tempConfigFilePath" "$originalConfigFilePath"

echo "Build complete: $binaryUploadPath"

appcenter distribute release -g $distributionGroup -f $binaryUploadPath -a "Hunt-App/Hunt-Android" -r "$releaseNotes" --token $token --debug

echo "Release of Hunt for Android v$newVersion to distribution group '$distributionGroup' complete!"