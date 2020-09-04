using System;
using System.Collections.Generic;
using System.Text;

namespace CoreTiles.Tiles
{
    public class LogViewerViewModel
    {
        public string LogData { get; }

        public LogViewerViewModel(string logData) => LogData = logData;

        public LogViewerViewModel(List<(DateTime, string)> logData)
        {
            var logs = new StringBuilder();
            logData.Reverse();
            logData.ForEach(l => logs.AppendLine(string.Join(" - ", l.Item1, l.Item2)));
            LogData = logs.ToString();
        }
    }
}
