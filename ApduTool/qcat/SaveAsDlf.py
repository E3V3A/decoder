#----------------------------------------------------------------------------
#  QCAT 6 Automation Example Script (Python 3.2.2)
#
# 1) takes 2 arguments: input file and output file
# 2) loads the input file
# 3) saves the output file (DLF format)
#
# Copyright (c) 2013 Qualcomm Proprietary
# Export of this technology or software is regulated by the U.S. Government. 
# Diversion contrary to U.S. law prohibited.
#
#----------------------------------------------------------------------------
import sys
import win32com.client

#----------------------------------------------------------------------------
# qcat_process_log: start QCAT instance, open the input log, & save it to 
#                   the specified output log in Dlf format
#----------------------------------------------------------------------------
def qcat_process_log(input, output):

    print("\nStart QCAT Application...\n")    
       
    qcatApp = win32com.client.Dispatch("QCAT6.Application")    
    qcatApp.Visible = 1
    qcatApp.PRev = 6
    qcatApp.Mode = 1 #CDMA_MODE
    
    print("QCAT version:", qcatApp.AppVersion, "\n")
                              
    print("Open Log:", input)
            
    if qcatApp.OpenLog(input) != 1:         
        print("Error:")
        print(qcatApp.LastError)
        exit()

    print("Done\n")

    print("Save Log:", output)

    if qcatApp.SaveAsDLF(output) != 1:
        print("Error:")
        print(qcatApp.LastError)
        exit() 

    print("Done\n")   

#----------------------------------------------------------------------------
# main program: 
#  usage case1 - network path for input & network path for output
#   python SaveAsDLF.py \\qcat-adm2\Dropbox\calldrop.qmdl \\qcat-adm2\Dropbox\calldrop.dlf
#  usage case2 - network path for input & local path for output
#   python SaveAsDLF.py \\qcat-adm2\Dropbox\calldrop.qmdl c:\Temp\calldrop.dlf
#  usage case3 - local path for input & local path for output
#   python SaveAsDLF.py c:\Temp\calldrop.qmdl c:\Temp\calldrop.dlf
#
# Note: to modify this script to pass input/output logs within the script instead
#       of passing them through command-line, like qcat_process_log("input", "output"),
#       then be sure to have network path specified like "\\\\qcat-adm2\Dropbox\calldrop.qmdl",
#       (i.e. qcat_process_log("\\\\qcat-adm2\Dropbox\calldrop.qmdl", "c:\Temp\calldrop.dlf"))  
#----------------------------------------------------------------------------
if __name__ == '__main__':    

    #check Number of arguments
    if len(sys.argv) < 3: #sys.argv[0] is the program ie. script name
        print("\nusage:python SaveAsDlf.py <inputfile> <outputfile>")
        exit()

    #process input/output logs
    qcat_process_log(sys.argv[1], sys.argv[2])
        
