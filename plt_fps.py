import pdb
import matplotlib.pyplot as plt
import numpy as np
from scipy.stats import norm
import statistics
import math

file_suffix = "FPS_test_1.csv"
def formatLabel(height, width):
    return str(height) + "x" + str(width)

def formatFileName(height, width):
    return str(height) + " " + str(width) + " " + file_suffix

def processFile(height, width):
    filename = formatFileName(height, width)
    vals = []
    buckets = {}
    cumsum = 0
    with open(filename) as f:
        skipped = False
        for line in f:
            if(not skipped):
                skipped = True
                continue
            vals.append(float(line))
            buckets[vals[-1]] = True
            cumsum+=float(line)
   
    plt.hist(vals, 50, 
            label=formatLabel(height, width),
            weights=np.ones_like(vals)/float(len(vals)),
            linewidth=2,
            histtype="bar",
            alpha=.75
        )
    


processFile(2160, 3840)
processFile(1440, 2560)
processFile(1080, 1920)

plt.title('FPS Capture with target FPS of 24', fontsize=16)

plt.xlabel('FPS', fontsize=14)
plt.ylabel('Probability', fontsize=14)
plt.legend(loc='right', fontsize=12)


plt.show()