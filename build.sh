#!/bin/bash

set -e

echo deleting previous build...
rm -rf dystopia_release/

echo building...
cd repo
dotnet publish -c Release Dystopia/Dystopia.csproj -r linux-x64 --self-contained true -o ../dystopia_release
cd ..

echo building docker container...
docker build -t dystopia:latest .

echo "done!"

