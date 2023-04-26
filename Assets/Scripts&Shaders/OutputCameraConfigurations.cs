using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System.Text;
using System.IO;


class OutputCameraConfigurations
{
    public static List<List<double>> matrix4x4To2dList(Matrix4x4 mat){

        List<List<double>> list = new();
        for(int i = 0; i < 4; i++){
            Vector4 row = mat.GetRow(i);
            List<double> tmp = new();
            tmp.Add(row.x);
            tmp.Add(row.y);
            tmp.Add(row.z);
            tmp.Add(row.w);
            list.Add(new List<double>(tmp));
        }

        return list;
    }


    public static void WriteCameraConfigurations(string outputFile, List<Camera> cameras, List<string> filePaths){

        if(cameras.Count != filePaths.Count){
            Debug.LogError("Cameras and Filepaths must have the same length");
            throw new System.Exception();
        }
        if(cameras.Count == 0){
            return;
        }

        string output = "[";
        for(int i = 0; i < filePaths.Count; i++){
            CameraConfig tmp = new CameraConfig(matrix4x4To2dList(cameras[i].projectionMatrix.inverse), matrix4x4To2dList(cameras[i].gameObject.transform.localToWorldMatrix), filePaths[i]);
            output += tmp.ToJson() + ",";
        }
        output = output.Substring(0, output.Length - 1);
        output += "]";

        File.WriteAllText(outputFile, output);

    }
}


[System.Serializable]
class Serializable2dDoubleList{
    [SerializeField]
    public List<SerializableDoubleList> mat;
    public Serializable2dDoubleList(List<SerializableDoubleList> matIn){
        mat = matIn;
    }
}

[System.Serializable]
class SerializableDoubleList{
    [SerializeField]
    public List<double> mat;
    public SerializableDoubleList(List<double> matIn){
        mat = matIn;
    }
}

    
[System.Serializable]
class CameraConfig{
    [SerializeField]
    public List<List<double>> inverseProjectionMatrix;

    [SerializeField]
    public List<List<double>> localCoordinatesToWorldCoordinatesMatrix;

    public string filePrefix;
    

    public CameraConfig(List<List<double>> inverseProjectionMatrixIn, List<List<double>> localCoordinatesToWorldCoordinatesMatrixIn, string filePrefixIn){
        inverseProjectionMatrix = inverseProjectionMatrixIn;
        localCoordinatesToWorldCoordinatesMatrix = localCoordinatesToWorldCoordinatesMatrixIn;
        filePrefix = filePrefixIn;
    }

    public string matrixTojJson(List<List<double>> mat){
        string output = "[";
        foreach(var row in mat){
            output += "[";
            foreach(var val in row){
                output += val + ",";
            }
            output = output.Substring(0, output.Length - 1);            
            output += "],";
        }
        output = output.Substring(0, output.Length - 1);
        output += "]";
        return output;

    }

    public string ToJson(){
        string output = "{";

        output += "\"inverseProjectionMatrix\":" + matrixTojJson(inverseProjectionMatrix) + ",";
        output += "\"localCoordinatestoWorldCoordinatesMatrix\":" + matrixTojJson(localCoordinatesToWorldCoordinatesMatrix) + ",";
        output += "\"filePrefix\":\"" + filePrefix + "\"";
        output += "}"; 



        return output;
    }
};
