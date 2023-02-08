import pdb
import matplotlib.pyplot as plt
import numpy as np


def getFilename(height, width):
    return file_prefix + getLable(height, width) + file_suffix

def getLable(height, width):
    return str(height) + "x" + str(width)

def processFile(height, width):
    filename = getFilename(height, width)

    vals = []
    buckets = {}
    with open(filename) as f:
        skipped = False
        for line in f:
            if(not skipped):
                skipped = True
                continue
            vals.append(int(line))
            buckets[vals[-1]] = True

    print("Bytes Transfered " + getLable(height, width) + " " + str(np.cumsum(np.array(vals)).max() / len(vals)))

    h, edges = np.histogram(vals, density=True, bins=len(buckets), )
    h = np.cumsum(h)/np.cumsum(h).max()

    X = edges.repeat(2)[:-1]
    y = np.zeros_like(X)
    y[1:] = h.repeat(2)
    #pdb.set_trace()

    #fig, ax=plt.subplots()



    #plt.plot(X,y,label=getLable(height, width), lw=2)
#
    #plt.grid(True)
    
    #    plt.show()
    #
    #    h, edges = np.histogram(vals, bins=len(buckets))
    #    h = h.cumsum() / h.cumsum().max()
    #
    #    plt.scatter(h, edges)


    plt.hist(vals, int(len(buckets)/10), 
            label=getLable(height, width),
            weights=np.ones_like(vals)/float(len(vals)),
            linewidth=2,
            histtype="bar",
            alpha=.75
        )


    #plt.hist(vals, len(buckets), cumulative=True, weights=np.ones_like(vals)/float(len(vals)),
    #        histtype='step', label=getLable(height, width),
    #        linewidth=2
    #    )        


file_prefix = "./Bytes Read "
file_suffix = " frame_test_1.csv"

title = "Bytes Read From GPU"
xlabel = "Bytes"
ylabel = "Probability"


processFile( 720 ,1280)
processFile( 1080,1920)
processFile( 1440,2560)
processFile( 2160,3840)

plt.title(title, fontsize=16)
plt.xlabel(xlabel, fontsize=14, )
plt.ylabel(ylabel, fontsize=14)
plt.xscale("log")


plt.legend(loc='right', fontsize=12)
plt.show()