#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public static class FileTypeGenerator
{
    // FileTypeGenerator will generate .cs files for associated column definitions, to allow easy logging of data per-file

    private const string generatedCsPath = "Assets/VERA/FileTypes/GeneratedCode/";

    // Deletes all generated file type cs code
    public static void ClearAllFileTypeCsCode()
    {
        // Get all files in the folder
        string[] files = Directory.GetFiles(generatedCsPath);

        // Delete each file
        foreach (string file in files)
        {
            File.Delete(file);
        }

        AssetDatabase.Refresh();
    }

    // Generates .cs files for every column definition currently in the columns folder
    public static void GenerateAllFileTypesCsCode()
    {
        // Get all items
        VERAColumnDefinition[] columnDefinitions = Resources.LoadAll<VERAColumnDefinition>("");

        // Generate code
        foreach (VERAColumnDefinition columnDefinition in columnDefinitions)
        {
            GenerateFileTypeCsCode(columnDefinition, false);
        }

        AssetDatabase.Refresh();
    }

    // Generates .cs file for a given column definition
    public static void GenerateFileTypeCsCode(VERAColumnDefinition columnDefinition, bool refreshOnFinish)
    {
        string fileName = columnDefinition.fileType.name;
        string filePath = generatedCsPath + "VERAFile_" + fileName + ".cs";

        // Use StringBuilder to create the code
        StringBuilder sb = new StringBuilder();

        // Build the class; for example, for a file named "PlayerTransform", would be VERAFile_PlayerTransform
        sb.AppendLine("#if VERAFile_" + fileName);
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using System;");
        sb.AppendLine("");
        sb.AppendLine("public static class VERAFile_" + fileName);
        sb.AppendLine("{");
        sb.AppendLine("\t");
        sb.AppendLine("\tprivate const string fileName = \"" + fileName + "\";");
        sb.AppendLine("\t");

        // Begin building the logging function defintion
        // Will always be in a specific format. For example, for a file type "PlayerTransform" with columns
        //   "Timestamp", "EventId", "Message", "Transform", would have the definition:
        //   public static void CreateEntry(int EventId, string Message, Transform Transform)
        string functionDefinition = "\tpublic static void CreateCsvEntry(";
        List<string> parameterNames = new List<string>();

        // Add eventId
        functionDefinition += "int eventId";
        if (columnDefinition.columns.Count > 2)
        {
            functionDefinition += ", ";
        }

        // Loop through each column (skipping timestamp and eventId) to add a corresponding column parameter
        for (int i = 2; i < columnDefinition.columns.Count; i++)
        {
            // Add the parameter type
            switch (columnDefinition.columns[i].type)
            {
                case VERAColumnDefinition.DataType.Number:
                    functionDefinition += "int ";
                    break;
                case VERAColumnDefinition.DataType.String:
                    functionDefinition += "string ";
                    break;
                case VERAColumnDefinition.DataType.Date:
                    functionDefinition += "DateTime ";
                    break;
                case VERAColumnDefinition.DataType.Transform:
                    functionDefinition += "Transform ";
                    break;
            }

            // Add the parameter name
            string parameterName = columnDefinition.columns[i].name;
            parameterName = parameterName.Replace(" ", "");
            parameterNames.Add(parameterName);
            functionDefinition += parameterName;

            if (i != columnDefinition.columns.Count - 1)
            {
                functionDefinition += ", ";
            }
        }

        functionDefinition += ")";
        sb.AppendLine(functionDefinition);
        sb.AppendLine("\t{");

        // Add the actual function code, which calls the VERALogger
        string loggerCall = "\t\tVERALogger.Instance.CreateCsvEntry(fileName, eventId";
        if (parameterNames.Count > 0)
            loggerCall += ", ";

        for (int i = 0; i < parameterNames.Count; i++)
        {
            loggerCall += parameterNames[i];
            if (i != parameterNames.Count - 1)
            {
                loggerCall += ", ";
            }
        }
        loggerCall += ");";
        sb.AppendLine(loggerCall);
        sb.AppendLine("\t}");
        sb.AppendLine("\t");

        // Add the function to submit the file
        sb.AppendLine("\tpublic static void SubmitCsvFile(bool flushOnSubmit = false)");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\tVERALogger.Instance.SubmitCsvFile(fileName, flushOnSubmit);");
        sb.AppendLine("\t}");
        sb.AppendLine("}");
        sb.AppendLine("#endif");

        string newContents = sb.ToString();
        // if the file exists, read it and compare (normalize line endings to '\n')
        // This way we can prevent a recompile if possible - if text matches, we don't need to write
        if (File.Exists(filePath))
        {
            var oldContents = File.ReadAllText(filePath, Encoding.UTF8);
            string oldNormalized = oldContents.Replace("\r\n", "\n");
            string newNormalized = newContents.Replace("\r\n", "\n");

            if (oldNormalized == newNormalized)
            {
                // no changes, skip write and recompile
                return;
            }
        }

        // Write the file
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

        // Force Unity to refresh so the new/modified code is recognized
        if (refreshOnFinish)
        {
            AssetDatabase.Refresh();
        }
    }
}
#endif