import json
import subprocess

csharp_executable = "./bin/Release/net8.0/publish/Simplex"
csharp_output = subprocess.check_output([csharp_executable])

parsed_output = json.loads(csharp_output)

print(parsed_output[0])
print(parsed_output)
