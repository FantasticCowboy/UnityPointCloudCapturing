import pdb
import struct
import os
import pprint
from PIL import Image
import numpy as np
import skvideo.io
import math

def rotate_coordinates(x : int, y : int, angle : float):
    roation_matrix = np.matrix([math.cos(angle), -math.sin(angle)], [math.sin(angle), math.cos(angle)])
    point = np.array([x,y])





# opens upo file with name filename and interprets it as an array of floats
# using the following pattern [{val, xpos, ypos}, ..., {val, xpos, ypos}
def read_delta_encoding(filename : str) -> list[dict[str : float]]:
    b = bytearray()
    ret = []

    with open(filename, "rb") as f :
        for line in f:
            for byte in line:
                b.append(byte)


    # 16 -> four floats
    for i in range(0,len(b), 16):
        vals = []
        # for each float
        for k in range(0,4):
            f = bytearray()
            for j in range(0,4):
                if(i + j < len(b) ):
                    f.append(b[i + k*4 + j])
            val = struct.unpack("f", f)
            vals.append(val)

        ret.append({"val" : float(vals[0][0]), "xpos" : int(vals[1][0]), "ypos" : int(vals[2][0])})


    return ret


# Texture should be in RGBA32 format where each channel is an int8 for a total of 32 bits
# I.E: R = 8 bit channel, G = 8 bit channel, B = 8 bit channel, A = 8 bit chanel
# returned list represents the texture such that the first 4 elements in the list represent the
# bottom left pixel and the last 4 elements in the list represent the top right pixel
def read_initial_texture(filename : str, height : int, width : int, datatype_size : int, elements_per_pixels : int) -> list[list[list[int]]]:
    b = bytearray()
    ret = list[dict[str : float]]
    ret = []

    with open(filename, "rb") as f:
        for line in f:
            for byte in line:        
                b.append(byte)
    
    # TODO: This will break when I change the resolution of the frames 
    # each value is a float, 4 floats in a pixel    
    total_size = datatype_size * elements_per_pixels
    rowLengthInBytes = width * total_size    

    for rowNum in range(0, height):
        row = []
        for i in range(rowNum * rowLengthInBytes, rowNum * rowLengthInBytes + rowLengthInBytes, total_size):
            vals = []
            for j in range(0,total_size,datatype_size):
                f = bytearray()
                #f.append(b[i+j:i+j+datatype_size])
                tmp = struct.unpack("f", b[i+j:i+j+datatype_size])[0]
                vals.append(tmp)
            row.append([vals[0], vals[1],vals[2], vals[3] ])
        ret.append(row)
    
    #ret.reverse()
    return ret


def format_filename(encodingIdentifier : str, file_num : int) -> str:
    return encodingIdentifier + "_" + str(file_num)


def create_empty_frame(height : int, width : int) -> list[list[list[int]]]:
    l = list[list[list[int]]]
    l = []
    for i in range(0, height):
        l.append([])
        for j in range(0,width):
            l[i].append([])
            for _ in range(0,4):

                l[i][j].append(0)
    return l


def recreate_frame(prevFrame : list[list[list[int]]] , curFrameDeltaEncoding : list[dict[str : float]]):
    recreated_frame = create_empty_frame(1080, 1920)    

    # probably computing the wrong values because the read in original frame goes from bottom left to top right
    # whereas I am not sure in what coordinate space the encoded frame values are using!!!!!!
    exploredCoordinataes = {}



    for pixel in curFrameDeltaEncoding:
        if(pixel["val"] == 0):
            continue
        #if(pixel["val"] < 0):
        #    pdb.set_trace()
        recreated_frame[pixel["xpos"]][pixel["ypos"]][0] = ((pixel["val"] ) + prevFrame[pixel["xpos"]][pixel["ypos"]][0])
        recreated_frame[pixel["xpos"]][pixel["ypos"]][1] = ((pixel["val"] ) + prevFrame[pixel["xpos"]][pixel["ypos"]][1])
        recreated_frame[pixel["xpos"]][pixel["ypos"]][2] = ((pixel["val"] ) + prevFrame[pixel["xpos"]][pixel["ypos"]][2])
        recreated_frame[pixel["xpos"]][pixel["ypos"]][3] = ((pixel["val"] ) + prevFrame[pixel["xpos"]][pixel["ypos"]][3])
        exploredCoordinataes[(pixel["xpos"] , pixel["ypos"])] = True

    for x in range(0, len(recreated_frame)):
        for y in range(0, len(recreated_frame[x])):
            if (x,y) not in exploredCoordinataes:
                recreated_frame[x][y] = prevFrame[x][y]

    return recreated_frame


    

# returns a list containing the recreated frames from first frame to last frame 
def recreate_frames(encodingIdentifier : str) -> list[dict[str:int]]:

    prevFrame = read_initial_texture(format_filename(encodingIdentifier=encodingIdentifier, file_num=0), 1080, 1920, 4, 4)
    frames = [prevFrame]
    currentFile = 1
    formatedName = format_filename(encodingIdentifier=encodingIdentifier, file_num=currentFile)   
    while(os.path.exists(formatedName)):
        if(currentFile == 24):
            break
        encoding = read_delta_encoding(formatedName)
        prevFrame = recreate_frame(prevFrame=prevFrame, curFrameDeltaEncoding=encoding)
        frames.append(prevFrame)
        currentFile+=1
        formatedName = format_filename(encodingIdentifier=encodingIdentifier, file_num=currentFile)
    return frames

#x = create_empty_frame()
#
#init = np.array(read_initial_texture("delta_encoding_raw_data/1909729944_0"))
#
#img = Image.fromarray( init.astype(np.uint8), "RGBA")
#
#img.show()

frames = recreate_frames("delta_encoding_raw_data/544838990")

for frame in frames:
    frame.reverse()

frames = np.array(frames) * 255


elements = np.array(frames)
elements = elements.flatten()

for element in elements:
    if(element > 1.0 or element < 0):
        print(f"Found {element}")
        break
    


# frameTwo = np.array(frames[0])
# pdb.set_trace()
# img = Image.fromarray( frameTwo.astype(np.uint8), "RGBA")
# 
# img.show()

skvideo.io.vwrite("./postprocessing/test_video.mov", frames)
