import pdb
import matplotlib.pyplot as plt
import numpy as np

def writeStats(stats, test_count):
    for stat in stats:
        with open( f"{stat}_test_{test_count}.csv", "w") as csv:
            csv.write(f"{stat}\n")
            #pdb.set_trace()
            csv.writelines(stats[stat])
    stats = {}
    test_count+=1    

def makeHistogram(stats : dict[str, list[float]]):
    for stat in stats:          
        buckets = {}

        cumsum = 1
        count = 0
        for val in stats[stat]:
            cumsum += float(val)            
            buckets[val] = True
            count +=1
        vals = [float(x) for x in stats[stat]]   

        print(f"\n{stat}'s Statistics")   
        print(f"Cummulative Sum={cumsum}")
        #pdb.set_trace()
        print(f"Average Value={cumsum / len(vals)}")

        plt.hist(vals, len(buckets), cumulative=True, weights=np.ones_like(vals)/float(len(vals)),histtype='step')
        plt.hist( vals , len(buckets), histtype='step', weights=np.ones_like(vals)/float(len(vals) ))


        #plt.yscale("log")
        plt.title(stat)
        plt.xlabel("Time in ms")
        plt.ylabel("Probability")
        plt.show()     



with open("stats.txt") as f:
    stats : dict[str, list[float]] = {}
    test_count = 0
    for line in f:
        if line[0] == "#":
            continue
        if line == "Starting New Test\n":
                        
            #writeStats(stats=stats, test_count=test_count)
            makeHistogram(stats=stats)    

            stats = {}
            test_count+=1
            continue
        #pdb.set_trace()
        stat, val, timestamp, executionId = line.split(",")
        if not stat in stats:
            stats[stat] = []
        stats[stat].append(str(val) + "\n")
    writeStats(stats=stats, test_count=test_count)
    makeHistogram(stats=stats)    
        
