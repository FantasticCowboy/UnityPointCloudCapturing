# Reserach Lab Notes

The new plan for computing the delta compression is to add an additional step to the pipeline. The additonal step is to get the number of elements written to the StructuredAppend buffer before I read back from it. Afterwards I will asynchronously read the number of elements written to the buffer.
