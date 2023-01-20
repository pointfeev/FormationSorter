﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FormationSorter.Utilities;

internal static class OutputUtils
{
    private static readonly List<string> Outputs = new();

    internal static void DoOutputForException(Exception e)
    {
        StringBuilder output = new();
        int stackDepth = 0;
        while (e is not null)
        {
            if (stackDepth > 10)
                break;
            if (output.Length > 0)
                _ = output.Append("\n\n");
            string[] stackTrace = e.StackTrace?.Split('\n');
            if (stackTrace is not null && stackTrace.Length > 0)
            {
                _ = output.Append(e.GetType() + (e.Message is not null ? ": " + e.Message : ""));
                foreach (string line in stackTrace)
                {
                    int atNum = line.IndexOf("at ", StringComparison.Ordinal);
                    int inNum = line.IndexOf("in ", StringComparison.Ordinal);
                    int fsNum = line.LastIndexOf(@"FormationSorter\", StringComparison.Ordinal);
                    int lineNum = line.LastIndexOf(":line ", StringComparison.Ordinal);
                    if (atNum != -1)
                        _ = output.Append("\n    " + (inNum != -1 ? line.Substring(atNum, inNum - atNum) : line.Substring(atNum)) + (inNum != -1
                            ? "\n        " + (fsNum != -1
                                ? "in " + (lineNum != -1
                                    ? line.Substring(fsNum, lineNum - fsNum) + "\n            on " + line.Substring(lineNum + 1)
                                    : line.Substring(fsNum))
                                : line.Substring(inNum))
                            : null));
                }
            }
            e = e.InnerException;
            stackDepth++;
        }
        string outputString = output + "\n\n"
                                     + "BUG REPORTING: The easiest way to report this error is to snap an image of this message box with Snipping Tool or Lightshot, "
                                     + "upload the image to imgur.com, and paste the link to the image in a new bug report on Nexus Mods (along with any helpful details)."
                                     + "\n\nNOTE: This is not a game crash; press OK to continue playing.";
        if (Outputs.Contains(outputString))
            return;
        Outputs.Add(outputString);
        _ = MessageBox.Show(outputString, "Formation Sorter encountered an exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}