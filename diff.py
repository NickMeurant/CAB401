import difflib
import os

main_path = os.path.dirname(__file__)
parrallel = os.path.join(main_path, 'DigitalMusicAnalysisParallel\\output.txt')
normal = os.path.join(main_path, 'DigitalMusicAnalysis\\output.txt')

print(main_path)

with open(parrallel) as file_1:
    file_1_text = file_1.readlines()
 
with open(normal) as file_2:
    file_2_text = file_2.readlines()

# Find and print the diff:
for line in difflib.unified_diff(
        file_1_text, file_2_text, fromfile='file1.txt',
        tofile='file2.txt', lineterm=''):
    print(line)