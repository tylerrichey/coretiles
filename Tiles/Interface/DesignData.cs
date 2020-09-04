using System;
using System.Collections.Generic;
using System.Text;

namespace CoreTiles.Tiles
{
    public static class DesignData
    {
        public static LogViewerViewModel LogViewerViewModel { get; } =
            new LogViewerViewModel(
                new List<(DateTime, string)>
                {
                    (DateTime.Now, "Hello this is log"),
                    (DateTime.Now, "hi this is another log"),
                    (DateTime.Now, "bye this is log no more")
                });
    }
}
