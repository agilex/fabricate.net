#!/bin/bash

# check env var
if [ -n "${AGILEX_NUGET_REPO:+x}" ]
then
	echo "using ${AGILEX_NUGET_REPO} as my deployment location"
	remoteRepo="${AGILEX_NUGET_REPO}"
else
	echo "Please set AGILEX_NUGET_REPO to point at the git repository you want to deploy to"
	echo "example: export AGILEX_NUGET_REPO=https://nicpillinger@appharbor.com/omigidev.git"
	exit
fi

# update assembly info
currentVersion="`grep AssemblyFileVersion agilex.fabricate/agilex.fabrication/Properties/AssemblyInfo.cs |cut -d '\"' -f2`"
echo "current assembly version for fabricate.net $currentVersion"
let patch=`echo $currentVersion | cut -d '.' -f4`+1
suggestedVersion="`echo $currentVersion | cut -d '.' -f1,2,3`.${patch}"
echo "enter new version number eg. $suggestedVersion" [Enter to accept]
read -e newversion
if [ "" == "${newversion}" ]
then
 newversion=$suggestedVersion
fi
echo "Using $newversion"
sed -i -e s/Version\(\"[0-9]*.[0-9]*.[0-9]*.[0-9]*\"\)/Version\(\""$newversion"\"\)/ agilex.fabricate/agilex.fabrication/Properties/AssemblyInfo.cs

# build package
nuget.exe pack -Build -OutputDirectory ./packages agilex.fabricate/agilex.fabrication/agilex.fabrication.csproj

# commit changes to github repo
git add .
git commit -m "updated fabricate.net package to version $newversion"
echo "push updated source to github"
git push

# get nuget repo
echo "pull agilex nuget repo, please enter your apphb password when prompted"
deployLocation=./deploy-nuget-packages
if [ -d $deployLocation ] 
then
	echo "updating nuget repo"
	cd $deployLocation
	git pull
else
	echo "cloning nuget repo from $remoteRepo to $deployLocation"
	git clone $remoteRepo $deployLocation
	cd $deployLocation
fi

# deploy to nuget repo
echo "deploying $newversion to nuget repo"
cp ../packages/*.nupkg ./packages
git add .
git commit -m "added fabricate.net package version $newversion"
echo "please enter your apphb password when prompted"
git push

cd ..
echo "my work here is done"
