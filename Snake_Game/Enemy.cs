﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Snake_Game
{
    public class Enemy
    {
        public UIElement UiElement { get; set; }
        public Point Position { get; set; }
        public EnemyDirection MoveDirection { get; set; }
    }
}
