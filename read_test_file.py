import pdb
import struct

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


# Texture should be in RGBA32 format where each channel is an int8 for a total of 32 bits
# I.E: R = 8 bit channel, G = 8 bit channel, B = 8 bit channel, A = 8 bit chanel
def read_initial_texture(filename : str) -> list[dict[str : int]]:
    b = bytearray()
    ret = list[dict[str : float]]
    ret = []

    with open(filename, "rb") as f:
        for line in f:
            for byte in line:        
                b.append(byte)
    
    for i in range(0,len(b) ,4):
        vals = []
        for j in range(0,4):
            f = bytearray()
            f.append(b[i+j])
            tmp = struct.unpack("B", f)[0]
            vals.append(tmp)
        ret.append({"r" : vals[0], "g" : vals[1], "b" : vals[2] , "a" : vals[3] })

    return ret

t = read_initial_texture("test_file")

pdb.set_trace()