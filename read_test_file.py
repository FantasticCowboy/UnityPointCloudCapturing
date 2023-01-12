import pdb
import struct

b = bytearray()

# opens upo file with name filename and interprets it as an array of floats
# using the following pattern [{val, xpos, ypos, 0}, ..., {val, xpos, ypos, 0}]
def read_delta_encoding(filename : str) -> list[dict[str : float]]:
    b = bytearray()
    ret = []
    with open(filename, "rb") as f :
        for line in f:
            for byte in line:
                b.append(byte)
    for i in range(0,len(b), 4):
        vals = []        
        for _ in range(0,4):
            for j in range(0,4):
                if(i + j < len(b) ):
                    f.append(b[i + j])
            val = struct.unpack("f", f)
            vals.append(val)
        ret.append({"val" : vals[0], "xpos" : vals[1], "ypos" : vals[2]})
    return ret

    
    

with open("test_file", "rb") as f:
    for line in f:
        for byte in line:        
            b.append(byte)

print(len(b))
for i in range(0,len(b), 4):
    f = bytearray()
    f.append(b[i ])

    tmp = struct.unpack("c", f)
    print(tmp[0])
