using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Desktop.Models
{
    public class Tile
    {
        public Guid Id { get; set; }
        public static Tile New() => new Tile { Id = Guid.NewGuid() };

        public TextBox GetControl() => new TextBox { Text = Id.ToString() };
    }
}
