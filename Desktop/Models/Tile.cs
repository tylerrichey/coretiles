using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Desktop.Models
{
    public abstract class Tile
    {
        public Guid Id = Guid.NewGuid();
    }
}
