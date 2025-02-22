import os
import re
from xml.etree import ElementTree

doc          = ElementTree.parse("RE-Editor/RE-Editor.csproj").getroot()
versions     = {}
passPercents = {}

for node in doc.findall(".//PropertyGroup[@Condition]"):
    name    = node.find(".//AssemblyName").text
    version = node.find(".//AssemblyVersion").text
    versions[name] = version

# Read in passing percents; default to 0 if not found.
testResultsDir = "TestResults"
for shortName in os.listdir(testResultsDir):
    path = os.path.join(testResultsDir, shortName, "Pass Percent.txt")
    if os.path.isfile(path):
        with open(path, "r") as file:
            percent = file.read()
            passPercents[shortName] = percent
    else:
        passPercents[shortName] = "0"

# Read the lines from README.md.
with open("README.md", "r") as file:
    lines = file.readlines()

# Update the linked releases in the table based on the current version in `RE-Editor.csproj`.
for i in range(len(lines)):
    match = re.compile(r"^([^|]+)\s\|\s([^|]+)\s\|\s\[.+?tag\/([^-]+-Editor).+?\s\|\s([\d%]+)$").match(lines[i])
    if match is not None:
        shortName   = match.group(1)
        middlePart  = match.group(2)
        progType    = match.group(3)
        version     = versions[progType]
        passPercent = passPercents[shortName]
        lines[i] = f"{shortName} | {middlePart} | [v{version}](https://github.com/Synthlight/MHR-Editor/releases/tag/{progType}_v{version}) | {passPercent}%\n"

# Write the updated lines back to README.md.
with open('README.md', 'w') as file:
    file.writelines(lines)