import subprocess
import os

main_path = os.path.dirname(__file__)

Parrallel = os.path.join(main_path, "DigitalMusicAnalysisParallel//bin//Debug//netcoreapp3.1//DigitalMusicAnalysis.exe")

Single = os.path.join(main_path,"DigitalMusicAnalysis//bin//Debug//netcoreapp3.1//DigitalMusicAnalysis.exe")

for i in range(50):
    proc = subprocess.run(Single, stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    print(i)
print(proc) # To see what it is.